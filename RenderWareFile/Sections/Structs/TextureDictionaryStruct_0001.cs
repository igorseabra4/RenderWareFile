using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureDictionaryStruct_0001 : RWSection
    {
        public short textureCount { get; internal set; }
        public short unknown { get; set; }

        public TextureDictionaryStruct_0001(short textureCount, short unknown = 0)
        {
            this.textureCount = textureCount;
            this.unknown = unknown;
        }

        public TextureDictionaryStruct_0001(BinaryReader binaryReader)
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