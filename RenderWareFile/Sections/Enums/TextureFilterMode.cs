namespace RenderWareFile.Sections
{
    public enum TextureFilterMode : byte
    {
        FILTERNAFILTERMODE = 0, // filtering is disabled
        FILTERNEAREST = 1, // Point sampled
        FILTERLINEAR = 2, // Bilinear
        FILTERMIPNEAREST = 3, // Point sampled per pixel mip map
        FILTERMIPLINEAR = 4, // Bilinear per pixel mipmap
        FILTERLINEARMIPNEAREST = 5, // MipMap interp point sampled
        FILTERLINEARMIPLINEAR = 6 // Trilinear
    }
}
