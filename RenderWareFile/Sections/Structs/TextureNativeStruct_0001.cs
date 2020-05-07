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
        public int platformType;
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
        public Color[] palette;
        public MipMapEntry[] mipMaps;

        public int gcnUnknown1;
        public int gcnUnknown2;
        public int gcnUnknown3;
        public int gcnUnknown4;

        public String_0002 ps2TextureNameString;
        public String_0002 ps2AlphaNameString;

        private int totalMipMapDataSize;

        private byte[] sectionData;
        private long startSectionPosition;

        public TextureNativeStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();
            
            startSectionPosition = binaryReader.BaseStream.Position;

            platformType = binaryReader.ReadInt32();

            if (platformType == 8 | platformType == 5)
            {
                ReadNormalData(binaryReader, (int)startSectionPosition + sectionSize);
            }
            else if (platformType == 100663296)
            {
                ReadGameCubeData(binaryReader);
            }
            else if (platformType == 3298128)
            {
                ReadPS2Data(binaryReader);
                return this;
            }
            else throw new InvalidDataException("Unsupported texture format: " + platformType.ToString());

            if (binaryReader.BaseStream.Position != startSectionPosition + sectionSize)
                throw new Exception(binaryReader.BaseStream.Position.ToString());

            return this;
        }

        private void ReadNormalData(BinaryReader binaryReader, int endOfSectionPosition)
        {
            filterMode = (TextureFilterMode)binaryReader.ReadByte();
            byte addressMode = binaryReader.ReadByte();
            addressModeU = (TextureAddressMode)((addressMode & 0xF0) >> 4);
            addressModeV = (TextureAddressMode)(addressMode & 0x0F);
            binaryReader.BaseStream.Position += 2;

            textureName = ReadString(binaryReader);
            alphaName = ReadString(binaryReader);

            rasterFormatFlags = (TextureRasterFormat)binaryReader.ReadInt32();
            hasAlpha = binaryReader.ReadInt32() != 0;
            width = binaryReader.ReadInt16();
            height = binaryReader.ReadInt16();

            bitDepth = binaryReader.ReadByte();
            mipMapCount = binaryReader.ReadByte();
            type = binaryReader.ReadByte();
            compression = binaryReader.ReadByte();

            if (platformType == 5)
                totalMipMapDataSize = binaryReader.ReadInt32();

            int palleteSize =
                ((rasterFormatFlags & TextureRasterFormat.RASTER_PAL4) != 0) ? 0x80 / 4 :
                ((rasterFormatFlags & TextureRasterFormat.RASTER_PAL8) != 0) ? 0x400 / 4 : 0;

            if (palleteSize != 0)
            {
                palette = new Color[palleteSize];
                for (int i = 0; i < palleteSize; i++)
                    palette[i] = new Color(binaryReader.ReadInt32());
            }

            int passedSize = 0;
            mipMaps = new MipMapEntry[mipMapCount];
            for (int i = 0; i < mipMapCount; i++)
            {
                int dataSize = 0;

                if (platformType == 8)
                    dataSize = binaryReader.ReadInt32();
                else if (platformType == 5)
                    dataSize = BiggestPowerOfTwoUnder(totalMipMapDataSize - passedSize);

                byte[] data = binaryReader.ReadBytes(dataSize);
                mipMaps[i] = new MipMapEntry(dataSize, data);

                passedSize += dataSize;
            }
        }

        private void ReadPS2Data(BinaryReader binaryReader)
        {
            filterMode = (TextureFilterMode)binaryReader.ReadByte();
            byte addressMode = binaryReader.ReadByte();
            addressModeU = (TextureAddressMode)((addressMode & 0xF0) >> 4);
            addressModeV = (TextureAddressMode)(addressMode & 0x0F);
            binaryReader.BaseStream.Position += 2;

            binaryReader.ReadInt32();
            textureName = new String_0002().Read(binaryReader).stringString;
            binaryReader.ReadInt32();
            alphaName = new String_0002().Read(binaryReader).stringString;
            
            binaryReader.ReadInt32();
            int sizeOfdata = binaryReader.ReadInt32();
            binaryReader.ReadInt32();

            sectionData = binaryReader.ReadBytes(sizeOfdata);
        }

        private int BiggestPowerOfTwoUnder(int number)
        {
            return (int)Math.Pow(2, (Math.Floor(Math.Log(number, 2))));
        }

        private void ReadGameCubeData(BinaryReader binaryReader)
        {
            binaryReader.BaseStream.Position += 2;
            byte addressMode = binaryReader.ReadByte();
            addressModeU = (TextureAddressMode)((addressMode & 0xF0) >> 4);
            addressModeV = (TextureAddressMode)(addressMode & 0x0F);
            filterMode = (TextureFilterMode)binaryReader.ReadByte();

            gcnUnknown1 = Shared.Switch(binaryReader.ReadInt32());
            gcnUnknown2 = Shared.Switch(binaryReader.ReadInt32());
            gcnUnknown3 = Shared.Switch(binaryReader.ReadInt32());
            gcnUnknown4 = Shared.Switch(binaryReader.ReadInt32());

            textureName = ReadString(binaryReader);
            alphaName = ReadString(binaryReader);

            if (ReadFileMethods.treatStuffAsByteArray)
            {
                sectionData = binaryReader.ReadBytes((int)(sectionSize - (binaryReader.BaseStream.Position - startSectionPosition)));
                return;
            }

            rasterFormatFlags = (TextureRasterFormat)Shared.Switch(binaryReader.ReadInt32());
            width = Shared.Switch(binaryReader.ReadInt16());
            height = Shared.Switch(binaryReader.ReadInt16());

            bitDepth = binaryReader.ReadByte();
            mipMapCount = binaryReader.ReadByte();
            type = binaryReader.ReadByte();
            compression = binaryReader.ReadByte();

            int palleteSize =
                ((rasterFormatFlags & TextureRasterFormat.RASTER_PAL4) != 0) ? 0x80 / 4 :
                ((rasterFormatFlags & TextureRasterFormat.RASTER_PAL8) != 0) ? 0x400 / 4 : 0;

            if (palleteSize != 0)
            {
                palette = new Color[palleteSize];
                for (int i = 0; i < palleteSize; i++)
                    palette[i] = new Color(binaryReader.ReadInt32());
            }

            mipMaps = new MipMapEntry[mipMapCount];
            for (int i = 0; i < mipMapCount; i++)
            {
                int dataSize = Shared.Switch(binaryReader.ReadInt32());
                byte[] data = binaryReader.ReadBytes(dataSize);

                mipMaps[i] = new MipMapEntry(dataSize, data);
            }
        }

        private static string ReadString(BinaryReader binaryReader)
        {
            long posBeforeString = binaryReader.BaseStream.Position;

            List<char> chars = new List<char>();
            char c = binaryReader.ReadChar();
            while (c != '\0')
            {
                chars.Add(c);
                c = binaryReader.ReadChar();
            }

            binaryReader.BaseStream.Position = posBeforeString + 32;

            return new string(chars.ToArray());
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;
            
            listBytes.AddRange(BitConverter.GetBytes(platformType));

            if (platformType == 8 | platformType == 5)
            {
                SetNormalListBytes(fileVersion, ref listBytes);
            }
            else if (platformType == 100663296)
            {
                SetGameCubeListBytes(fileVersion, ref listBytes);
            }
            else if (platformType == 3298128)
            {
                SetPS2ListBytes(fileVersion, ref listBytes);
            }
            else throw new NotImplementedException("Unsupported writing of this platform type");
        }

        private void SetNormalListBytes(int fileVersion, ref List<byte> listBytes)
        {
            listBytes.Add((byte)filterMode);
            listBytes.Add((byte)((byte)addressModeV + ((byte)addressModeU << 4)));
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

            if (platformType == 5)
            {
                totalMipMapDataSize = 0;
                foreach (MipMapEntry i in mipMaps)
                    totalMipMapDataSize += i.dataSize;

                listBytes.AddRange(BitConverter.GetBytes(totalMipMapDataSize));
            }

            if (palette != null)
                foreach (Color c in palette)
                {
                    listBytes.Add(c.R);
                    listBytes.Add(c.G);
                    listBytes.Add(c.B);
                    listBytes.Add(c.A);
                }

            foreach (MipMapEntry i in mipMaps)
            {
                if (platformType == 8)
                    listBytes.AddRange(BitConverter.GetBytes(i.dataSize));

                foreach (byte j in i.data)
                    listBytes.Add(j);
            }
        }

        private void SetPS2ListBytes(int fileVersion, ref List<byte> listBytes)
        {
            listBytes.Add((byte)filterMode);
            listBytes.Add((byte)((byte)addressModeV + ((byte)addressModeU << 4)));
            listBytes.Add(0);
            listBytes.Add(0);

            listBytes.AddRange(new String_0002(textureName).GetBytes(fileVersion));
            listBytes.AddRange(new String_0002(alphaName).GetBytes(fileVersion));

            listBytes.Add(1);
            listBytes.Add(0);
            listBytes.Add(0);
            listBytes.Add(0);
            listBytes.AddRange(BitConverter.GetBytes(sectionData.Length));
            listBytes.AddRange(BitConverter.GetBytes(renderWareVersion));
            listBytes.AddRange(sectionData);
        }

        private void SetGameCubeListBytes(int fileVersion, ref List<byte> listBytes)
        {
            listBytes.Add(0);
            listBytes.Add(0);
            listBytes.Add((byte)((byte)addressModeV + ((byte)addressModeU << 4)));
            listBytes.Add((byte)filterMode);

            listBytes.AddRange(BitConverter.GetBytes(gcnUnknown1));
            listBytes.AddRange(BitConverter.GetBytes(gcnUnknown2));
            listBytes.AddRange(BitConverter.GetBytes(gcnUnknown3));
            listBytes.AddRange(BitConverter.GetBytes(gcnUnknown4));

            foreach (char i in textureName)
                listBytes.Add((byte)i);
            for (int i = textureName.Length; i < 32; i++)
                listBytes.Add(0);
            foreach (char i in alphaName)
                listBytes.Add((byte)i);
            for (int i = alphaName.Length; i < 32; i++)
                listBytes.Add(0);

            if (ReadFileMethods.treatStuffAsByteArray)
            {
                listBytes.AddRange(sectionData);
                return;
            }
            else throw new NotImplementedException("Can't write GameCube texture as actual data yet.");
        }
    }
}