using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureDictionaryStruct_0001 : RWSection
    {
        public short textureCount;
        public short unknown;

        public TextureDictionaryStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            textureCount = binaryReader.ReadInt16();
            unknown = binaryReader.ReadInt16();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(textureCount));
        }
    }
}