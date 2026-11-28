using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

public class Ps2DecodedMip
{
    public int Width;
    public int Height;
    /// <summary>BGRA, 4 bytes per pixel. Alpha is in PS2 range (0x80 = opaque) scaled to 0-255.</summary>
    public byte[] Bgra;
}

/// <summary>Decodes PS2 GS raster data (linear PSMCT32/24/16, PSMT8, PSMT4) to BGRA.</summary>
public static class Ps2TextureDecoder
{
    internal static byte[][] DecodeSupported(Ps2TextureRasterFormatStruct_0001 raster, bool baseOnly = false)
    {
        if (raster == null || raster.IsSwizzled || !raster.HasTexelHeaders ||
            raster.Width == 0 || raster.Height == 0 || raster.Width > 16384 || raster.Height > 16384)
            return null;

        int width = (int)raster.Width, height = (int)raster.Height;
        try
        {
            var uploads = Ps2GifParser.ParseView(raster.TexelDataView);
            var palettes = raster.Depth == 4 || raster.Depth == 8
                ? Ps2GifParser.ParseView(raster.PaletteDataView) : null;
            var indexed = palettes != null ? DecodeIndexedTransfers(raster, uploads, palettes, baseOnly) : null;
            if (indexed != null)
                return indexed.Select(m => m.Bgra).ToArray();

            if (raster.SkyRasterVersion == 2 &&
                (raster.Depth == 8 || (raster.Depth == 4 && raster.renderWareVersion.PackedVersion > 0x31000)))
                return null;

            if (uploads.Count == 0 || uploads[0].Width != width || uploads[0].Height != height ||
                uploads[0].TrxPos != 0)
                return null;
            int psm = (int)((raster.TEX0 >> 20) & 0x3f);
            int expectedPsm = raster.Depth switch { 32 => 0, 24 => 1, 16 => 2, 8 => 19, 4 => 20, _ => -1 };
            if (psm != expectedPsm || (uploads[0].HasBitBltBuf && uploads[0].DestinationFormat != psm))
                return null;
            long bytes = raster.Depth switch
            {
                32 => (long)width * height * 4,
                24 => (long)width * height * 4,
                16 => (long)width * height * 2,
                8 => (long)width * height,
                4 => ((long)width * height + 1) / 2,
                _ => long.MaxValue
            };
            if (uploads[0].DataView.Length < bytes)
                return null;
            if (raster.Depth <= 8)
            {
                if (((raster.TEX0 >> 51) & 0x3ff) != 0)
                    return null;
                if (palettes.Count == 0 ||
                    palettes[0].DataView.Length < (raster.Depth == 8 ? 1024 : 64))
                    return null;
            }
            var decoded = DecodeLinear(raster, uploads, palettes, baseOnly);
            if (decoded.Count == 0)
                return null;
            if (decoded.Select((m, i) => m.Width == Math.Max(1, width >> i) &&
                    m.Height == Math.Max(1, height >> i) &&
                    m.Bgra.LongLength == (long)m.Width * m.Height * 4).All(valid => valid))
                return decoded.Select(m => m.Bgra).ToArray();
            return null;
        }
        catch (NotSupportedException) { return null; }
        catch (InvalidDataException) { return null; }
        catch (ArgumentException) { return null; }
        catch (IndexOutOfRangeException) { return null; }
    }

    private static bool CanDecodeIndexedTransfers(Ps2TextureRasterFormatStruct_0001 raster)
    {
        if (raster == null || raster.IsSwizzled || !raster.HasTexelHeaders || raster.SkyRasterVersion != 2 ||
            (raster.Depth != 4 && raster.Depth != 8) || raster.Width < 4 || raster.Height < 4 ||
            raster.Width > 16384 || raster.Height > 16384 ||
            (raster.Width & (raster.Width - 1)) != 0 || (raster.Height & (raster.Height - 1)) != 0 ||
            (raster.FullRasterFormat & 0x7f00) != (raster.Depth == 4 ? 0x4500u : 0x2500u) ||
            ((raster.TEX0 >> 20) & 0x3f) != (raster.Depth == 4 ? 20u : 19u) ||
            ((raster.TEX0 >> 51) & 0x3ff) != 0 ||
            (raster.Depth == 4 && raster.renderWareVersion.PackedVersion <= 0x31000))
            return false;
        return true;
    }

