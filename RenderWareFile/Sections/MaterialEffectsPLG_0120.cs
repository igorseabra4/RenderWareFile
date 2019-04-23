using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class MaterialEffectsPLG_0120 : RWSection
    {
        public int value;

        public MaterialEffectsPLG_0120 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.MaterialEffectsPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            value = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.MaterialEffectsPLG;

            listBytes.AddRange(BitConverter.GetBytes(value));
        }
    }
}
