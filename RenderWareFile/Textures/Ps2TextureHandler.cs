using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

internal class Ps2TextureHandler : TexturePlatformHandler
{
    public override bool ValidatesSectionSize => false;

    public override void Read(BinaryReader reader, TextureNativeStruct_0001 texture) { }

    public override void Write(TextureNativeStruct_0001 texture, List<byte> listBytes) { }

    public override void ReadTrailingSections(BinaryReader reader, TextureNative_0015 native)
    {
        reader.ReadInt32();
        native.textureNativeStruct.ps2TextureName = new Ps2TextureString(reader);
        native.textureNativeStruct.textureName = native.textureNativeStruct.ps2TextureName.stringString;
        reader.ReadInt32();
        native.textureNativeStruct.ps2AlphaName = new Ps2TextureString(reader);
        native.textureNativeStruct.alphaName = native.textureNativeStruct.ps2AlphaName.stringString;

        reader.ReadInt32();
        native.PS2RasterFormat = new Ps2TextureRasterFormatStruct_0001(reader);
    }

    public override void WriteTrailingSections(TextureNative_0015 native, RWVersion fileVersion, List<byte> listBytes)
    {
        listBytes.AddRange(new Ps2TextureString(native.textureNativeStruct.textureName, native.textureNativeStruct.ps2TextureName).GetBytes(fileVersion));
        listBytes.AddRange(new Ps2TextureString(native.textureNativeStruct.alphaName, native.textureNativeStruct.ps2AlphaName).GetBytes(fileVersion));
        listBytes.AddRange(native.PS2RasterFormat?.GetBytes(fileVersion));
    }
}
