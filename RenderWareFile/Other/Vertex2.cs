using System.IO;

namespace RenderWareFile
{
    public struct Vertex2
    {
        public float X;
        public float Y;

        public Vertex2(float a, float b)
        {
            X = a;
            Y = b;
        }

        public Vertex2(double x, double y) : this((float)x, (float)y) { }

        public Vertex2(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
        }
    }
}
