using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class MaterialListStruct_0001 : RWSection
    {
        public int materialCount;

        public MaterialListStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            materialCount = binaryReader.ReadInt32();
            for (int i = 0; i < materialCount; i++)
            {
                binaryReader.ReadInt32();
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(materialCount));
            for (int i = 0; i < materialCount; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(-1));
            }
        }
    }
}
