using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureDictionary_0016 : RWSection
    {
        public TextureDictionaryStruct_0001 textureDictionaryStruct;
        public List<TextureNative_0015> textureNativeList;
        public Extension_0003 textureDictionaryExtension;

        public TextureDictionary_0016 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.TextureDictionary;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section textureDictionaryStructSection = (Section)binaryReader.ReadInt32();
            if (textureDictionaryStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            textureDictionaryStruct = new TextureDictionaryStruct_0001().Read(binaryReader);

            textureNativeList = new List<TextureNative_0015>();

            for (int i = 0; i < textureDictionaryStruct.textureCount; i++)
            {
                Section textureNativeSection = (Section)binaryReader.ReadInt32();
                if (textureNativeSection != Section.TextureNative) throw new Exception(binaryReader.BaseStream.Position.ToString());
                textureNativeList.Add(new TextureNative_0015().Read(binaryReader));
            }
            
            Section textureDictionaryExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureDictionaryExtensionSection == Section.Extension)
                textureDictionaryExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.TextureDictionary;

            listBytes.AddRange(textureDictionaryStruct.GetBytes(fileVersion));

            foreach (TextureNative_0015 i in textureNativeList)
                listBytes.AddRange(i.GetBytes(fileVersion));

            listBytes.AddRange(textureDictionaryExtension.GetBytes(fileVersion));
        }
    }
}
