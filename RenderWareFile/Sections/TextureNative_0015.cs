using RenderWareFile.Sections.Structs.PS2;
using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class TextureNative_0015 : RWSection
    {
        public TextureNativeStruct_0001 textureNativeStruct;
        public TextureRasterFormatStruct_0001 PS2RasterFormat;
        public Extension_0003 textureNativeExtension;

        public TextureNative_0015 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.TextureNative;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            try
            {
                Section textureNativeStructSection = (Section)binaryReader.ReadInt32();
                if (textureNativeStructSection != Section.Struct)
                    throw new Exception(binaryReader.BaseStream.Position.ToString());
                textureNativeStruct = new TextureNativeStruct_0001();
                textureNativeStruct.Read(binaryReader);
            }
            catch (Exception ex)
            {
                throw new Exception(textureNativeStruct.textureName + ": " + ex.Message, ex);
            }

            if (textureNativeStruct.platformType == 0x325350)
            {
                binaryReader.ReadInt32();
                textureNativeStruct.textureName = new String_0002().Read(binaryReader).stringString;
                binaryReader.ReadInt32();
                textureNativeStruct.alphaName = new String_0002().Read(binaryReader).stringString;

                binaryReader.ReadInt32();
                PS2RasterFormat = new TextureRasterFormatStruct_0001().Read(binaryReader);
            }

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

            if (textureNativeStruct.platformType == 0x325350)
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