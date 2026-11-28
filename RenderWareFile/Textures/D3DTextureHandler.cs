using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

/// <summary>PC Direct3D 8/9 layout (RpRasterPC_Header).</summary>
internal class D3DTextureHandler : TexturePlatformHandler
{
    public override void Read(BinaryReader reader, TextureNativeStruct_0001 texture)
    {
        texture.textureNameRaw = reader.ReadBytes(TextureString.Length);
        texture.textureName = TextureString.Decode(texture.textureNameRaw);
        texture.alphaNameRaw = reader.ReadBytes(TextureString.Length);
        texture.alphaName = TextureString.Decode(texture.alphaNameRaw);

        texture.rasterFormatFlags = (TextureRasterFormat)reader.ReadUInt32();
        texture.d3dFormat = reader.ReadUInt32();
        texture.originalD3DFormat = texture.d3dFormat;
        texture.hasAlpha = texture.d3dFormat != 0;
        texture.width = reader.ReadInt16();
        texture.height = reader.ReadInt16();

        texture.bitDepth = reader.ReadByte();
        texture.mipMapCount = reader.ReadByte();
        texture.type = reader.ReadByte();
        texture.compression = reader.ReadByte();

        ReadExtraHeader(reader, texture);

        int paletteSize =
            (texture.rasterFormatFlags & TextureRasterFormat.RASTER_PAL4) != 0 ? 0x80 / 4 :
            (texture.rasterFormatFlags & TextureRasterFormat.RASTER_PAL8) != 0 ? 0x400 / 4 : 0;

        if (paletteSize != 0)
        {
            texture.palette = new Color[paletteSize];
            for (int i = 0; i < paletteSize; i++)
            {
                texture.palette[i] = new Color(reader.ReadInt32());
                if (texture.platformType == TexturePlatformID.Xbox)
                    (texture.palette[i].B, texture.palette[i].R) = (texture.palette[i].R, texture.palette[i].B);
            }
        }

        int passedSize = 0;
        texture.mipMaps = new MipMapEntry[texture.mipMapCount];
        for (int i = 0; i < texture.mipMapCount; i++)
        {
            int dataSize = ReadMipDataSize(reader, texture, passedSize);
            texture.mipMaps[i] = new MipMapEntry(dataSize, reader.ReadBytes(dataSize));
            passedSize += dataSize;
        }
    }

    public override void Write(TextureNativeStruct_0001 texture, List<byte> listBytes)
    {
        listBytes.AddRange(TextureString.Encode(texture.textureName, texture.textureNameRaw, 0));
        listBytes.AddRange(TextureString.Encode(texture.alphaName, texture.alphaNameRaw, 0));

        listBytes.AddRange(BitConverter.GetBytes((uint)texture.rasterFormatFlags));
        listBytes.AddRange(BitConverter.GetBytes(
            texture.platformType == TexturePlatformID.PCD3D9 && texture.d3dFormat != 0 ? texture.d3dFormat :
            texture.platformType != TexturePlatformID.PCD3D9 &&
            texture.originalD3DFormat == texture.d3dFormat &&
            texture.hasAlpha == (texture.d3dFormat != 0) ? texture.d3dFormat :
            texture.hasAlpha ? 1u : 0u));
        listBytes.AddRange(BitConverter.GetBytes(texture.width));
        listBytes.AddRange(BitConverter.GetBytes(texture.height));

        listBytes.Add(texture.bitDepth);
        listBytes.Add(texture.mipMapCount);
        listBytes.Add(texture.type);
        listBytes.Add(texture.compression);

        WriteExtraHeader(texture, listBytes);

        if (texture.palette != null)
            foreach (Color c in texture.palette)
            {
                listBytes.Add(texture.platformType == TexturePlatformID.Xbox ? c.B : c.R);
                listBytes.Add(c.G);
                listBytes.Add(texture.platformType == TexturePlatformID.Xbox ? c.R : c.B);
                listBytes.Add(c.A);
            }

        foreach (MipMapEntry mip in texture.mipMaps)
        {
            WriteMipDataSize(mip, listBytes);
            listBytes.AddRange(mip.data);
        }
    }

    protected virtual void ReadExtraHeader(BinaryReader reader, TextureNativeStruct_0001 texture) { }

    protected virtual int ReadMipDataSize(BinaryReader reader, TextureNativeStruct_0001 texture, int passedSize) =>
        reader.ReadInt32();

    protected virtual void WriteExtraHeader(TextureNativeStruct_0001 texture, List<byte> listBytes) { }

    protected virtual void WriteMipDataSize(MipMapEntry mip, List<byte> listBytes) =>
        listBytes.AddRange(BitConverter.GetBytes(mip.dataSize));
}