    private static List<Ps2DecodedMip> DecodeIndexedTransfers(Ps2TextureRasterFormatStruct_0001 raster,
        List<Ps2GifUpload> uploads, List<Ps2GifUpload> palettes, bool baseOnly)
    {
        if (!CanDecodeIndexedTransfers(raster))
            return null;
        if (uploads.Count == 0 || uploads.Count > 1 + (int)Math.Log(Math.Max(raster.Width, raster.Height), 2) ||
            uploads[0].TrxPos != 0 || palettes.Count == 0 ||
            palettes[0].DataView.Length < (raster.Depth == 8 ? 1024 : 64))
            return null;

        int depth = (int)raster.Depth;
        int[] palette = ReadPalette(palettes, depth);
        var result = new List<Ps2DecodedMip>();
        for (int level = 0; level < uploads.Count; level++)
        {
            int width = Math.Max(1, (int)raster.Width >> level);
            int height = Math.Max(1, (int)raster.Height >> level);
            int transferWidth = Math.Max(width, depth == 8 ? 16 : 32);
            int transferHeight = Math.Max(height, 4);
            var upload = uploads[level];
            int expectedFormat = depth == 8 ? 0 : 2;
            if (upload.Width != transferWidth / 2 || upload.Height != transferHeight / 2 ||
                (upload.HasBitBltBuf && upload.DestinationFormat != expectedFormat) ||
                upload.DataView.Length < (long)transferWidth * transferHeight * depth / 8)
                return null;

            if (baseOnly && level != 0)
                continue;

            var data = upload.DataView.Span;
            int logWidth = 0;
            for (int n = transferWidth; n > 1; n >>= 1) logWidth++;
            int mask = (1 << (logWidth + 2)) - 1;
            byte[] bgra = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int sx = x ^ ((((y >> 1) ^ (y >> 2)) & 1) << 2);
                int nx = (sx & 7) | ((sx >> 1) & ~7);
                int ny = (y & 1) | ((y >> 1) & ~1);
                int offset = (((y >> 1) & 1) | (((sx >> 3) & 1) << 1) |
                    (nx << 2) | (ny << (logWidth + 1))) & mask;
                int source = (y & ~3) * transferWidth + offset;
                int index = depth == 8 ? data[source] :
                    (data[source / 2] >> ((source & 1) * 4)) & 15;
                int color = palette[index], pixel = (y * width + x) * 4;
                bgra[pixel] = (byte)color;
                bgra[pixel + 1] = (byte)(color >> 8);
                bgra[pixel + 2] = (byte)(color >> 16);
                bgra[pixel + 3] = (byte)(color >> 24);
            }
            result.Add(new Ps2DecodedMip { Width = width, Height = height, Bgra = bgra });
        }
        return result;
    }

    private static List<Ps2DecodedMip> DecodeLinear(Ps2TextureRasterFormatStruct_0001 raster,
        List<Ps2GifUpload> uploads, List<Ps2GifUpload> palettes, bool baseOnly)
    {
        int depth = (int)raster.Depth;
        int[] palette = depth <= 8 ? ReadPalette(palettes, depth) : null;
        var result = new List<Ps2DecodedMip>();
        foreach (Ps2GifUpload upload in uploads)
        {
            int w = upload.Width, h = upload.Height;
            long pixels = (long)w * h;
            long bytes = depth switch
            {
                32 => pixels * 4,
                24 => pixels * 4,
                16 => pixels * 2,
                8 => pixels,
                4 => (pixels + 1) / 2,
                _ => throw new NotSupportedException($"PS2 depth {depth} is not supported.")
            };
            if (upload.DataView.Length < bytes)
                throw new InvalidDataException("Truncated PS2 texture upload.");
            if (baseOnly && result.Count != 0)
                continue;
            byte[] bgra = new byte[w * h * 4];
            var d = upload.DataView.Span;

            for (int i = 0; i < w * h; i++)
            {
                int argb;
                switch (depth)
                {
                    case 32:
                        argb = Pack(d[i * 4], d[i * 4 + 1], d[i * 4 + 2], Alpha(d[i * 4 + 3]));
                        break;
                    case 24:
                        argb = Pack(d[i * 4], d[i * 4 + 1], d[i * 4 + 2], 255);
                        break;
                    case 16:
                        int v = d[i * 2] | d[i * 2 + 1] << 8;
                        argb = Pack((v & 0x1F) * 255 / 31, (v >> 5 & 0x1F) * 255 / 31, (v >> 10 & 0x1F) * 255 / 31, (v & 0x8000) != 0 ? 255 : 0);
                        break;
                    case 8:
                        argb = palette[d[i]];
                        break;
                    case 4:
                        argb = palette[(d[i / 2] >> (i % 2 * 4)) & 0xF];
                        break;
                    default:
                        throw new NotSupportedException($"PS2 depth {depth} is not supported.");
                }
                bgra[i * 4] = (byte)argb;
                bgra[i * 4 + 1] = (byte)(argb >> 8);
                bgra[i * 4 + 2] = (byte)(argb >> 16);
                bgra[i * 4 + 3] = (byte)(argb >> 24);
            }

            result.Add(new Ps2DecodedMip { Width = w, Height = h, Bgra = bgra });
        }

        return result;
    }

    private static int[] ReadPalette(List<Ps2GifUpload> uploads, int depth)
    {
        int count = depth == 8 ? 256 : 16;
        var palette = new int[count];

        if (uploads.Count == 0)
            throw new InvalidOperationException("PS2 palette data not found.");

        var d = uploads[0].DataView.Span;
        for (int i = 0; i < count && i * 4 + 3 < d.Length; i++)
        {
            // 8 bit palettes are stored in CSM1 order: bits 3 and 4 of the index are swapped
            int index = depth == 8 ? (i & ~0x18) | (i & 0x08) << 1 | (i & 0x10) >> 1 : i;
            palette[index] = Pack(d[i * 4], d[i * 4 + 1], d[i * 4 + 2], Alpha(d[i * 4 + 3]));
        }
        return palette;
    }

    private static int Alpha(int a) => Math.Min(255, a * 255 / 128);

    private static int Pack(int r, int g, int b, int a) => a << 24 | r << 16 | g << 8 | b;
}
