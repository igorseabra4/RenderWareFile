using System.IO;

namespace RenderWareFile
{
    public struct Vertex4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public int W { get; set; }

        public Vertex4(float x, float y, float z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vertex4(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            W = reader.ReadInt32();
        }
    }
}
