using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class Atomic_0014 : RWSection
    {
        byte[] data;

        public Atomic_0014 Read(BinaryReader binaryReader, Section section)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            data = binaryReader.ReadBytes(sectionSize);

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Atomic;
            throw new NotImplementedException();
        }
    }
}
