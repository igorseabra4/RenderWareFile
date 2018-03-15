using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RenderWareFile
{    
    public class Texture_0006 : RWSection
    {
        public TextureStruct_0001 textureStruct;
        public String_0002 diffuseTextureName;
        public String_0002 alphaTextureName;
        public Extension_0003 textureExtension;

        public Texture_0006 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Texture;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section textureStructSection = (Section)binaryReader.ReadInt32();
            if (textureStructSection != Section.Struct) throw new Exception();
            textureStruct = new TextureStruct_0001().Read(binaryReader);

            Section diffuseTextureNameSection = (Section)binaryReader.ReadInt32();
            if (diffuseTextureNameSection != Section.String) throw new Exception();
            diffuseTextureName = new String_0002().Read(binaryReader);

            Section alphaTextureNameSection = (Section)binaryReader.ReadInt32();
            if (alphaTextureNameSection != Section.String) throw new Exception();
            alphaTextureName = new String_0002().Read(binaryReader);

            Section textureExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureExtensionSection != Section.Extension) throw new Exception();
            textureExtension = new Extension_0003().Read(binaryReader);

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Texture;

            listBytes.AddRange(textureStruct.GetBytes(fileVersion));
            listBytes.AddRange(diffuseTextureName.GetBytes(fileVersion));
            listBytes.AddRange(alphaTextureName.GetBytes(fileVersion));
            listBytes.AddRange(textureExtension.GetBytes(fileVersion));
        }
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
}
