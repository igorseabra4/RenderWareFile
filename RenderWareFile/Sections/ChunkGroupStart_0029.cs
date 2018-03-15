using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public class ChunkGroupStart_0029 : RWSection
    {
        // Not yet implemented

        public ChunkGroupStart_0029 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.ChunkGroupStart;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.ChunkGroupStart;

            throw new NotImplementedException();
        }
    }
}
