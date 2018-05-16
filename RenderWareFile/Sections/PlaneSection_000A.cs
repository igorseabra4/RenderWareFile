using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class PlaneSector_000A : RWSection
    {
        public PlaneStruct_0001 planeStruct;
        public RWSection leftSection;
        public RWSection rightSection;

        public PlaneSector_000A Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.PlaneSector;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section planeStructSection = (Section)binaryReader.ReadInt32();
            if (planeStructSection != Section.Struct) throw new Exception();
            planeStruct = new PlaneStruct_0001().Read(binaryReader);

            Section leftSectionSection = (Section)binaryReader.ReadInt32();
            if (leftSectionSection == Section.AtomicSector & planeStruct.leftIsAtomic == 1)
            {
                leftSection = new AtomicSector_0009().Read(binaryReader);
            }
            else if (leftSectionSection == Section.PlaneSector & planeStruct.leftIsAtomic == 0)
            {
                leftSection = new PlaneSector_000A().Read(binaryReader);
            }
            else throw new Exception();

            Section rightSectionSection = (Section)binaryReader.ReadInt32();
            if (rightSectionSection == Section.AtomicSector & planeStruct.rightIsAtomic == 1)
            {
                rightSection = new AtomicSector_0009().Read(binaryReader);
            }
            else if (rightSectionSection == Section.PlaneSector & planeStruct.rightIsAtomic == 0)
            {
                rightSection = new PlaneSector_000A().Read(binaryReader);
            }
            else throw new Exception();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.PlaneSector;

            listBytes.AddRange(planeStruct.GetBytes(fileVersion));
            listBytes.AddRange(leftSection.GetBytes(fileVersion));
            listBytes.AddRange(rightSection.GetBytes(fileVersion));
        }
    }
}
