namespace RenderWareFile
{
    public struct Triangle
    {
        public ushort materialIndex { get; set; }
        public ushort vertex1 { get; set; }
        public ushort vertex2 { get; set; }
        public ushort vertex3 { get; set; }

        public Triangle(ushort m, ushort v1, ushort v2, ushort v3)
        {
            materialIndex = m;
            vertex1 = v1;
            vertex2 = v2;
            vertex3 = v3;
        }
    }
}
