using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public enum FilterMode : byte
    {
        FILTERNAFILTERMODE = 0,
        FILTERNEAREST = 1,
        FILTERLINEAR = 2,
        FILTERMIPNEAREST = 3,
        FILTERMIPLINEAR = 4,
        FILTERLINEARMIPNEAREST = 5,
        FILTERLINEARMIPLINEAR = 6
    }

    public enum AddressMode : byte
    {
        TEXTUREADDRESSNATEXTUREADDRESS = 0,
        TEXTUREADDRESSWRAP = 1,
        TEXTUREADDRESSMIRROR = 2,
        TEXTUREADDRESSCLAMP = 3,
        TEXTUREADDRESSBORDER = 4
    }

    public class TextureStruct_0001 : RWSection
    {
        public FilterMode filterMode;
        public AddressMode addressModeU;
        public AddressMode addressModeV;
        public ushort useMipLevels;

        public TextureStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            filterMode = (FilterMode)binaryReader.ReadByte();

            byte addressMode = binaryReader.ReadByte();
            addressModeU = (AddressMode)(addressMode & 0xF0);
            addressModeV = (AddressMode)(addressMode & 0x0F);

            useMipLevels = binaryReader.ReadUInt16();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.Add((byte)filterMode);
            listBytes.Add((byte)((byte)addressModeV + 16 * (byte)addressModeU));
            listBytes.AddRange(BitConverter.GetBytes(useMipLevels));            
        }
    }
}
