using System.IO;

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

        public Vertex3(double x, double y, double z) : this((float)x, (float)y, (float)z) { }

        public Vertex3(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
        }
    }
}
