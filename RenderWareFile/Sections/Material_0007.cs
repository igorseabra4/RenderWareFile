using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RenderWareFile
{
    public class Material_0007 : RWSection
    {
        public MaterialStruct_0001 materialStruct;
        public Texture_0006 texture;
        public Extension_0003 materialExtension;

        public Material_0007 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Material;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section materialListStructSection = (Section)binaryReader.ReadInt32();
            if (materialListStructSection != Section.Struct) throw new Exception();
            materialStruct = new MaterialStruct_0001().Read(binaryReader);

            if (materialStruct.isTextured != 0)
            {
                Section textureSection = (Section)binaryReader.ReadInt32();
                if (textureSection != Section.Texture) throw new Exception();
                texture = new Texture_0006().Read(binaryReader);
            }

            Section materialExtensionSection = (Section)binaryReader.ReadInt32();
            if (materialExtensionSection != Section.Extension) throw new Exception();
            materialExtension = new Extension_0003().Read(binaryReader);

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Material;

            listBytes.AddRange(materialStruct.GetBytes(fileVersion));
            if (materialStruct.isTextured != 0)
                listBytes.AddRange(texture.GetBytes(fileVersion));
            listBytes.AddRange(materialExtension.GetBytes(fileVersion));
        }
    }

    public class MaterialStruct_0001 : RWSection
    {
        public int unusedFlags;
        public Color color;
        public int unusedInt2;
        public int isTextured;
        public float ambient;
        public float specular;
        public float diffuse;

        public MaterialStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            unusedFlags = binaryReader.ReadInt32();
            byte R = binaryReader.ReadByte();
            byte G = binaryReader.ReadByte();
            byte B = binaryReader.ReadByte();
            byte A = binaryReader.ReadByte();
            color = new Color() { R = R, G = G, B = B, A = A };
            unusedInt2 = binaryReader.ReadInt32();
            isTextured = binaryReader.ReadInt32();
            ambient = binaryReader.ReadSingle();
            specular = binaryReader.ReadSingle();
            diffuse = binaryReader.ReadSingle();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(unusedFlags));
            listBytes.AddRange(BitConverter.GetBytes((int)color));
            listBytes.AddRange(BitConverter.GetBytes(unusedInt2));
            listBytes.AddRange(BitConverter.GetBytes(isTextured));
            listBytes.AddRange(BitConverter.GetBytes(ambient));
            listBytes.AddRange(BitConverter.GetBytes(specular));
            listBytes.AddRange(BitConverter.GetBytes(diffuse));
        }
    }
}