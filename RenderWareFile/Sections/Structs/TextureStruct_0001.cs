using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureStruct_0001 : RWSection
    {
        public TextureFilterMode filterMode;
        public TextureAddressMode addressModeU; // half a byte
        public TextureAddressMode addressModeV; // half a byte
        public ushort useMipLevels;

        public TextureStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            filterMode = (TextureFilterMode)binaryReader.ReadByte();

            byte addressMode = binaryReader.ReadByte();
            addressModeU = (TextureAddressMode)((addressMode & 0xF0) >> 4);
            addressModeV = (TextureAddressMode)(addressMode & 0x0F);

            useMipLevels = binaryReader.ReadUInt16();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.Add((byte)filterMode);
            listBytes.Add((byte)((byte)addressModeV + ((byte)addressModeU << 4)));
            listBytes.AddRange(BitConverter.GetBytes(useMipLevels));            
        }
    }
}
