using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public struct Vertex3
    {
        public float X;
        public float Y;
        public float Z;

        public Vertex3(float a, float b, float c)
        {
            X = a;
            Y = b;
            Z = c;
        }
    }

    public struct TextCoord
    {
        public float X;
        public float Y;

        public TextCoord(float a, float b)
        {
            X = a;
            Y = b;
        }
    }

    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(byte[] v)
        {
            R = v[0];
            G = v[1];
            B = v[2];
            A = v[3];
        }

        public Color(int a)
        {
            this = new Color(BitConverter.GetBytes(a));
        }

        public static Color FromString(string s)
        {
            if (s.Length != 8) throw new ArgumentException();

            Color c;
            c.R = Convert.ToByte(new string(new char[] { s[0], s[1] }), 16);
            c.G = Convert.ToByte(new string(new char[] { s[2], s[3] }), 16);
            c.B = Convert.ToByte(new string(new char[] { s[4], s[5] }), 16);
            c.A = Convert.ToByte(new string(new char[] { s[6], s[7] }), 16);
            return c;
        }

        public static explicit operator int(Color v)
        {
            return BitConverter.ToInt32(new byte[] { v.R, v.G, v.B, v.A }, 0);
        }

        public override string ToString()
        {
            return String.Format("{0, 2:X2}{1, 2:X2}{2, 2:X2}{3, 2:X2}", R, G, B, A);
        }
    }

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
