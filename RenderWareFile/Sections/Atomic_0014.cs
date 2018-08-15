using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class Atomic_0014 : RWSection
    {
        public AtomicStruct_0001 atomicStruct;
        public Extension_0003 atomicExtension;

        public Atomic_0014 Read(BinaryReader binaryReader, Section section)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section atomicStructSection = (Section)binaryReader.ReadInt32();
            if (atomicStructSection != Section.Struct) throw new Exception();
            atomicStruct = new AtomicStruct_0001().Read(binaryReader);

            Section atomicExtensionSection = (Section)binaryReader.ReadInt32();
            if (atomicExtensionSection != Section.Extension) throw new Exception();
            atomicExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Atomic;

            listBytes.AddRange(atomicStruct.GetBytes(fileVersion));
            listBytes.AddRange(atomicExtension.GetBytes(fileVersion));
        }
    }
}
