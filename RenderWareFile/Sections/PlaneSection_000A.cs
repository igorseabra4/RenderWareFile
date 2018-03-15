using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public class PlaneSection_000A : RWSection
    {
        public PlaneStruct_0001 planeStruct;
        public RWSection leftSection;
        public RWSection rightSection;

        public PlaneSection_000A Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.PlaneSection;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section planeStructSection = (Section)binaryReader.ReadInt32();
            if (planeStructSection != Section.Struct) throw new Exception();
            planeStruct = new PlaneStruct_0001().Read(binaryReader);

            Section leftSectionSection = (Section)binaryReader.ReadInt32();
            if (leftSectionSection == Section.AtomicSection & planeStruct.leftIsAtomic == 1)
            {
                leftSection = new AtomicSection_0009().Read(binaryReader);
            }
            else if (leftSectionSection == Section.PlaneSection & planeStruct.leftIsAtomic == 0)
            {
                leftSection = new PlaneSection_000A().Read(binaryReader);
            }
            else throw new Exception();

            Section rightSectionSection = (Section)binaryReader.ReadInt32();
            if (rightSectionSection == Section.AtomicSection & planeStruct.rightIsAtomic == 1)
            {
                rightSection = new AtomicSection_0009().Read(binaryReader);
            }
            else if (rightSectionSection == Section.PlaneSection & planeStruct.rightIsAtomic == 0)
            {
                rightSection = new PlaneSection_000A().Read(binaryReader);
            }
            else throw new Exception();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.PlaneSection;

            listBytes.AddRange(planeStruct.GetBytes(fileVersion));
            listBytes.AddRange(leftSection.GetBytes(fileVersion));
            listBytes.AddRange(rightSection.GetBytes(fileVersion));
        }
    }

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
