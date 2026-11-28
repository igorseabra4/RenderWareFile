using RenderWareFile.Sections.Structs.PS2;
using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureNative_0015 : RWSection
    {
        public TextureNativeStruct_0001 textureNativeStruct { get; set; }
        public TextureRasterFormatStruct_0001 PS2RasterFormat { get; set; }
        public Extension_0003 textureNativeExtension { get; set; }

        public TextureNative_0015() { }
        public TextureNative_0015(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.TextureNative;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            try
            {
                Section textureNativeStructSection = (Section)binaryReader.ReadInt32();
                if (textureNativeStructSection != Section.Struct)
                    throw new Exception(binaryReader.BaseStream.Position.ToString());
                textureNativeStruct = new TextureNativeStruct_0001(binaryReader);
            }
            catch (Exception ex)
            {
                throw new Exception(textureNativeStruct.textureName + ": " + ex.Message, ex);
            }

            if (textureNativeStruct.platformType == TexturePlatformID.PS2)
            {
                binaryReader.ReadInt32();
                textureNativeStruct.textureName = new String_0002(binaryReader).stringString;
                binaryReader.ReadInt32();
                textureNativeStruct.alphaName = new String_0002(binaryReader).stringString;

                binaryReader.ReadInt32();
                PS2RasterFormat = new TextureRasterFormatStruct_0001(binaryReader);
            }

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

            Section textureNativeExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureNativeExtensionSection == Section.Extension)
                textureNativeExtension = new Extension_0003(binaryReader);
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.TextureNative;

            listBytes.AddRange(textureNativeStruct.GetBytes(fileVersion));

            if (textureNativeStruct.platformType == TexturePlatformID.PS2)
            {
                listBytes.AddRange(new String_0002(textureNativeStruct.textureName).GetBytes(fileVersion));
                listBytes.AddRange(new String_0002(textureNativeStruct.alphaName).GetBytes(fileVersion));
                listBytes.AddRange(PS2RasterFormat?.GetBytes(fileVersion));
            }

            if (textureNativeExtension != null)
                listBytes.AddRange(textureNativeExtension.GetBytes(fileVersion));
        }
    }
}