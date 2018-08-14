namespace RenderWareFile.Sections
{
    public enum BinMeshHeaderFlags : int
    {
        TriangleList = 0x00000000,
        TriangleStrip = 0x00000001,
        TriangleFan = 0x00000002,
        LineList = 0x00000004,
        PolyLine = 0x00000008,
        PointList = 0x00000010,
        PrimitiveMask = 0x000000FF,
        Unindexed = 0x00000100
    }
}
