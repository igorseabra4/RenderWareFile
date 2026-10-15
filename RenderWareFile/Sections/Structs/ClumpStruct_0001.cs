using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class ClumpStruct_0001 : RWSection
    {
        public int atomicCount { get; internal set; }
        public int lightCount { get; internal set; }
        public int cameraCount { get; internal set; }

        public ClumpStruct_0001(int atomicCount, int lightCount, int cameraCount)
        {
            this.atomicCount = atomicCount;
            this.lightCount = lightCount;
            this.cameraCount = cameraCount;
        }

        public ClumpStruct_0001(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            atomicCount = binaryReader.ReadInt32();

            if (renderWareVersion >= (3, 3))
            {
                lightCount = binaryReader.ReadInt32();
                cameraCount = binaryReader.ReadInt32();
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(atomicCount));
            if (fileVersion < (3,3))
                return;
            listBytes.AddRange(BitConverter.GetBytes(lightCount));
            listBytes.AddRange(BitConverter.GetBytes(cameraCount));
        }
    }
}
