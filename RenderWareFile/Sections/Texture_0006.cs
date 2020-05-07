using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace RenderWareFile.Sections
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Texture_0006 : RWSection
    {
        public TextureStruct_0001 textureStruct;
        public String_0002 diffuseTextureName;
        public String_0002 alphaTextureName;
        public Extension_0003 textureExtension;

        public Texture_0006()
        {
            textureStruct = new TextureStruct_0001();
            diffuseTextureName = new String_0002();
            alphaTextureName = new String_0002();
            textureExtension = new Extension_0003();
        }

        public string DiffuseTextureName
        {
            get => diffuseTextureName.stringString;
            set => diffuseTextureName = new String_0002(value);
        }

        public string AlphaTextureName
        {
            get => alphaTextureName.stringString;
            set => alphaTextureName = new String_0002(value);
        }

        public TextureFilterMode FilterMode
        { 
            get => textureStruct.FilterMode; 
            set => textureStruct.FilterMode = value; 
        }
        public TextureAddressMode AddressModeU
        {
            get => textureStruct.AddressModeU;
            set => textureStruct.AddressModeU = value;
        }
        public TextureAddressMode AddressModeV
        {
            get => textureStruct.AddressModeV;
            set => textureStruct.AddressModeV = value;
        }
        public ushort UseMipLevels
        {
            get => textureStruct.UseMipLevels;
            set => textureStruct.UseMipLevels = value;
        }

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
}
