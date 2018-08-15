using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class AtomicStruct_0001 : RWSection
    {
        public int frameIndex;
        public int geometryIndex;
        public int unknown1;
        public int unknown2;

        public AtomicStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();
            
            frameIndex = binaryReader.ReadInt32();
            geometryIndex = binaryReader.ReadInt32();
            unknown1 = binaryReader.ReadInt32();
            unknown2 = binaryReader.ReadInt32();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(frameIndex));
            listBytes.AddRange(BitConverter.GetBytes(geometryIndex));
            listBytes.AddRange(BitConverter.GetBytes(unknown1));
            listBytes.AddRange(BitConverter.GetBytes(unknown2));
        }
    }
}