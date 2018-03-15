using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareFile
{
    public enum Section : int
    {
        None = 0x0,
        Struct = 0x1,
        String = 0x2,
        Extension = 0x3,
        Texture = 0x6,
        Material = 0x7,
        MaterialList = 0x8,
        AtomicSection = 0x9,
        PlaneSection = 0xA,
        World = 0xB,
        FrameList = 0xE,
        Geometry = 0xF,
        Clump = 0x10,
        Atomic = 0x14,
        GeometryList = 0x1A,
        ChunkGroupStart = 0x29,
        ChunkGroupEnd = 0x2A,
        BinMeshPLG = 0x50E
    }

    public abstract class RWSection
    {
        public Section sectionIdentifier;
        public int sectionSize;
        public int renderWareVersion;

        public byte[] GetBytes(int fileVersion)
        {
            List<byte> listBytes = new List<byte>()
            {
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            };

            SetListBytes(fileVersion, ref listBytes);

            sectionSize = listBytes.Count() - 0xC;
            renderWareVersion = fileVersion;

            listBytes[0] = BitConverter.GetBytes((int)sectionIdentifier)[0];
            listBytes[1] = BitConverter.GetBytes((int)sectionIdentifier)[1];
            listBytes[2] = BitConverter.GetBytes((int)sectionIdentifier)[2];
            listBytes[3] = BitConverter.GetBytes((int)sectionIdentifier)[3];
            listBytes[4] = BitConverter.GetBytes(sectionSize)[0];
            listBytes[5] = BitConverter.GetBytes(sectionSize)[1];
            listBytes[6] = BitConverter.GetBytes(sectionSize)[2];
            listBytes[7] = BitConverter.GetBytes(sectionSize)[3];
            listBytes[8] = BitConverter.GetBytes(renderWareVersion)[0];
            listBytes[9] = BitConverter.GetBytes(renderWareVersion)[1];
            listBytes[10] = BitConverter.GetBytes(renderWareVersion)[2];
            listBytes[11] = BitConverter.GetBytes(renderWareVersion)[3];

            return listBytes.ToArray();
        }

        public abstract void SetListBytes(int fileVersion, ref List<byte> listBytes);
    }
    
    public struct Vertex3
    {
        public float X;
        public float Y;
        public float Z;
    }

    public struct TextCoord
    {
        public float X;
        public float Y;
    }

    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static explicit operator int(Color v)
        {
            return BitConverter.ToInt32(new byte[] { v.R, v.G, v.B, v.A }, 0);
        }
    }

    public struct Triangle
    {
        public ushort materialIndex;
        public ushort vertex1;
        public ushort vertex2;
        public ushort vertex3;
    }

}
