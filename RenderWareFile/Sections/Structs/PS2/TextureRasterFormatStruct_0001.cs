using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RenderWareFile.Sections.Structs.PS2
{
    public class TextureRasterFormatStruct_0001 : RWSection
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Depth { get; set; }

        public TextureRasterFormat RasterFormat { get; set; }

        public ulong TEX0 { get; set; }
        public ulong TEX1 { get; set; }
        public ulong MipTbp1 { get; set; }
        public ulong MipTbp2 { get; set; }

        public byte PrivateFlags { get; set; }
        public ushort SkyRasterVersion { get; set; }

        public int gsMipmapDataSize { get; set; }
        public int gsCLUTDataSize { get; set; }
        public int textureMemorySize { get; set; }
        public int skyMipmapVal { get; set; }

        public byte[] sectionData { get; set; }

        public TextureRasterFormatStruct_0001(BinaryReader reader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = reader.ReadInt32();
            renderWareVersion = reader.ReadInt32();

            reader.BaseStream.Position += 12;

            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Depth = reader.ReadUInt32();

            ushort rasterFormat = reader.ReadUInt16();
            RasterFormat = (TextureRasterFormat)(rasterFormat >> 8);
            PrivateFlags = (byte)(rasterFormat & 0xff);
            SkyRasterVersion = reader.ReadUInt16();
            TEX0 = reader.ReadUInt64();
            TEX1 = reader.ReadUInt64();
            MipTbp1 = reader.ReadUInt64();
            MipTbp2 = reader.ReadUInt64();
            gsMipmapDataSize = reader.ReadInt32();
            gsCLUTDataSize = reader.ReadInt32();
            textureMemorySize = reader.ReadInt32();
            skyMipmapVal = reader.ReadInt32();

            reader.ReadInt32();
            int sizeOfData = reader.ReadInt32();
            reader.ReadInt32();
            sectionData = reader.ReadBytes(sizeOfData);
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            listBytes.AddRange(BitConverter.GetBytes(1));
            listBytes.AddRange(BitConverter.GetBytes(0x40));
            listBytes.AddRange(BitConverter.GetBytes(fileVersion));
            listBytes.AddRange(BitConverter.GetBytes(Width));
            listBytes.AddRange(BitConverter.GetBytes(Height));
            listBytes.AddRange(BitConverter.GetBytes(Depth));
            listBytes.Add(PrivateFlags);
            listBytes.Add((byte)RasterFormat);
            listBytes.AddRange(BitConverter.GetBytes(SkyRasterVersion));
            listBytes.AddRange(BitConverter.GetBytes(TEX0));
            listBytes.AddRange(BitConverter.GetBytes(TEX1));
            listBytes.AddRange(BitConverter.GetBytes(MipTbp1));
            listBytes.AddRange(BitConverter.GetBytes(MipTbp2));
            listBytes.AddRange(BitConverter.GetBytes(gsMipmapDataSize));
            listBytes.AddRange(BitConverter.GetBytes(gsCLUTDataSize));
            listBytes.AddRange(BitConverter.GetBytes(textureMemorySize));
            listBytes.AddRange(BitConverter.GetBytes(skyMipmapVal));

            listBytes.AddRange(BitConverter.GetBytes(1));
            listBytes.AddRange(BitConverter.GetBytes(sectionData.Length));
            listBytes.AddRange(BitConverter.GetBytes(fileVersion));
            listBytes.AddRange(sectionData);
        }
    }
}
