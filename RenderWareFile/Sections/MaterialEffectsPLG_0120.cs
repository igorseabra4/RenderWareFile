using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class MaterialEffectBumpMap : MaterialEffect
    {
        public float Intensity { get; set; }
        public bool ContainsBumpMap
        {
            get => BumpMapTexture != null;
            set
            {
                if (value)
                    BumpMapTexture = new Texture_0006();
                else
                    BumpMapTexture = null;
            }
        }
        public bool ContainsHeightMap
        {
            get => HeightMapTexture != null;
            set
            {
                if (value)
                    HeightMapTexture = new Texture_0006();
                else
                    HeightMapTexture = null;
            }
        }

        public Texture_0006 BumpMapTexture { get; set; }
        public Texture_0006 HeightMapTexture { get; set; }
        
        public MaterialEffectBumpMap Read(BinaryReader binaryReader)
        {
            binaryReader.ReadInt32(); // discard value

            Intensity = binaryReader.ReadSingle();

            bool ContainsBumpMap = binaryReader.ReadInt32() != 0;
            if (ContainsBumpMap)
            {
                Section textureSection = (Section)binaryReader.ReadInt32();
                if (textureSection != Section.Texture) throw new Exception();
                BumpMapTexture = new Texture_0006().Read(binaryReader);
            }

            bool ContainsHeightMap = binaryReader.ReadInt32() != 0;
            if (ContainsHeightMap)
            {
                Section textureSection = (Section)binaryReader.ReadInt32();
                if (textureSection != Section.Texture) throw new Exception();
                HeightMapTexture = new Texture_0006().Read(binaryReader);
            }

            return this;
        }

        public override byte[] GetBytes(int fileVersion)
        {
            List<byte> listBytes = new List<byte>();

            listBytes.AddRange(BitConverter.GetBytes((int)MaterialEffectType.BumpMap));
            listBytes.AddRange(BitConverter.GetBytes(Intensity));
            listBytes.AddRange(BitConverter.GetBytes(BumpMapTexture == null ? 0 : 1));
            if (BumpMapTexture != null)
                listBytes.AddRange(BumpMapTexture.GetBytes(fileVersion));
            listBytes.AddRange(BitConverter.GetBytes(HeightMapTexture == null ? 0 : 1));
            if (HeightMapTexture != null)
                listBytes.AddRange(HeightMapTexture.GetBytes(fileVersion));

            return listBytes.ToArray();
        }
    }

    public class MaterialEffectEnvironmentMap : MaterialEffect
    {
        public float ReflectionCoefficient { get; set; }
        public bool UseFrameBufferAlphaChannel { get; set; }
        public Texture_0006 EnvironmentMapTexture { get; set; } = new Texture_0006();

        public MaterialEffectEnvironmentMap Read(BinaryReader binaryReader)
        {
            binaryReader.ReadInt32(); // discard value

            ReflectionCoefficient = binaryReader.ReadSingle();
            UseFrameBufferAlphaChannel = binaryReader.ReadInt32() != 0;
            bool ContainsEnvironmentMap = binaryReader.ReadInt32() != 0;
            if (ContainsEnvironmentMap)
            {
                Section textureSection = (Section)binaryReader.ReadInt32();
                if (textureSection != Section.Texture) throw new Exception();
                EnvironmentMapTexture = new Texture_0006().Read(binaryReader);
            }

            return this;
        }

        public override byte[] GetBytes(int fileVersion)
        {
            List<byte> listBytes = new List<byte>();

            listBytes.AddRange(BitConverter.GetBytes((int)MaterialEffectType.EnvironmentMap));
            listBytes.AddRange(BitConverter.GetBytes(ReflectionCoefficient));
            listBytes.AddRange(BitConverter.GetBytes(UseFrameBufferAlphaChannel ? 1 : 0));

            listBytes.AddRange(BitConverter.GetBytes(EnvironmentMapTexture == null ? 0 : 1));
            if (EnvironmentMapTexture != null)
                listBytes.AddRange(EnvironmentMapTexture.GetBytes(fileVersion));

            return listBytes.ToArray();
        }
    }

    public enum BlendFactorType : int
    {
        None = 0x00,
        Zero = 0x01,
        One = 0x02,
        SourceColor = 0x03,
        InverseSourceColor = 0x04,
        SourceAlpha = 0x05,
        InverseSourceAlpha = 0x06,
        DestinationAlpha = 0x07,
        InverseDestinationAlpha = 0x08,
        DestinationColor = 0x09,
        InverseDestinationColor = 0x0A,
        SourceAlphaSaturated = 0x0B
    }

    public class MaterialEffectDualTextures : MaterialEffect
    {
        public BlendFactorType SourceBlendMode { get; set; }
        public BlendFactorType DestBlendMode { get; set; }
        public Texture_0006 Texture { get; set; } = new Texture_0006();
        
        public MaterialEffectDualTextures Read(BinaryReader binaryReader)
        {
            binaryReader.ReadInt32(); // discard value

            SourceBlendMode = (BlendFactorType)binaryReader.ReadInt32();
            DestBlendMode = (BlendFactorType)binaryReader.ReadInt32();

            bool ContainsTexture = binaryReader.ReadInt32() != 0;
            if (ContainsTexture)
            {
                Section textureSection = (Section)binaryReader.ReadInt32();
                if (textureSection != Section.Texture) throw new Exception();
                Texture = new Texture_0006().Read(binaryReader);
            }

            return this;
        }

        public override byte[] GetBytes(int fileVersion)
        {
            List<byte> listBytes = new List<byte>();

            listBytes.AddRange(BitConverter.GetBytes((int)MaterialEffectType.DualTextures));
            listBytes.AddRange(BitConverter.GetBytes((int)SourceBlendMode));
            listBytes.AddRange(BitConverter.GetBytes((int)DestBlendMode));

            listBytes.AddRange(BitConverter.GetBytes(Texture == null ? 0 : 1));
            if (Texture != null)
                listBytes.AddRange(Texture.GetBytes(fileVersion));

            return listBytes.ToArray();
        }
    }

    public class MaterialEffectUvTransformation : MaterialEffect
    {
        public MaterialEffectUvTransformation Read(BinaryReader binaryReader)
        {
            binaryReader.ReadInt32(); // discard value
            return this;
        }

        public override byte[] GetBytes(int fileVersion)
        {
            List<byte> listBytes = new List<byte>();
            listBytes.AddRange(BitConverter.GetBytes((int)MaterialEffectType.UvTransformation));
            return listBytes.ToArray();
        }
    }

    public abstract class MaterialEffect
    {
        public abstract byte[] GetBytes(int fileVersion);
    }

    public enum MaterialEffectType
    {
        NoEffect = 0,
        BumpMap = 1,
        EnvironmentMap = 2,
        BumpEnvironmentMap = 3,
        DualTextures = 4,
        UvTransformation = 5,
        DualTexturesUvTransformation = 6
    }

    public class MaterialEffectsPLG_0120 : RWSection
    {
        public MaterialEffectType value;

        public MaterialEffectType MaterialEffectType
        {
            get => value; 
            set
            {
                if (value == MaterialEffectType.BumpMap || value == MaterialEffectType.BumpEnvironmentMap)
                    materialEffect1 = new MaterialEffectBumpMap();
                else if (value == MaterialEffectType.EnvironmentMap)
                    materialEffect1 = new MaterialEffectEnvironmentMap();
                else if (value == MaterialEffectType.DualTextures || value == MaterialEffectType.DualTexturesUvTransformation)
                    materialEffect1 = new MaterialEffectDualTextures();
                else if (value == MaterialEffectType.UvTransformation)
                    materialEffect1 = new MaterialEffectUvTransformation();
                else
                    materialEffect1 = null;

                if (value == MaterialEffectType.BumpEnvironmentMap)
                    materialEffect2 = new MaterialEffectEnvironmentMap();
                else if (value == MaterialEffectType.DualTexturesUvTransformation)
                    materialEffect2 = new MaterialEffectUvTransformation();
                else
                    materialEffect2 = null;

                this.value = value;
            }
        }
        public MaterialEffect materialEffect1;
        public MaterialEffect materialEffect2;
        public bool isAtomicExtension = false;

        public MaterialEffectsPLG_0120 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.MaterialEffectsPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            value = (MaterialEffectType)binaryReader.ReadInt32();

            if (binaryReader.BaseStream.Position == startSectionPosition + sectionSize)
            {
                isAtomicExtension = true;
                return this;
            }
            if (value == MaterialEffectType.BumpMap || value == MaterialEffectType.BumpEnvironmentMap)
                materialEffect1 = new MaterialEffectBumpMap().Read(binaryReader);
            else if (value == MaterialEffectType.EnvironmentMap)
                materialEffect1 = new MaterialEffectEnvironmentMap().Read(binaryReader);
            else if (value == MaterialEffectType.DualTextures || value == MaterialEffectType.DualTexturesUvTransformation)
                materialEffect1 = new MaterialEffectDualTextures().Read(binaryReader);
            else if (value == MaterialEffectType.UvTransformation)
                materialEffect1 = new MaterialEffectUvTransformation().Read(binaryReader);
            else
                binaryReader.ReadInt32();

            if (value == MaterialEffectType.BumpEnvironmentMap)
                materialEffect2 = new MaterialEffectEnvironmentMap().Read(binaryReader);
            else if (value == MaterialEffectType.DualTexturesUvTransformation)
                materialEffect2 = new MaterialEffectUvTransformation().Read(binaryReader);
            else
                binaryReader.ReadInt32();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.MaterialEffectsPLG;

            if (isAtomicExtension)
                listBytes.AddRange(BitConverter.GetBytes((int)value));
            else
            {
                listBytes.AddRange(BitConverter.GetBytes((int)value));

                if (materialEffect1 != null)
                    listBytes.AddRange(materialEffect1.GetBytes(fileVersion));
                else
                    listBytes.AddRange(new byte[4]);

                if (materialEffect2 != null)
                    listBytes.AddRange(materialEffect2.GetBytes(fileVersion));
                else
                    listBytes.AddRange(new byte[4]);
            }
        }
    }
}
