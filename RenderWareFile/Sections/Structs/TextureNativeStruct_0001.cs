using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public TexturePlatformID platformType;

        private int id;
        public TextureFilterMode filterMode
        {
            get => (TextureFilterMode)(id & 0xFF);
            set { id = (id & ~0xff) | ((int)value & 0xff); }
        }
        public TextureAddressMode addressModeU
        {
            get => (TextureAddressMode)((id & 0xF000) >> 12);
            set { id = (id & ~0xF000) | ((int)value << 12); }
        }
        public TextureAddressMode addressModeV
        {
            get => (TextureAddressMode)((id & 0xF00) >> 8);
            set { id = (id & ~0xF00) | ((int)value << 8); }
        }
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

        #region Gamecube
        public uint maxAniso;
        public int biasClamp;
        public int edgeLod;
        public float lodBias;
        #endregion

        private int totalMipMapDataSize;

        private byte[] sectionData;
        private long startSectionPosition;

        public TextureNativeStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            startSectionPosition = binaryReader.BaseStream.Position;

            platformType = (TexturePlatformID)binaryReader.ReadInt32();

            id = platformType == TexturePlatformID.GameCube ? Shared.Switch(binaryReader.ReadInt32()) : binaryReader.ReadInt32();

            switch (platformType)
            {
                case TexturePlatformID.PCD3D8:
                case TexturePlatformID.PCD3D9:
                case TexturePlatformID.Xbox:
                    ReadNormalData(binaryReader, (int)startSectionPosition + sectionSize); break;
                case TexturePlatformID.GameCube:
                    ReadGameCubeData(binaryReader); break;
                case TexturePlatformID.PS2:
                    return this;
                default:
                    throw new InvalidDataException("Unsupported texture format: " + platformType.ToString());

            }

            if (binaryReader.BaseStream.Position != startSectionPosition + sectionSize)
                throw new Exception(binaryReader.BaseStream.Position.ToString());

            return this;
        }

        private void ReadNormalData(BinaryReader binaryReader, int endOfSectionPosition)
        {
            textureName = ReadString(binaryReader);
            alphaName = ReadString(binaryReader);

            rasterFormatFlags = (TextureRasterFormat)binaryReader.ReadUInt32();
            hasAlpha = binaryReader.ReadInt32() != 0;
            width = binaryReader.ReadInt16();
            height = binaryReader.ReadInt16();

            bitDepth = binaryReader.ReadByte();
            mipMapCount = binaryReader.ReadByte();
            type = binaryReader.ReadByte();
            compression = binaryReader.ReadByte();

            if (platformType == TexturePlatformID.Xbox)
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

                if (platformType != TexturePlatformID.Xbox)
                    dataSize = binaryReader.ReadInt32();
                else
                    dataSize = BiggestPowerOfTwoUnder(totalMipMapDataSize - passedSize);

                byte[] data = binaryReader.ReadBytes(dataSize);
                mipMaps[i] = new MipMapEntry(dataSize, data);

                passedSize += dataSize;
            }
        }

        private int BiggestPowerOfTwoUnder(int number)
        {
            return (int)Math.Pow(2, (Math.Floor(Math.Log(number, 2))));
        }

        private void ReadGameCubeData(BinaryReader binaryReader)
        {
            maxAniso = Shared.Switch(binaryReader.ReadUInt32());
            biasClamp = Shared.Switch(binaryReader.ReadInt32());
            edgeLod = Shared.Switch(binaryReader.ReadInt32());
            lodBias = Shared.Switch(binaryReader.ReadSingle());

            textureName = ReadString(binaryReader);
            alphaName = ReadString(binaryReader);

            rasterFormatFlags = (TextureRasterFormat)Shared.Switch(binaryReader.ReadInt32());
            width = Shared.Switch(binaryReader.ReadInt16());
            height = Shared.Switch(binaryReader.ReadInt16());

            bitDepth = binaryReader.ReadByte();
            mipMapCount = binaryReader.ReadByte();
            type = binaryReader.ReadByte();
            compression = binaryReader.ReadByte();

            if (ReadFileMethods.treatStuffAsByteArray)
            {
                sectionData = binaryReader.ReadBytes((int)(sectionSize - (binaryReader.BaseStream.Position - startSectionPosition)));
                return;
            }

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

            listBytes.AddRange(BitConverter.GetBytes((int)platformType));
            listBytes.AddRange(platformType == TexturePlatformID.GameCube ? BitConverter.GetBytes(id).Reverse() : BitConverter.GetBytes(id));

            switch (platformType)
            {
                case TexturePlatformID.PS2:
                    return;
                case TexturePlatformID.PCD3D8:
                case TexturePlatformID.PCD3D9:
                case TexturePlatformID.Xbox:
                    SetNormalListBytes(fileVersion, ref listBytes); break;
                case TexturePlatformID.GameCube:
                    SetGameCubeListBytes(fileVersion, ref listBytes); break;
                default:
                    throw new NotImplementedException("Unsupported writing of this platform type");
            }
            
        }

        private void SetNormalListBytes(int fileVersion, ref List<byte> listBytes)
        {
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

            if (platformType == TexturePlatformID.Xbox)
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
                if (platformType != TexturePlatformID.Xbox)
                    listBytes.AddRange(BitConverter.GetBytes(i.dataSize));

                foreach (byte j in i.data)
                    listBytes.Add(j);
            }
        }
        private void SetGameCubeListBytes(int fileVersion, ref List<byte> listBytes)
        {
            listBytes.AddRange(BitConverter.GetBytes(maxAniso).Reverse());
            listBytes.AddRange(BitConverter.GetBytes(biasClamp).Reverse());
            listBytes.AddRange(BitConverter.GetBytes(edgeLod).Reverse());
            listBytes.AddRange(BitConverter.GetBytes(lodBias).Reverse());

            for (int i = 0; i < 32; i++)
            {
                if (i < textureName.Length)
                    listBytes.Add((byte)textureName[i]);
                else
                    listBytes.Add(0);
            }

            for (int i = 0; i < 32; i++)
            {
                if (i < alphaName.Length)
                    listBytes.Add((byte)alphaName[i]);
                else
                    listBytes.Add(0);
            }

            listBytes.AddRange(BitConverter.GetBytes((int)rasterFormatFlags).Reverse());
            listBytes.AddRange(BitConverter.GetBytes(width).Reverse());
            listBytes.AddRange(BitConverter.GetBytes(height).Reverse());
            listBytes.Add(bitDepth);
            listBytes.Add(mipMapCount);
            listBytes.Add(type);
            listBytes.Add(compression);

            if (ReadFileMethods.treatStuffAsByteArray)
            {
                listBytes.AddRange(sectionData);
                return;
            }
            else throw new NotImplementedException("Can't write GameCube texture as actual data yet.");
        }
    }
}