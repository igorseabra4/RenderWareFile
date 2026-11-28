using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

/// <summary>
/// Decodes the mipmaps of a parsed native texture to 32 bit BGRA.
/// Supports PC (D3D8/D3D9), PS2, GameCube GX and Xbox rasters.
/// </summary>
public static class TextureDecoder
{
    /// <summary>
    /// Decodes a native texture to BGRA mipmaps.
    /// </summary>
    public static byte[][] DecodeAll(TextureNative_0015 texture, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (texture?.textureNativeStruct == null)
            return null;

        byte[][] mipmaps;
        if (texture.textureNativeStruct.platformType == TexturePlatformID.PS2)
        {
            var raster = texture.PS2RasterFormat;
            if (raster == null || raster.Width > 16384 || raster.Height > 16384)
                return null;
            width = (int)raster.Width;
            height = (int)raster.Height;
            mipmaps = Ps2TextureDecoder.DecodeSupported(raster);
        }
        else
        {
            width = texture.textureNativeStruct.width;
            height = texture.textureNativeStruct.height;
            mipmaps = DecodeAll(texture.textureNativeStruct);
        }

        byte[] pixels = mipmaps != null && mipmaps.Length != 0 ? mipmaps[0] : null;
        if (pixels == null || width <= 0 || height <= 0 || pixels.LongLength != (long)width * height * 4)
            return null;
        if (mipmaps.Length > 1 + (int)Math.Log(Math.Max(width, height), 2))
            return null;
        return mipmaps;
    }

    /// <summary>Decodes only the base level to BGRA.</summary>
    public static byte[] DecodeBase(TextureNative_0015 texture, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (texture?.textureNativeStruct == null)
            return null;

        byte[] pixels;
        if (texture.textureNativeStruct.platformType == TexturePlatformID.PS2)
        {
            var raster = texture.PS2RasterFormat;
            if (raster == null || raster.Width > 16384 || raster.Height > 16384)
                return null;
            width = (int)raster.Width;
            height = (int)raster.Height;
            pixels = Ps2TextureDecoder.DecodeSupported(raster, true)?[0];
        }
        else
        {
            width = texture.textureNativeStruct.width;
            height = texture.textureNativeStruct.height;
            pixels = DecodeMip(texture.textureNativeStruct);
        }

        return pixels != null && width > 0 && height > 0 && pixels.LongLength == (long)width * height * 4
            ? pixels : null;
    }

    public static bool CanDecode(TextureNativeStruct_0001 texture)
    {
        if (texture?.mipMaps == null || texture.mipMaps.Length == 0 || texture.width <= 0 || texture.height <= 0)
            return false;

        switch (texture.platformType)
        {
            case TexturePlatformID.PCD3D8:
            case TexturePlatformID.PCD3D9:
                return true;
            case TexturePlatformID.Xbox:
                return texture.compression != 13 && texture.compression != 16;
            case TexturePlatformID.GameCube:
                return GameCubeTextureHandler.TryGetBlock(texture.type, out _, out _, out _);
            default:
                return false;
        }
    }

