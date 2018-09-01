using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public struct Struct1
    {
        public int unknown1;
        public int unknown2;
        public float unknown3;
        public float unknown4;
    }

    public struct Struct2
    {
        public short unknown1;
        public short unknown2;
        public byte unknown3;
        public byte unknown4;
        public short unknown5;
    }

    public class BFBB_CollisionData_Section1_00BEEF01 : RWSection
    {
        public string CCOL;
        public int amountOf1;
        public int amountOf2;

        public List<Struct1> list1;
        public List<Struct2> list2;
        
        public BFBB_CollisionData_Section1_00BEEF01 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section1;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            CCOL = new string(binaryReader.ReadChars(4).Reverse().ToArray());

            amountOf1 = Switch(binaryReader.ReadInt32());
            amountOf2 = Switch(binaryReader.ReadInt32());

            list1 = new List<Struct1>();
            for (int i = 0; i < amountOf1; i++)
            {
                list1.Add(new Struct1()
                {
                    unknown1 = Switch(binaryReader.ReadInt32()),
                    unknown2 = Switch(binaryReader.ReadInt32()),
                    unknown3 = Switch(binaryReader.ReadSingle()),
                    unknown4 = Switch(binaryReader.ReadSingle())
                });
            }

            list2 = new List<Struct2>();
            for (int i = 0; i < amountOf2; i++)
            {
                list2.Add(new Struct2()
                {
                    unknown1 = Switch(binaryReader.ReadInt16()),
                    unknown2 = Switch(binaryReader.ReadInt16()),
                    unknown3 = binaryReader.ReadByte(),
                    unknown4 = binaryReader.ReadByte(),
                    unknown5 = Switch(binaryReader.ReadInt16())
                });
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section1;

            listBytes.AddRange(CCOL.Cast<Byte>().Reverse());
            listBytes.AddRange(BitConverter.GetBytes(Switch(amountOf1)));
            listBytes.AddRange(BitConverter.GetBytes(Switch(amountOf2)));

            for (int i = 0; i < amountOf1; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(Switch(list1[i].unknown1)));
                listBytes.AddRange(BitConverter.GetBytes(Switch(list1[i].unknown2)));
                listBytes.AddRange(BitConverter.GetBytes(Switch(list1[i].unknown3)));
                listBytes.AddRange(BitConverter.GetBytes(Switch(list1[i].unknown4)));
            }

            for (int i = 0; i < amountOf2; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(Switch(list2[i].unknown1)));
                listBytes.AddRange(BitConverter.GetBytes(Switch(list2[i].unknown2)));
                listBytes.Add(list2[i].unknown3);
                listBytes.Add(list2[i].unknown4);
                listBytes.AddRange(BitConverter.GetBytes(Switch(list2[i].unknown5)));
            }
        }
    }
}
