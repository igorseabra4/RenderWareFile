using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class Material_0007 : RWSection
    {
        public MaterialStruct_0001 materialStruct { get; set; }
        public Texture_0006 texture { get; set; }
        public Extension_0003 materialExtension { get; set; }

        public bool IsTextured
        {
            get => materialStruct.isTextured != 0;
            set
            {
                if (value)
                {
                    materialStruct.isTextured = 1;
                    texture ??= new Texture_0006();
                }
                else
                {
                    materialStruct.isTextured = 0;
                    texture = null;
                }
            }
        }

        public Material_0007(MaterialStruct_0001 materialStruct, Texture_0006 texture, Extension_0003 materialExtension)
        {
            this.materialStruct = materialStruct;
            this.texture = texture;
            this.materialExtension = materialExtension;
        }

        public Material_0007(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Material;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            Section materialListStructSection = (Section)binaryReader.ReadInt32();
            if (materialListStructSection != Section.Struct) throw new Exception();
            materialStruct = new MaterialStruct_0001(binaryReader);

            if (materialStruct.isTextured != 0)
            {
                Section textureSection = (Section)binaryReader.ReadInt32();
                if (textureSection != Section.Texture) throw new Exception();
                texture = new Texture_0006(binaryReader);
            }

            Section materialExtensionSection = (Section)binaryReader.ReadInt32();
            if (materialExtensionSection != Section.Extension) throw new Exception();
            materialExtension = new Extension_0003(binaryReader);
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Material;

            listBytes.AddRange(materialStruct.GetBytes(fileVersion));
            if (materialStruct.isTextured != 0)
                listBytes.AddRange(texture.GetBytes(fileVersion));
            listBytes.AddRange(materialExtension.GetBytes(fileVersion));
        }

        public override string ToString()
        {
            if (texture != null)
                return texture.diffuseTextureName.stringString;
            return "Diffuse Color: " + materialStruct.color.ToString();
        }
    }
}