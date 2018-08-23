using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public struct MipMapEntry
    {
        public int dataSize;
        public byte[] data;
        
        public MipMapEntry(int dataSize, byte[] data)
        {
            this.dataSize = dataSize;
            this.data = data;
        }
    }

    public class TextureNativeStruct_0001 : RWSection
    {
        public int unknown8;
        public TextureFilterMode filterMode;
        public TextureAddressMode addressModeU; // half a byte
        public TextureAddressMode addressModeV; // half a byte
        public string textureName;
        public string alphaName;
        public TextureRasterFormat rasterFormatFlags;
        public bool hasAlpha;
        public short width;
        public short height;
        public byte bitDepth;
        public byte mipMapCount;
        public byte type;
        public byte compression;
        public Color[] pallete;
        public MipMapEntry[] mipMaps;

        public TextureNativeStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            unknown8 = binaryReader.ReadInt32();

            filterMode = (TextureFilterMode)binaryReader.ReadByte();
            byte addressMode = binaryReader.ReadByte();
            addressModeU = (TextureAddressMode)(addressMode & 0xF0);
            addressModeV = (TextureAddressMode)(addressMode & 0x0F);
            binaryReader.BaseStream.Position += 2;
            
            long posBeforeString = binaryReader.BaseStream.Position;

            List<char> chars = new List<char>();
            char c = binaryReader.ReadChar();
            while (c != '\0')
            {
                chars.Add(c);
                c = binaryReader.ReadChar();
            }
            textureName = new string(chars.ToArray());

            binaryReader.BaseStream.Position = posBeforeString + 32;

            chars = new List<char>();
            c = binaryReader.ReadChar();
            while (c != '\0')
            {
                chars.Add(c);
                c = binaryReader.ReadChar();
            }
            alphaName = new string(chars.ToArray());

            binaryReader.BaseStream.Position = posBeforeString + 64;
            
            rasterFormatFlags = (TextureRasterFormat)binaryReader.ReadInt16();
            binaryReader.BaseStream.Position += 2;
            hasAlpha = binaryReader.ReadInt32() != 0;
            width = binaryReader.ReadInt16();
            height = binaryReader.ReadInt16();

            bitDepth = binaryReader.ReadByte();
            mipMapCount = binaryReader.ReadByte();
            type = binaryReader.ReadByte();
            compression = binaryReader.ReadByte();

            int palleteSize = 
                ((rasterFormatFlags & TextureRasterFormat.RASTER_PAL4) != 0) ? 0x80 / 4 :
                ((rasterFormatFlags & TextureRasterFormat.RASTER_PAL8) != 0) ? 0x400 / 4 : 0;

            if (palleteSize != 0)
            {
                pallete = new Color[palleteSize];
                for (int i = 0; i < palleteSize; i++)
                    pallete[i] = new Color(binaryReader.ReadInt32());
            }

            mipMaps = new MipMapEntry[mipMapCount];
            for (int i = 0; i < mipMapCount; i++)
            {
                int dataSize = binaryReader.ReadInt32();
                byte[] data = binaryReader.ReadBytes(dataSize);

                mipMaps[i] = new MipMapEntry(dataSize, data);
            }

            if (binaryReader.BaseStream.Position != startSectionPosition + sectionSize)
                throw new Exception(binaryReader.BaseStream.Position.ToString());

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(unknown8));

            listBytes.Add((byte)filterMode);
            listBytes.Add((byte)((byte)addressModeV + 16 * (byte)addressModeU));
            listBytes.Add(0);
            listBytes.Add(0);

            foreach (char i in textureName)
                listBytes.Add((byte)i);
            for (int i = textureName.Length; i < 32; i++)
                listBytes.Add(0);
            foreach (char i in alphaName)
                listBytes.Add((byte)i);
            for (int i = alphaName.Length; i < 32; i++)
                listBytes.Add(0);

            listBytes.AddRange(BitConverter.GetBytes((short)rasterFormatFlags));
            listBytes.Add(0);
            listBytes.Add(0);

            listBytes.AddRange(BitConverter.GetBytes(hasAlpha ? 1 : 0));
            listBytes.AddRange(BitConverter.GetBytes(width));
            listBytes.AddRange(BitConverter.GetBytes(height));

            listBytes.Add(bitDepth);
            listBytes.Add(mipMapCount);
            listBytes.Add(type);
            listBytes.Add(compression);

            foreach (MipMapEntry i in mipMaps)
            {
                listBytes.AddRange(BitConverter.GetBytes(i.dataSize));
                foreach (byte j in i.data)
                    listBytes.Add(j);
            }
        }
    }
}