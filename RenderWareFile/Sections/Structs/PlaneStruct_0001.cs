using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class PlaneStruct_0001 : RWSection
    {
        public int type { get; set; }
        public float value { get; set; }
        public int leftIsAtomic { get; set; }
        public int rightIsAtomic { get; set; }
        public float leftValue { get; set; }
        public float rightValue { get; set; }

        public PlaneStruct_0001(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            type = binaryReader.ReadInt32();
            value = binaryReader.ReadSingle();
            leftIsAtomic = binaryReader.ReadInt32();
            rightIsAtomic = binaryReader.ReadInt32();
            leftValue = binaryReader.ReadSingle();
            rightValue = binaryReader.ReadSingle();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
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
