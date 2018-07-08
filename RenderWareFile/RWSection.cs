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
        AtomicSector = 0x9,
        PlaneSector = 0xA,
        World = 0xB,
        FrameList = 0xE,
        Geometry = 0xF,
        Clump = 0x10,
        Atomic = 0x14,
        GeometryList = 0x1A,
        ChunkGroupStart = 0x29,
        ChunkGroupEnd = 0x2A,
        ColTree = 0x2C,
        MorphPLG = 0x105,
        SkyMipmapVal = 0x110,
        CollisionPLG = 0x11D,
        UserDataPLG = 0x11F,
        BinMeshPLG = 0x50E,
        NativeDataPLG = 0x510
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

    public class GenericSection : RWSection
    {
        byte[] data;

        public GenericSection Read(BinaryReader binaryReader, Section section)
        {
            sectionIdentifier = section;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            data = binaryReader.ReadBytes(sectionSize);

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            //listBytes.AddRange(data);
            sectionIdentifier = Section.None;
            throw new NotImplementedException();
        }
    }

    public static class General
    {
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
