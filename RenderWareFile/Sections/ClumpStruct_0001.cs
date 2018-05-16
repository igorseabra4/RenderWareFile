using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class ClumpStruct_0001 : RWSection
    {
        public int atomicCount;
        public int lightCount;
        public int cameraCount;

        public ClumpStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            atomicCount = binaryReader.ReadInt32();
            lightCount = binaryReader.ReadInt32();
            cameraCount = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(atomicCount));
            if (renderWareVersion == 0x0310)
                return;
            listBytes.AddRange(BitConverter.GetBytes(lightCount));
            listBytes.AddRange(BitConverter.GetBytes(cameraCount));
        }
    }
}
