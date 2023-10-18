namespace RenderWareFile
{
    public struct Vertex3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vertex3(float a, float b, float c)
        {
            X = a;
            Y = b;
            Z = c;
        }
    }
}
