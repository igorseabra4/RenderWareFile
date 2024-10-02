using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RenderWareFile.Sections.Structs.PS2
{
    public class TextureRasterFormatStruct_0001 : RWSection
    {
        public uint Width;
        public uint Height;
        public uint Depth;

        public TextureRasterFormat RasterFormat;

        public ulong TEX0;
        public ulong TEX1;
        public ulong MipTbp1;
        public ulong MipTbp2;

        public byte PrivateFlags;
        public ushort SkyRasterVersion;

        public int gsMipmapDataSize;
        public int gsCLUTDataSize;
        public int textureMemorySize;
        public int skyMipmapVal;

        public byte[] sectionData;
        private long startSectionPostion;

        public TextureRasterFormatStruct_0001 Read(BinaryReader reader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = reader.ReadInt32();
            renderWareVersion = reader.ReadInt32();

            startSectionPostion = reader.BaseStream.Position;
 
            reader.BaseStream.Position += 12;

            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Depth = reader.ReadUInt32();

            SkyRasterVersion = reader.ReadUInt16();
            short rasterFormat = reader.ReadInt16();
            RasterFormat = (TextureRasterFormat)(rasterFormat & 0xFF);
            PrivateFlags = (byte)(rasterFormat >> 8);
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

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            listBytes.AddRange(BitConverter.GetBytes(1));
            listBytes.AddRange(BitConverter.GetBytes(0x40));
            listBytes.AddRange(BitConverter.GetBytes(fileVersion));
            listBytes.AddRange(BitConverter.GetBytes(Width));
            listBytes.AddRange(BitConverter.GetBytes(Height));
            listBytes.AddRange(BitConverter.GetBytes(Depth));
            listBytes.AddRange(BitConverter.GetBytes(SkyRasterVersion));
            listBytes.Add((byte)RasterFormat);
            listBytes.Add(PrivateFlags);
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
