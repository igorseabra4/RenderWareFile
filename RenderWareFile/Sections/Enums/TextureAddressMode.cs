namespace RenderWareFile.Sections
{
    public enum TextureAddressMode : byte
    {
        TEXTUREADDRESSNATEXTUREADDRESS = 0, // no tiling
        TEXTUREADDRESSWRAP = 1, // tile in U or V direction
        TEXTUREADDRESSMIRROR = 2, // mirror in U or V direction
        TEXTUREADDRESSCLAMP = 3,
        TEXTUREADDRESSBORDER = 4
    }
}
