using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class PlaneStruct_0001 : RWSection
    {
        public int type;
        public float value;
        public int leftIsAtomic;
        public int rightIsAtomic;
        public float leftValue;
        public float rightValue;

        public PlaneStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            type = binaryReader.ReadInt32();
            value = binaryReader.ReadSingle();
            leftIsAtomic = binaryReader.ReadInt32();
            rightIsAtomic = binaryReader.ReadInt32();
            leftValue = binaryReader.ReadSingle();
            rightValue = binaryReader.ReadSingle();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(type));
            listBytes.AddRange(BitConverter.GetBytes(value));
            listBytes.AddRange(BitConverter.GetBytes(leftIsAtomic));
            listBytes.AddRange(BitConverter.GetBytes(rightIsAtomic));
            listBytes.AddRange(BitConverter.GetBytes(leftValue));
            listBytes.AddRange(BitConverter.GetBytes(rightValue));
        }
    }
}
