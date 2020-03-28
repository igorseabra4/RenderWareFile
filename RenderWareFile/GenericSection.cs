using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile
{
    public class GenericSection : RWSection
    {
        public byte[] data;

        public GenericSection Read(BinaryReader binaryReader, Section section)
        {
            sectionIdentifier = section;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            data = binaryReader.ReadBytes(sectionSize);

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            listBytes.AddRange(data);
        }
    }
}
