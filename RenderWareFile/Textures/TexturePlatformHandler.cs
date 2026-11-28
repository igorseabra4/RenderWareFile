using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

/// <summary>
/// Reads and writes the platform specific part of a texture native struct
/// (everything after the platform id and filter/addressing word).
/// </summary>
internal abstract class TexturePlatformHandler
{
    /// <summary>If true, the reader checks that the whole struct was consumed.</summary>
    public virtual bool ValidatesSectionSize => true;

    public abstract void Read(BinaryReader reader, TextureNativeStruct_0001 texture);
    public abstract void Write(TextureNativeStruct_0001 texture, List<byte> listBytes);

    /// <summary>Reads sections that follow the struct inside the texture native section (before the extension).</summary>
    public virtual void ReadTrailingSections(BinaryReader reader, TextureNative_0015 native) { }

    /// <summary>Writes sections that follow the struct inside the texture native section (before the extension).</summary>
    public virtual void WriteTrailingSections(TextureNative_0015 native, RWVersion fileVersion, List<byte> listBytes) { }

    public static TexturePlatformHandler Get(TexturePlatformID platform)
    {
        switch (platform)
        {
            case TexturePlatformID.PCD3D8:
            case TexturePlatformID.PCD3D9:
                return new D3DTextureHandler();
            case TexturePlatformID.Xbox:
                return new XboxTextureHandler();
            case TexturePlatformID.GameCube:
                return new GameCubeTextureHandler();
            case TexturePlatformID.PS2:
                return new Ps2TextureHandler();
            default:
                throw new InvalidDataException("Unsupported texture format: " + platform.ToString());
        }
    }
}
