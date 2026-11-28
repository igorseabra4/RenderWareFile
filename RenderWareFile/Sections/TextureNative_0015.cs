using RenderWareFile.Textures;
using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureNative_0015 : RWSection
    {
        public TextureNativeStruct_0001 textureNativeStruct { get; set; }
        public Ps2TextureRasterFormatStruct_0001 PS2RasterFormat { get; set; }
        public Extension_0003 textureNativeExtension { get; set; }

        public TextureNative_0015() { }
        public TextureNative_0015(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.TextureNative;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            Section textureNativeStructSection = (Section)binaryReader.ReadInt32();
            if (textureNativeStructSection != Section.Struct)
                throw new Exception(binaryReader.BaseStream.Position.ToString());
            textureNativeStruct = new TextureNativeStruct_0001(binaryReader);

            TexturePlatformHandler.Get(textureNativeStruct.platformType).ReadTrailingSections(binaryReader, this);

            Section textureNativeExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureNativeExtensionSection == Section.Extension)
                textureNativeExtension = new Extension_0003(binaryReader);
        }

        public TextureNative_0015(byte[] data)
        {
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(data));

            sectionIdentifier = (Section)binaryReader.ReadInt32();
            if (sectionIdentifier != Section.TextureNative) throw new Exception(binaryReader.BaseStream.Position.ToString());

            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            Section textureNativeStructSection = (Section)binaryReader.ReadInt32();
            if (textureNativeStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            textureNativeStruct = new TextureNativeStruct_0001(binaryReader);

            TexturePlatformHandler.Get(textureNativeStruct.platformType).ReadTrailingSections(binaryReader, this);

            Section textureNativeExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureNativeExtensionSection == Section.Extension)
                textureNativeExtension = new Extension_0003(binaryReader);
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.TextureNative;

            listBytes.AddRange(textureNativeStruct.GetBytes(fileVersion));

            TexturePlatformHandler.Get(textureNativeStruct.platformType).WriteTrailingSections(this, fileVersion, listBytes);

            if (textureNativeExtension != null)
                listBytes.AddRange(textureNativeExtension.GetBytes(fileVersion));
        }
    }
}