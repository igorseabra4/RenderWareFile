namespace RenderWareFile
{
    public struct Triangle
    {
        public ushort materialIndex;
        public ushort vertex1;
        public ushort vertex2;
        public ushort vertex3;

        public Triangle(ushort m, ushort v1, ushort v2, ushort v3)
        {
            materialIndex = m;
            vertex1 = v1;
            vertex2 = v2;
            vertex3 = v3;
        }
    }
}
