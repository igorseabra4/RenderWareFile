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
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            if (renderWareVersion > (3,5))
            {
                textureCount = binaryReader.ReadInt16();
                unknown = binaryReader.ReadInt16();
            }
            else
                textureCount = (short)binaryReader.ReadUInt32();

            return this;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            if (fileVersion > (3,5))
            {
                listBytes.AddRange(BitConverter.GetBytes(textureCount));
                listBytes.AddRange(BitConverter.GetBytes(unknown));
            }
            else
                listBytes.AddRange(BitConverter.GetBytes((int)textureCount));
        }
    }
}