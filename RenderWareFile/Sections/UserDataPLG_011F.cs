using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class UserDataPLG_011F : RWSection
    {
        public byte[] data;

        public int unknown1;
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
            sectionIdentifier = Section.CollisionPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            if (true)
            {
                data = binaryReader.ReadBytes(sectionSize);
                return this;
            }

            unknown1 = binaryReader.ReadInt32();
            unknown2 = binaryReader.ReadInt32();
            attribute = General.ReadFromZeroTerminatedString(binaryReader);
            unknown3 = binaryReader.ReadInt32();
            numTriangles = binaryReader.ReadInt32();
            collisionFlags = new Color[numTriangles];
            for (int i = 0; i < numTriangles; i++)
            {
                collisionFlags[i] = new Color(binaryReader.ReadInt32());
            }
            unknown4 = binaryReader.ReadInt32();
            userData = General.ReadFromZeroTerminatedString(binaryReader);
            unknown5 = binaryReader.ReadInt32();
            unknown6 = binaryReader.ReadInt32();
            unknown7 = binaryReader.ReadInt32();
            
            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.UserDataPLG;

            listBytes.AddRange(BitConverter.GetBytes(unknown1));
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