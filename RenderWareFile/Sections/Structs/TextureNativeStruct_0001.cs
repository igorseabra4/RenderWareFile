using RenderWareFile.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareFile.Sections
{
    /// <summary>
    /// Texture native struct. Holds the data common to all platforms; the platform specific
    /// reading and writing is done by the TexturePlatformHandler classes.
    /// </summary>
    public class TextureNativeStruct_0001 : RWSection
    {
        public TexturePlatformID platformType { get; set; }

        internal int id;
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
        private string _textureName;
        public string textureName { get => _textureName; set { _textureName = value.Length > 32 ? value[..32] : value; } }
        private string _alphaName;
        public string alphaName { get => _alphaName; set { _alphaName = value.Length > 32 ? value[..32] : value; } }
        public TextureRasterFormat rasterFormatFlags { get; set; }
        public bool hasAlpha { get; set; }
        public uint d3dFormat { get; set; }
        public short width { get; set; }
        public short height { get; set; }
        public byte bitDepth { get; set; }
        public byte mipMapCount { get; set; }
        public byte type { get; set; }
        public byte compression { get; set; }
        /// <summary>GameCube palette pixel format for versions newer than 3.3.0.2; -1 when not present.</summary>
        public int gameCubePalettePixelFormat { get; set; } = -1;
        public Color[] palette { get; set; }
        public MipMapEntry[] mipMaps { get; set; }

        #region Gamecube
        public uint maxAniso { get; set; }
        public int biasClamp { get; set; }
        public int edgeLod { get; set; }
        public float lodBias { get; set; }
        public uint unknownGameCube { get; set; }
        public byte[] gameCubePalette { get; set; }
        #endregion

        internal int totalMipMapDataSize;
        internal byte[] textureNameRaw;
        internal byte[] alphaNameRaw;
        internal uint? originalD3DFormat;
        internal Ps2TextureString ps2TextureName;
        internal Ps2TextureString ps2AlphaName;

        public TextureNativeStruct_0001() { }
        public TextureNativeStruct_0001(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            platformType = (TexturePlatformID)binaryReader.ReadInt32();

            id = platformType == TexturePlatformID.GameCube ? Shared.Switch(binaryReader.ReadInt32()) : binaryReader.ReadInt32();

            TexturePlatformHandler handler = TexturePlatformHandler.Get(platformType);
            handler.Read(binaryReader, this);

            if (handler.ValidatesSectionSize && binaryReader.BaseStream.Position != startSectionPosition + sectionSize)
                throw new Exception(binaryReader.BaseStream.Position.ToString());
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes((int)platformType));
            listBytes.AddRange(platformType == TexturePlatformID.GameCube ? BitConverter.GetBytes(id).Reverse() : BitConverter.GetBytes(id));

            TexturePlatformHandler.Get(platformType).Write(this, listBytes);
        }
    }
}
