using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace RenderWareFile.Sections
{
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