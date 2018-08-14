using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class UserDataPLG_011F : RWSection
    {
        public byte[] data;

        public int userDataType;
        public int unknown2;
        public string attribute;
        public int unknown3;
        public int numTriangles;
        public Color[] collisionFlags;
        public int unknown4;
        public string userData;
        public int unknown5;
        public int unknown6;
        public int unknown7;

        public UserDataPLG_011F Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.UserDataPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            data = binaryReader.ReadBytes(sectionSize);

            if (!ReadFileMethods.isCollision)
                return this;

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
            
            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.UserDataPLG;

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