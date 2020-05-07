using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public enum AtomicFlags : int
    {
        None = 0,
        CollisionTest = 1,
        Render = 4,
        CollisionTestAndRender = 5
    }

    public class AtomicStruct_0001 : RWSection
    {
        public int frameIndex;
        public int geometryIndex;
        public AtomicFlags flags;
        public int unused;

        public AtomicStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();
            
            frameIndex = binaryReader.ReadInt32();
            geometryIndex = binaryReader.ReadInt32();
            flags = (AtomicFlags)binaryReader.ReadInt32();
            unused = binaryReader.ReadInt32();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(frameIndex));
            listBytes.AddRange(BitConverter.GetBytes(geometryIndex));
            listBytes.AddRange(BitConverter.GetBytes((int)flags));
            listBytes.AddRange(BitConverter.GetBytes(unused));
        }
    }
}