    /// <summary>Decodes every mipmap.</summary>
    public static byte[][] DecodeAll(TextureNativeStruct_0001 texture)
    {
        if (!CanDecode(texture))
            return null;

        try
        {
            var result = new System.Collections.Generic.List<byte[]>(texture.mipMaps.Length);
            for (int i = 0; i < texture.mipMaps.Length; i++)
            {
                byte[] data = texture.mipMaps[i].data;
                if (texture.platformType == TexturePlatformID.Xbox && texture.compression >= 12 && texture.compression <= 16 &&
                    (data == null || data.Length == 0))
                {
                    for (int j = i + 1; j < texture.mipMaps.Length; j++)
                        if (texture.mipMaps[j].data != null && texture.mipMaps[j].data.Length != 0)
                            return null;
                    break;
                }

                int w = Math.Max(1, texture.width >> i);
                int h = Math.Max(1, texture.height >> i);
                byte[] decoded = texture.platformType == TexturePlatformID.GameCube ?
                    DecodeGameCube(texture, data, w, h) :
                    DecodeD3D(texture, data, w, h);
                if (decoded == null)
                    return null;
                result.Add(decoded);
            }
            return result.ToArray();
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    public static byte[] DecodeMip(TextureNativeStruct_0001 texture, int level = 0)
    {
        if (!CanDecode(texture))
            return null;
        if (level < 0 || level >= texture.mipMaps.Length)
            throw new ArgumentOutOfRangeException(nameof(level));

        try
        {
            byte[] data = texture.mipMaps[level].data;
            if (texture.platformType == TexturePlatformID.Xbox && texture.compression >= 12 && texture.compression <= 16 &&
                (data == null || data.Length == 0))
                return null;
            int width = Math.Max(1, texture.width >> level);
            int height = Math.Max(1, texture.height >> level);
            return texture.platformType == TexturePlatformID.GameCube
                ? DecodeGameCube(texture, data, width, height)
                : DecodeD3D(texture, data, width, height);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    #region D3D

    private static byte[] DecodeD3D(TextureNativeStruct_0001 t, byte[] data, int w, int h)
    {
        byte[] output = new byte[w * h * 4];

        if (t.compression != 0)
        {
            int compression = t.compression;
            if (t.platformType == TexturePlatformID.Xbox)
            {
                switch (compression)
                {
                    case 12: compression = 1; break; // DXT1
                    case 14: compression = 3; break; // DXT3
                    case 15: compression = 5; break; // DXT5
                }
            }
            if (compression != 1 && compression != 3 && compression != 5)
                return null;
            DecodeDxt(data, w, h, compression, output);
            return output;
        }

        if (t.platformType == TexturePlatformID.Xbox)
        {
            bool palettized = (t.rasterFormatFlags & (TextureRasterFormat.RASTER_PAL4 | TextureRasterFormat.RASTER_PAL8)) != 0;
            int bytesPerTexel = palettized ? 1 : t.bitDepth / 8;
            data = XboxSwizzle.Unswizzle(data, w, h, bytesPerTexel);
            if (data == null)
                return null;
        }

        if ((t.rasterFormatFlags & (TextureRasterFormat.RASTER_PAL4 | TextureRasterFormat.RASTER_PAL8)) != 0)
        {
            if (t.palette == null || data.Length < w * h)
                return null;
            bool pal4 = (t.rasterFormatFlags & TextureRasterFormat.RASTER_PAL4) != 0;
            int stride = w;
            int alignedStride = (w + 3) & ~3;
            if (t.platformType != TexturePlatformID.Xbox && data.Length == alignedStride * h)
                stride = alignedStride;
            for (int i = 0; i < w * h; i++)
            {
                byte index = data[i / w * stride + i % w];
                Color c = t.palette[pal4 ? index & 0x0F : index];
                output[i * 4] = c.B;
                output[i * 4 + 1] = c.G;
                output[i * 4 + 2] = c.R;
                output[i * 4 + 3] = ((int)t.rasterFormatFlags & 0x0F00) == (int)TextureRasterFormat.RASTER_C888 ? (byte)255 : c.A;
            }
            return output;
        }

        switch ((TextureRasterFormat)((int)t.rasterFormatFlags & 0x0F00))
        {
            case TextureRasterFormat.RASTER_C8888:
                if (data.Length < w * h * 4) return null;
                Array.Copy(data, output, w * h * 4);
                return output;
            case TextureRasterFormat.RASTER_C888:
                if (data.Length < w * h * 4) return null;
                Array.Copy(data, output, w * h * 4);
                for (int i = 0; i < w * h; i++)
                    output[i * 4 + 3] = 0xFF;
                return output;
            case TextureRasterFormat.RASTER_LUM8:
                if (data.Length < w * h) return null;
                for (int i = 0; i < w * h; i++)
                {
                    output[i * 4] = output[i * 4 + 1] = output[i * 4 + 2] = data[i];
                    output[i * 4 + 3] = 0xFF;
                }
                return output;
            case TextureRasterFormat.RASTER_C565:
            case TextureRasterFormat.RASTER_C1555:
            case TextureRasterFormat.RASTER_C555:
            case TextureRasterFormat.RASTER_C4444:
                if (data.Length < w * h * 2) return null;
                for (int i = 0; i < w * h; i++)
                    Write16(output, i, (ushort)(data[i * 2] | (data[i * 2 + 1] << 8)),
                        (TextureRasterFormat)((int)t.rasterFormatFlags & 0x0F00));
                return output;
            default:
                return null;
        }
    }

    private static void Write16(byte[] output, int pixel, ushort v, TextureRasterFormat format)
    {
        int r, g, b, a = 255;
        switch (format)
        {
            case TextureRasterFormat.RASTER_C565:
                r = Expand(v >> 11 & 0x1F, 5); g = Expand(v >> 5 & 0x3F, 6); b = Expand(v & 0x1F, 5);
                break;
            case TextureRasterFormat.RASTER_C4444:
                a = (v >> 12 & 0xF) * 0x11; r = (v >> 8 & 0xF) * 0x11; g = (v >> 4 & 0xF) * 0x11; b = (v & 0xF) * 0x11;
                break;
            case TextureRasterFormat.RASTER_C1555:
                a = (v & 0x8000) != 0 ? 255 : 0;
                r = Expand(v >> 10 & 0x1F, 5); g = Expand(v >> 5 & 0x1F, 5); b = Expand(v & 0x1F, 5);
                break;
            default:
                r = Expand(v >> 10 & 0x1F, 5); g = Expand(v >> 5 & 0x1F, 5); b = Expand(v & 0x1F, 5);
                break;
        }
        SetPixel(output, pixel, r, g, b, a);
    }

    #endregion

    #region DXT

    private static void DecodeDxt(byte[] data, int w, int h, int compression, byte[] output)
    {
        int blockBytes = compression == 1 ? 8 : 16;
        int blocksX = (w + 3) / 4, blocksY = (h + 3) / 4;
        int[] colors = new int[16];
        byte[] alpha = new byte[16];

        for (int by = 0; by < blocksY; by++)
            for (int bx = 0; bx < blocksX; bx++)
            {
                int offset = (by * blocksX + bx) * blockBytes;
                if (offset + blockBytes > data.Length)
                    throw new IndexOutOfRangeException();

                int colorOffset = offset + (compression == 1 ? 0 : 8);
                ushort c0 = (ushort)(data[colorOffset] | (data[colorOffset + 1] << 8));
                ushort c1 = (ushort)(data[colorOffset + 2] | (data[colorOffset + 3] << 8));
                BuildPalette(c0, c1, compression == 1, colors);
                uint indices = BitConverter.ToUInt32(data, colorOffset + 4);

                for (int i = 0; i < 16; i++)
                    alpha[i] = 255;

                if (compression == 3)
                    for (int i = 0; i < 16; i++)
                        alpha[i] = (byte)((data[offset + i / 2] >> (i % 2 * 4) & 0xF) * 0x11);
                else if (compression == 5)
                    DecodeDxt5Alpha(data, offset, alpha);

                for (int py = 0; py < 4; py++)
                    for (int px = 0; px < 4; px++)
                    {
                        int x = bx * 4 + px, y = by * 4 + py;
                        if (x >= w || y >= h) continue;
                        int i = py * 4 + px;
                        int color = colors[(int)(indices >> (i * 2) & 3)];
                        int a = compression == 1 ? color >> 24 & 0xFF : alpha[i];
                        SetPixel(output, y * w + x, color >> 16 & 0xFF, color >> 8 & 0xFF, color & 0xFF, a);
                    }
            }
    }

    private static void DecodeDxt5Alpha(byte[] data, int offset, byte[] alpha)
    {
        int a0 = data[offset], a1 = data[offset + 1];
        int[] table = new int[8];
        table[0] = a0; table[1] = a1;
        if (a0 > a1)
            for (int i = 1; i < 7; i++) table[i + 1] = ((7 - i) * a0 + i * a1) / 7;
        else
        {
            for (int i = 1; i < 5; i++) table[i + 1] = ((5 - i) * a0 + i * a1) / 5;
            table[6] = 0; table[7] = 255;
        }

        ulong bits = 0;
        for (int i = 0; i < 6; i++)
            bits |= (ulong)data[offset + 2 + i] << (8 * i);
        for (int i = 0; i < 16; i++)
            alpha[i] = (byte)table[(int)(bits >> (i * 3) & 7)];
    }

    /// <summary>Builds the 4 colors of a DXT color block as 0xAARRGGBB.</summary>
    internal static void BuildPalette(ushort c0, ushort c1, bool dxt1, int[] colors)
    {
        int r0 = Expand(c0 >> 11 & 0x1F, 5), g0 = Expand(c0 >> 5 & 0x3F, 6), b0 = Expand(c0 & 0x1F, 5);
        int r1 = Expand(c1 >> 11 & 0x1F, 5), g1 = Expand(c1 >> 5 & 0x3F, 6), b1 = Expand(c1 & 0x1F, 5);

        colors[0] = Pack(r0, g0, b0, 255);
        colors[1] = Pack(r1, g1, b1, 255);
        if (c0 > c1 || !dxt1)
        {
            colors[2] = Pack((2 * r0 + r1) / 3, (2 * g0 + g1) / 3, (2 * b0 + b1) / 3, 255);
            colors[3] = Pack((r0 + 2 * r1) / 3, (g0 + 2 * g1) / 3, (b0 + 2 * b1) / 3, 255);
        }
        else
        {
            colors[2] = Pack((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 255);
            colors[3] = 0;
        }
    }

    #endregion

    #region GameCube

    private static byte[] DecodeGameCube(TextureNativeStruct_0001 t, byte[] data, int w, int h)
    {
        if (!GameCubeTextureHandler.TryGetBlock(t.type, out int bw, out int bh, out int blockBytes))
            return null;

        int blocksX = (w + bw - 1) / bw, blocksY = (h + bh - 1) / bh;
        if (data.Length < blocksX * blocksY * blockBytes)
            return null;

        Color[] palette = null;
        bool pal4 = (t.rasterFormatFlags & TextureRasterFormat.RASTER_PAL4) != 0;
        bool pal8 = (t.rasterFormatFlags & TextureRasterFormat.RASTER_PAL8) != 0;
        if (pal4 || pal8 || t.type == GameCubeTextureHandler.CI4 || t.type == GameCubeTextureHandler.CI8)
        {
            if (t.gameCubePalette == null) return null;
            palette = new Color[t.gameCubePalette.Length / 2];
            for (int i = 0; i < palette.Length; i++)
            {
                int value = t.gameCubePalette[i * 2] << 8 | t.gameCubePalette[i * 2 + 1];
                int p;
                TextureRasterFormat colorFormat = (TextureRasterFormat)((int)t.rasterFormatFlags & 0x0F00);
                if (t.gameCubePalettePixelFormat >= 0)
                {
                    switch (t.gameCubePalettePixelFormat)
                    {
                        case 0: // Luminance-alpha 8:8
                            p = Pack(t.gameCubePalette[i * 2 + 1], t.gameCubePalette[i * 2 + 1],
                                t.gameCubePalette[i * 2 + 1], t.gameCubePalette[i * 2]);
                            break;
                        case 1: // RGB565
                            p = Pack(Expand(value >> 11 & 0x1F, 5), Expand(value >> 5 & 0x3F, 6), Expand(value & 0x1F, 5), 255);
                            break;
                        case 2: // RGB5A3
                            p = Rgb5A3(value);
                            break;
                        default:
                            return null;
                    }
                }
                else if (colorFormat == TextureRasterFormat.RASTER_C565)
                    p = Pack(Expand(value >> 11 & 0x1F, 5), Expand(value >> 5 & 0x3F, 6), Expand(value & 0x1F, 5), 255);
                else if (colorFormat == TextureRasterFormat.RASTER_C4444)
                    p = Pack((value >> 8 & 0xF) * 0x11, (value >> 4 & 0xF) * 0x11,
                        (value & 0xF) * 0x11, (value >> 12 & 0xF) * 0x11);
                else
                    p = Rgb5A3(value);
                palette[i] = new Color((byte)(p >> 16 & 0xFF), (byte)(p >> 8 & 0xFF), (byte)(p & 0xFF), (byte)(p >> 24 & 0xFF));
            }
        }

        byte[] output = new byte[w * h * 4];
        int[] colors = new int[4];

        for (int by = 0; by < blocksY; by++)
            for (int bx = 0; bx < blocksX; bx++)
            {
                int block = (by * blocksX + bx) * blockBytes;

                if (t.type == 0xE)
                {
                    for (int sub = 0; sub < 4; sub++)
                    {
                        int so = block + sub * 8;
                        BuildPalette((ushort)(data[so] << 8 | data[so + 1]), (ushort)(data[so + 2] << 8 | data[so + 3]), true, colors);
                        for (int py = 0; py < 4; py++)
                            for (int px = 0; px < 4; px++)
                            {
                                int x = bx * 8 + sub % 2 * 4 + px, y = by * 8 + sub / 2 * 4 + py;
                                if (x >= w || y >= h) continue;
                                int color = colors[data[so + 4 + py] >> (6 - px * 2) & 3];
                                SetPixel(output, y * w + x, color >> 16 & 0xFF, color >> 8 & 0xFF, color & 0xFF, color >> 24 & 0xFF);
                            }
                    }
                    continue;
                }

                for (int py = 0; py < bh; py++)
                    for (int px = 0; px < bw; px++)
                    {
                        int x = bx * bw + px, y = by * bh + py;
                        if (x >= w || y >= h) continue;
                        int i = py * bw + px;
                        int color;

                        switch (t.type)
                        {
                            case 0x0: // I4
                                {
                                    int v = (data[block + i / 2] >> (i % 2 == 0 ? 4 : 0) & 0xF) * 0x11;
                                    color = Pack(v, v, v, v);
                                    break;
                                }
                            case 0x1: // I8 (also used as a PAL8 index stream by RW)
                                if (pal8)
                                {
                                    Color c = palette[Math.Min((int)data[block + i], palette.Length - 1)];
                                    color = Pack(c.R, c.G, c.B, c.A);
                                }
                                else
                                    color = Pack(data[block + i], data[block + i], data[block + i], data[block + i]);
                                break;
                            case 0x2: // IA4, also used as a PAL8 index stream by RW
                                {
                                    if (pal8)
                                    {
                                        Color c = palette[Math.Min((int)data[block + i], palette.Length - 1)];
                                        color = Pack(c.R, c.G, c.B, c.A);
                                    }
                                    else
                                    {
                                        int v = (data[block + i] & 0xF) * 0x11, a = (data[block + i] >> 4) * 0x11;
                                        color = Pack(v, v, v, a);
                                    }
                                    break;
                                }
                            case 0x3: // IA8
                                color = Pack(data[block + i * 2 + 1], data[block + i * 2 + 1], data[block + i * 2 + 1], data[block + i * 2]);
                                break;
                            case 0x4: // RGB565
                                {
                                    int v = data[block + i * 2] << 8 | data[block + i * 2 + 1];
                                    color = Pack(Expand(v & 0x1F, 5), Expand(v >> 5 & 0x3F, 6), Expand(v >> 11 & 0x1F, 5), 255);
                                    break;
                                }
                            case 0x5: // RGB5A3
                                color = Rgb5A3(data[block + i * 2] << 8 | data[block + i * 2 + 1]);
                                break;
                            case 0x6: // RGBA8
                                color = Pack(data[block + i * 2 + 1], data[block + 32 + i * 2], data[block + 32 + i * 2 + 1], data[block + i * 2]);
                                break;
                            case GameCubeTextureHandler.CI4:
                                {
                                    int index = data[block + i / 2] >> (i % 2 == 0 ? 4 : 0) & 0xF;
                                    Color c = palette[Math.Min(index, palette.Length - 1)];
                                    color = Pack(c.R, c.G, c.B, c.A);
                                    break;
                                }
                            case GameCubeTextureHandler.CI8:
                                {
                                    Color c = palette[Math.Min((int)data[block + i], palette.Length - 1)];
                                    color = Pack(c.R, c.G, c.B, c.A);
                                    break;
                                }
                            default:
                                return null;
                        }

                        SetPixel(output, y * w + x, color >> 16 & 0xFF, color >> 8 & 0xFF, color & 0xFF, color >> 24 & 0xFF);
                    }
            }

        return output;
    }

    private static int Rgb5A3(int v)
    {
        if ((v & 0x8000) != 0)
            return Pack(Expand(v >> 10 & 0x1F, 5), Expand(v >> 5 & 0x1F, 5), Expand(v & 0x1F, 5), 255);
        return Pack((v >> 8 & 0xF) * 0x11, (v >> 4 & 0xF) * 0x11, (v & 0xF) * 0x11, Expand(v >> 12 & 0x7, 3));
    }

    #endregion

    private static int Expand(int value, int bits) =>
        bits == 5 ? (value << 3) | (value >> 2) :
        bits == 6 ? (value << 2) | (value >> 4) :
        (value << 5) | (value << 2) | (value >> 1);

    private static int Pack(int r, int g, int b, int a) => (a << 24) | (r << 16) | (g << 8) | b;

    private static void SetPixel(byte[] output, int pixel, int r, int g, int b, int a)
    {
        output[pixel * 4] = (byte)b;
        output[pixel * 4 + 1] = (byte)g;
        output[pixel * 4 + 2] = (byte)r;
        output[pixel * 4 + 3] = (byte)a;
    }
}
