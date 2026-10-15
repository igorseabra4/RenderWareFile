using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class Atomic_0014 : RWSection
    {
        public AtomicStruct_0001 atomicStruct { get; set; }
        public Extension_0003 atomicExtension { get; set; }

        public Atomic_0014() { }
        public Atomic_0014(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section atomicStructSection = (Section)binaryReader.ReadInt32();
            if (atomicStructSection != Section.Struct) throw new Exception();
            atomicStruct = new AtomicStruct_0001(binaryReader);

            Section atomicExtensionSection = (Section)binaryReader.ReadInt32();
            if (atomicExtensionSection != Section.Extension) throw new Exception();
            atomicExtension = new Extension_0003(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Atomic;

            listBytes.AddRange(atomicStruct.GetBytes(fileVersion));
            listBytes.AddRange(atomicExtension.GetBytes(fileVersion));
        }
    }
}
