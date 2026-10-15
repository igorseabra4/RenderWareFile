using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class PlaneSector_000A : RWSection
    {
        public PlaneStruct_0001 planeStruct { get; set; }
        public RWSection leftSection { get; set; }
        public RWSection rightSection { get; set; }

        public PlaneSector_000A(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.PlaneSector;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            Section planeStructSection = (Section)binaryReader.ReadInt32();
            if (planeStructSection != Section.Struct) throw new Exception();
            planeStruct = new PlaneStruct_0001(binaryReader);

            Section leftSectionSection = (Section)binaryReader.ReadInt32();
            if (leftSectionSection == Section.AtomicSector & planeStruct.leftIsAtomic == 1)
            {
                leftSection = new AtomicSector_0009(binaryReader);
            }
            else if (leftSectionSection == Section.PlaneSector & planeStruct.leftIsAtomic == 0)
            {
                leftSection = new PlaneSector_000A(binaryReader);
            }
            else throw new Exception();

            Section rightSectionSection = (Section)binaryReader.ReadInt32();
            if (rightSectionSection == Section.AtomicSector & planeStruct.rightIsAtomic == 1)
            {
                rightSection = new AtomicSector_0009(binaryReader);
            }
            else if (rightSectionSection == Section.PlaneSector & planeStruct.rightIsAtomic == 0)
            {
                rightSection = new PlaneSector_000A(binaryReader);
            }
            else throw new Exception();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.PlaneSector;

            listBytes.AddRange(planeStruct.GetBytes(fileVersion));
            listBytes.AddRange(leftSection.GetBytes(fileVersion));
            listBytes.AddRange(rightSection.GetBytes(fileVersion));
        }
    }
}
