using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

/// <summary>
/// Xbox layout: same header as PC plus the total mipmap data size, and no per-mipmap size.
/// Mipmap sizes are currently inferred from the total (see BiggestPowerOfTwoUnder).
/// </summary>
internal class XboxTextureHandler : D3DTextureHandler
{
    private int mipMapIndex;

    protected override void ReadExtraHeader(BinaryReader reader, TextureNativeStruct_0001 texture) =>
        ReadTotalMipDataSize(reader, texture);

    private void ReadTotalMipDataSize(BinaryReader reader, TextureNativeStruct_0001 texture)
    {
        texture.totalMipMapDataSize = reader.ReadInt32();
        mipMapIndex = 0;
    }

    protected override int ReadMipDataSize(BinaryReader reader, TextureNativeStruct_0001 texture, int passedSize)
    {
        int compression = texture.compression;
        int blockBytes = compression == 12 ? 8 : compression >= 13 && compression <= 16 ? 16 : 0;
        if (blockBytes != 0)
        {
            int remaining = texture.totalMipMapDataSize - passedSize;
            if (remaining <= 0)
            {
                mipMapIndex++;
                return 0;
            }

            int width = Math.Max(1, texture.width >> mipMapIndex);
            int height = Math.Max(1, texture.height >> mipMapIndex);
            int expected = ((width + 3) / 4) * ((height + 3) / 4) * blockBytes;
            mipMapIndex++;
            if (expected <= remaining)
                return expected;

            throw new InvalidDataException("Xbox compressed mipmap data is truncated.");
        }

        mipMapIndex++;
        return BiggestPowerOfTwoUnder(texture.totalMipMapDataSize - passedSize);
    }

    protected override void WriteExtraHeader(TextureNativeStruct_0001 texture, List<byte> listBytes)
    {
        texture.totalMipMapDataSize = 0;
        foreach (MipMapEntry mip in texture.mipMaps)
            texture.totalMipMapDataSize += mip.dataSize;

        listBytes.AddRange(BitConverter.GetBytes(texture.totalMipMapDataSize));
    }

    protected override void WriteMipDataSize(MipMapEntry mip, List<byte> listBytes) { }

    private static int BiggestPowerOfTwoUnder(int number) =>
        (int)Math.Pow(2, Math.Floor(Math.Log(number, 2)));
}
