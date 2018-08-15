using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class AtomicSector_0009 : RWSection
    {
        public AtomicSectorStruct_0001 atomicSectorStruct;
        public Extension_0003 atomicSectorExtension;

        public AtomicSector_0009 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.AtomicSector;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section atomicStructSection = (Section)binaryReader.ReadInt32();
            if (atomicStructSection != Section.Struct) throw new Exception();
            atomicSectorStruct = new AtomicSectorStruct_0001().Read(binaryReader);

            Section atomicExtensionSection = (Section)binaryReader.ReadInt32();
            if (atomicExtensionSection != Section.Extension) throw new Exception();
            atomicSectorExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.AtomicSector;

            listBytes.AddRange(atomicSectorStruct.GetBytes(fileVersion));
            listBytes.AddRange(atomicSectorExtension.GetBytes(fileVersion));
        }
    }
}
