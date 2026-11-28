using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureDictionary_0016 : RWSection
    {
        public TextureDictionaryStruct_0001 textureDictionaryStruct { get; internal set; }
        public List<TextureNative_0015> textureNativeList { get; set; }
        public Extension_0003 textureDictionaryExtension { get; set; }

        public TextureDictionary_0016(List<TextureNative_0015> textureNativeList, Extension_0003 textureDictionaryExtension = null)
        {
            this.textureDictionaryStruct = new TextureDictionaryStruct_0001((short)textureNativeList.Count);
            this.textureNativeList = textureNativeList;
            this.textureDictionaryExtension = textureDictionaryExtension ?? new Extension_0003();
        }

        public TextureDictionary_0016(TextureNative_0015 textureNative, Extension_0003 textureDictionaryExtension = null) :
            this(new List<TextureNative_0015>() { textureNative }, textureDictionaryExtension)
        { }

        public TextureDictionary_0016(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.TextureDictionary;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section textureDictionaryStructSection = (Section)binaryReader.ReadInt32();
            if (textureDictionaryStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            textureDictionaryStruct = new TextureDictionaryStruct_0001(binaryReader);

            textureNativeList = new List<TextureNative_0015>();

            for (int i = 0; i < textureDictionaryStruct.textureCount; i++)
            {
                Section textureNativeSection = (Section)binaryReader.ReadInt32();
                if (textureNativeSection != Section.TextureNative) throw new Exception(binaryReader.BaseStream.Position.ToString());
                textureNativeList.Add(new TextureNative_0015(binaryReader));
            }

            Section textureDictionaryExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureDictionaryExtensionSection == Section.Extension)
                textureDictionaryExtension = new Extension_0003(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.TextureDictionary;

            listBytes.AddRange(textureDictionaryStruct.GetBytes(fileVersion));

            foreach (TextureNative_0015 i in textureNativeList)
                listBytes.AddRange(i.GetBytes(fileVersion));

            listBytes.AddRange(textureDictionaryExtension.GetBytes(fileVersion));
        }
    }
}
