using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareFile
{
    public static class Shared
    {
        public static bool DoNotSwitch = false;

        public static float Switch(float f)
        {
            byte[] a = BitConverter.GetBytes(f).Reverse().ToArray();
            return BitConverter.ToSingle(a, 0);
        }

        public static int Switch(int f)
        {
            byte[] a = BitConverter.GetBytes(f).Reverse().ToArray();
            return BitConverter.ToInt32(a, 0);
        }

        public static short Switch(short f)
        {
            byte[] a = BitConverter.GetBytes(f).Reverse().ToArray();
            return BitConverter.ToInt16(a, 0);
        }

        public static float SwitchToggleable(float f)
        {
            if (DoNotSwitch) return f;

            byte[] a = BitConverter.GetBytes(f).Reverse().ToArray();
            return BitConverter.ToSingle(a, 0);
        }

        public static int SwitchToggleable(int f)
        {
            if (DoNotSwitch) return f;

            byte[] a = BitConverter.GetBytes(f).Reverse().ToArray();
            return BitConverter.ToInt32(a, 0);
        }

        public static short SwitchToggleable(short f)
        {
            if (DoNotSwitch) return f;

            byte[] a = BitConverter.GetBytes(f).Reverse().ToArray();
            return BitConverter.ToInt16(a, 0);
        }

        public static string ReadFromZeroTerminatedString(BinaryReader binaryReader)
        {
            List<char> charList = new List<char>();
            byte c = binaryReader.ReadByte();
            while (c != 0)
            {
                charList.Add((char)c);
                c = binaryReader.ReadByte();
            }

            return new string(charList.ToArray());
        }

        public static List<int> MaterialList;
    }
}
