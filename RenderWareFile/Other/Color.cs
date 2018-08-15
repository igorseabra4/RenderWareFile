using System;

namespace RenderWareFile
{
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

        public static bool operator ==(Color c1, Color c2)
        {
            return Equals(c1, c2);
        }

        public static bool operator !=(Color c1, Color c2)
        {
            return !Equals(c1, c2);
        }

        public override string ToString()
        {
            return String.Format("{0, 2:X2}{1, 2:X2}{2, 2:X2}{3, 2:X2}", R, G, B, A);
        }

        public override bool Equals(object obj)
        {
            if (obj is Color c) return (GetHashCode() == c.GetHashCode());
            else return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 1960784236;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            return hashCode;
        }
    }
}
