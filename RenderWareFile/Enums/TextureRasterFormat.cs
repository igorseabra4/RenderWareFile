namespace RenderWareFile.Sections
{
    public enum TextureRasterFormat : int
    {
        RASTER_DEFAULT = 0,
        RASTER_C1555 = 0x0100,
        RASTER_C565 = 0x0200,
        RASTER_C4444 = 0x0300,
        RASTER_LUM8 = 0x0400,
        RASTER_C8888 = 0x0500,
        RASTER_C888 = 0x0600,
        RASTER_D16 = 0x0700,
        RASTER_D24 = 0x0800,
        RASTER_D32 = 0x0900,
        RASTER_C555 = 0x0A00,
        RASTER_AUTOMIPMAP = 0x1000,
        RASTER_PAL8 = 0x2000,
        RASTER_PAL4 = 0x4000,
        RASTER_MIPMAP = 0x8000
    }
}