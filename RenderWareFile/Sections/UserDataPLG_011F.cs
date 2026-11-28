using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class UserDataPLG_011F : RWSection
    {
        public byte[] data { get; set; }

        public int userDataType { get; set; }
        public int unknown2 { get; set; }
        public string attribute { get; set; }
        public int unknown3 { get; set; }
        public int numTriangles { get; set; }
        public Color[] collisionFlags { get; set; }
        public int unknown4 { get; set; }
        public string userData { get; set; }
        public int unknown5 { get; set; }
        public int unknown6 { get; set; }
        public int unknown7 { get; set; }

        public UserDataPLG_011F(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.UserDataPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            data = binaryReader.ReadBytes(sectionSize);

            if (!ReadFileMethods.isCollision)
                return;

            binaryReader.BaseStream.Position -= sectionSize;

            userDataType = binaryReader.ReadInt32();

            if (userDataType == 0x02)
            {
                unknown2 = binaryReader.ReadInt32();
                attribute = Shared.ReadFromZeroTerminatedString(binaryReader);
                unknown3 = binaryReader.ReadInt32();
                numTriangles = binaryReader.ReadInt32();
                collisionFlags = new Color[numTriangles];
                for (int i = 0; i < numTriangles; i++)
                {
                    collisionFlags[i] = new Color(binaryReader.ReadInt32());
                }
                unknown4 = binaryReader.ReadInt32();
            }

            userData = Shared.ReadFromZeroTerminatedString(binaryReader);
            unknown5 = binaryReader.ReadInt32();
            unknown6 = binaryReader.ReadInt32();
            unknown7 = binaryReader.ReadInt32();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.UserDataPLG;

            if (!ReadFileMethods.isCollision)
            {
                listBytes.AddRange(data);
                return;
            }

            listBytes.AddRange(BitConverter.GetBytes(userDataType));
            listBytes.AddRange(BitConverter.GetBytes(unknown2));
            foreach (char i in attribute)
                listBytes.Add((byte)i);
            listBytes.Add(0);
            listBytes.AddRange(BitConverter.GetBytes(unknown3));
            listBytes.AddRange(BitConverter.GetBytes(numTriangles));
            for (int i = 0; i < numTriangles; i++)
            {
                listBytes.Add(collisionFlags[i].R);
                listBytes.Add(collisionFlags[i].G);
                listBytes.Add(collisionFlags[i].B);
                listBytes.Add(collisionFlags[i].A);
            }
            listBytes.AddRange(BitConverter.GetBytes(unknown4));
            foreach (char i in userData)
                listBytes.Add((byte)i);
            listBytes.Add(0);
            listBytes.AddRange(BitConverter.GetBytes(unknown5));
            listBytes.AddRange(BitConverter.GetBytes(unknown6));
            listBytes.AddRange(BitConverter.GetBytes(unknown7));
        }
    }
}