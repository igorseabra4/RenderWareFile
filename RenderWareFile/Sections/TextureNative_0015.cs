using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureNative_0015 : RWSection
    {
        public TextureNativeStruct_0001 textureNativeStruct;
        public Extension_0003 textureNativeExtension;

        public TextureNative_0015 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.TextureNative;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section textureNativeStructSection = (Section)binaryReader.ReadInt32();
            if (textureNativeStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            textureNativeStruct = new TextureNativeStruct_0001().Read(binaryReader);

            Section textureNativeExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureNativeExtensionSection == Section.Extension)
                textureNativeExtension = new Extension_0003().Read(binaryReader);

            return this;
        }

        public TextureNative_0015 FromBytes(byte[] data)
        {
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(data));

            sectionIdentifier = (Section)binaryReader.ReadInt32();
            if (sectionIdentifier != Section.TextureNative) throw new Exception(binaryReader.BaseStream.Position.ToString());

            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section textureNativeStructSection = (Section)binaryReader.ReadInt32();
            if (textureNativeStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            textureNativeStruct = new TextureNativeStruct_0001().Read(binaryReader);

            Section textureNativeExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureNativeExtensionSection == Section.Extension)
                textureNativeExtension = new Extension_0003().Read(binaryReader);

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.TextureNative;

            listBytes.AddRange(textureNativeStruct.GetBytes(fileVersion));
            if (textureNativeExtension != null)
                listBytes.AddRange(textureNativeExtension.GetBytes(fileVersion));
        }
    }
}