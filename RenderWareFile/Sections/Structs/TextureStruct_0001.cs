using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureStruct_0001 : RWSection
    {
        public TextureFilterMode FilterMode { get; set; }
        public TextureAddressMode AddressModeU { get; set; } // half a byte
        public TextureAddressMode AddressModeV { get; set; } // half a byte
        public ushort UseMipLevels { get; set; }

        public TextureStruct_0001()
        {
            sectionIdentifier = Section.Struct;
            FilterMode = TextureFilterMode.FILTERLINEARMIPLINEAR;

            AddressModeU = TextureAddressMode.TEXTUREADDRESSWRAP;
            AddressModeV = TextureAddressMode.TEXTUREADDRESSWRAP;

            UseMipLevels = 1;
        }

        public TextureStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            FilterMode = (TextureFilterMode)binaryReader.ReadByte();

            byte addressMode = binaryReader.ReadByte();
            AddressModeU = (TextureAddressMode)((addressMode & 0xF0) >> 4);
            AddressModeV = (TextureAddressMode)(addressMode & 0x0F);

            UseMipLevels = binaryReader.ReadUInt16();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.Add((byte)FilterMode);
            listBytes.Add((byte)((byte)AddressModeV + ((byte)AddressModeU << 4)));
            listBytes.AddRange(BitConverter.GetBytes(UseMipLevels));
        }
    }
}
