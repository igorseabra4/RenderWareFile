using System.Collections.Generic;
using System.IO;

namespace RenderWareFile
{
    public class GenericSection : RWSection
    {
        public byte[] data { get; set; }

        public GenericSection(BinaryReader binaryReader, Section section)
        {
            sectionIdentifier = section;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            data = binaryReader.ReadBytes(sectionSize);
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            listBytes.AddRange(data);
        }
    }
}
