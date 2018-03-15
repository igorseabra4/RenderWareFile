using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public class FrameList_000E : RWSection
    {
        // Not yet implemented

        public FrameList_000E Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.FrameList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }
        
        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.FrameList;

            throw new NotImplementedException();
        }
    }
}
