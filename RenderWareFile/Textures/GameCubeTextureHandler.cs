using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

/// <summary>GameCube layout: big endian, GX formats, palette (CI4/CI8) then the mipmaps back to back.</summary>
internal class GameCubeTextureHandler : TexturePlatformHandler
{
    public const byte CI4 = 0x8;
    public const byte CI8 = 0x9;

    public static int GetPaletteSize(byte gxFormat, TextureRasterFormat rasterFormat)
    {
        if ((rasterFormat & TextureRasterFormat.RASTER_PAL4) != 0 || gxFormat == CI4)
            return 32;
        if ((rasterFormat & TextureRasterFormat.RASTER_PAL8) != 0 || gxFormat == CI8)
            return 512;
        return 0;
    }

    /// <summary>Block width, block height and bytes per block for each known GX format.</summary>
    public static bool TryGetBlock(byte gxFormat, out int blockWidth, out int blockHeight, out int blockBytes)
    {
        blockBytes = 32;
        switch (gxFormat)
        {
            case 0x0:
                blockWidth = 8;
                blockHeight = 8;
                return true; // I4
            case 0x1:
                blockWidth = 8;
                blockHeight = 4;
                return true; // I8
            case 0x2:
                blockWidth = 8;
                blockHeight = 4;
                return true; // IA4
            case 0x3:
                blockWidth = 4;
                blockHeight = 4;
                return true; // IA8
            case 0x4:
                blockWidth = 4;
                blockHeight = 4;
                return true; // RGB565
            case 0x5:
                blockWidth = 4;
                blockHeight = 4;
                return true; // RGB5A3
            case 0x6:
                blockWidth = 4;
                blockHeight = 4;
                blockBytes = 64;
                return true; // RGBA8
            case CI4:
                blockWidth = 8;
                blockHeight = 8;
                return true;
            case CI8:
                blockWidth = 8;
                blockHeight = 4;
                return true;
            case 0xA:
                blockWidth = 4;
                blockHeight = 4;
                return true; // CI14
            case 0xE:
                blockWidth = 8;
                blockHeight = 8;
                return true; // CMPR
            default:
                blockWidth = blockHeight = 0;
                return false;
        }
    }

    /// <summary>Returns the size of each mipmap, or null if the format is unknown.</summary>
    public static int[] GetMipSizes(byte gxFormat, int width, int height, int mipMapCount)
    {
        if (!TryGetBlock(gxFormat, out int bw, out int bh, out int bytes))
            return null;

        int[] sizes = new int[mipMapCount];
        for (int i = 0; i < mipMapCount; i++)
        {
            int w = Math.Max(1, width >> i);
            int h = Math.Max(1, height >> i);
            sizes[i] = (w + bw - 1) / bw * ((h + bh - 1) / bh) * bytes;
        }
        return sizes;
    }

    public override void Read(BinaryReader reader, TextureNativeStruct_0001 texture)
    {
        texture.maxAniso = Shared.Switch(reader.ReadUInt32());
        texture.biasClamp = Shared.Switch(reader.ReadInt32());
        texture.edgeLod = Shared.Switch(reader.ReadInt32());
        texture.lodBias = Shared.Switch(reader.ReadSingle());

        texture.textureNameRaw = reader.ReadBytes(TextureString.Length);
        texture.textureName = TextureString.Decode(texture.textureNameRaw);
        texture.alphaNameRaw = reader.ReadBytes(TextureString.Length);
        texture.alphaName = TextureString.Decode(texture.alphaNameRaw);

        texture.rasterFormatFlags = (TextureRasterFormat)Shared.Switch(reader.ReadInt32());
        texture.width = Shared.Switch(reader.ReadInt16());
        texture.height = Shared.Switch(reader.ReadInt16());

        texture.bitDepth = reader.ReadByte();
        texture.mipMapCount = reader.ReadByte();
        texture.type = reader.ReadByte();
        texture.compression = reader.ReadByte();
        if (texture.renderWareVersion > new RWVersion(3, 3, 0, 2))
            texture.gameCubePalettePixelFormat = texture.compression;

        texture.unknownGameCube = Shared.Switch(reader.ReadUInt32());

        // Some RenderWare GameCube exporters store a PAL8 texture as GX_TF_I8
        // (the raster flags, rather than the GX type, identify the palette).
        int paletteSize = GetPaletteSize(texture.type, texture.rasterFormatFlags);
        if (paletteSize != 0)
            texture.gameCubePalette = reader.ReadBytes(paletteSize);

        int totalDataSize = Shared.Switch(reader.ReadInt32());

        int[] mipSizes = GetMipSizes(texture.type, texture.width, texture.height, texture.mipMapCount);
        if (mipSizes == null || mipSizes.Sum() != totalDataSize)
            mipSizes = new[] { totalDataSize };

        texture.mipMaps = new MipMapEntry[mipSizes.Length];
        for (int i = 0; i < mipSizes.Length; i++)
            texture.mipMaps[i] = new MipMapEntry(mipSizes[i], reader.ReadBytes(mipSizes[i]));
    }

    public override void Write(TextureNativeStruct_0001 texture, List<byte> listBytes)
    {
        listBytes.AddRange(BitConverter.GetBytes(texture.maxAniso).Reverse());
        listBytes.AddRange(BitConverter.GetBytes(texture.biasClamp).Reverse());
        listBytes.AddRange(BitConverter.GetBytes(texture.edgeLod).Reverse());
        listBytes.AddRange(BitConverter.GetBytes(texture.lodBias).Reverse());

        listBytes.AddRange(TextureString.Encode(texture.textureName, texture.textureNameRaw, 0));
        listBytes.AddRange(TextureString.Encode(texture.alphaName, texture.alphaNameRaw, 0xDD));

        listBytes.AddRange(BitConverter.GetBytes((int)texture.rasterFormatFlags).Reverse());
        listBytes.AddRange(BitConverter.GetBytes(texture.width).Reverse());
        listBytes.AddRange(BitConverter.GetBytes(texture.height).Reverse());
        listBytes.Add(texture.bitDepth);
        listBytes.Add(texture.mipMapCount);
        listBytes.Add(texture.type);
        listBytes.Add(texture.compression);

        listBytes.AddRange(BitConverter.GetBytes(texture.unknownGameCube).Reverse());
        if (texture.gameCubePalette != null)
            listBytes.AddRange(texture.gameCubePalette);

        listBytes.AddRange(BitConverter.GetBytes(texture.mipMaps.Sum(m => m.dataSize)).Reverse());
        foreach (MipMapEntry mip in texture.mipMaps)
            listBytes.AddRange(mip.data);
    }
}
