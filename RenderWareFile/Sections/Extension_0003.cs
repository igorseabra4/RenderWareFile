using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RenderWareFile
{
    public class Extension_0003 : RWSection
    {
        public Extension_0003 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Extension;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Extension;
        }
    }
}
