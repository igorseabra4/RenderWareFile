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
        public int frameIndex { get; set; }
        public int geometryIndex { get; set; }
        public AtomicFlags flags { get; set; }
        public int unused { get; set; }

        public AtomicStruct_0001(int frameIndex, int geometryIndex, AtomicFlags flags, int unused = 0)
        {
            this.frameIndex = frameIndex;
            this.geometryIndex = geometryIndex;
            this.flags = flags;
            this.unused = unused;
        }

        public AtomicStruct_0001(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            frameIndex = binaryReader.ReadInt32();
            geometryIndex = binaryReader.ReadInt32();
            flags = (AtomicFlags)binaryReader.ReadInt32();
            unused = binaryReader.ReadInt32();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(frameIndex));
            listBytes.AddRange(BitConverter.GetBytes(geometryIndex));
            listBytes.AddRange(BitConverter.GetBytes((int)flags));
            listBytes.AddRange(BitConverter.GetBytes(unused));
        }
    }
}