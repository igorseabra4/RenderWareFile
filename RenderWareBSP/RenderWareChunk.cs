using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareChunk
{	
    public enum Section
    {
        None = 0x0,
        Struct = 0x1,
        String = 0x2,
        Extension = 0x3,
        Texture = 0x6,
        Material = 0x7,
        MaterialList = 0x8,
        AtomicSection = 0x9,
        PlaneSection = 0xA,
        World = 0xB,
        FrameList = 0xE,
        Geometry = 0xF,
        Clump = 0x10,
        Atomic = 0x14,
        GeometryList = 0x1A,
        ChunkGroupStart = 0x29,
        ChunkGroupEnd = 0x2A,
        BinMeshPLG = 0x50E
    }

    public abstract class RWSection
    {
        public Section sectionIdentifier;
        public int sectionSize;
        public int renderWareVersion;
        
        public void Write(BinaryWriter binaryWriter)
        {
            SetandGetSize();

            binaryWriter.Write((int)sectionIdentifier);
            binaryWriter.Write(sectionSize);
            binaryWriter.Write(renderWareVersion);

            WriteSection(binaryWriter);
        }

        public abstract void WriteSection(BinaryWriter binaryWriter);
        public abstract int SetandGetSize();
    }
    
    public class String_0002 : RWSection
    {
        public string stringString;
        
        public String_0002 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.String;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            List<char> charList = new List<char>(sectionSize);
            char c = binaryReader.ReadChar();
            while (c != 0)
            {
                charList.Add(c);
                c = binaryReader.ReadChar();
            }

            stringString = new string(charList.ToArray());

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            long startSectionPosition = binaryWriter.BaseStream.Position;

            binaryWriter.Write(stringString.ToCharArray());
            binaryWriter.Write((byte)0);
            
            binaryWriter.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override int SetandGetSize()
        {
            sectionSize = stringString.Length;
            if (stringString.Length % 4 == 0) sectionSize += 4;
            if (stringString.Length % 4 == 1) sectionSize += 3;
            if (stringString.Length % 4 == 2) sectionSize += 2;
            if (stringString.Length % 4 == 3) sectionSize += 1;

            return sectionSize;
        }
    }

    public class Extension_0003 : RWSection
    {
        public Extension_0003 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Extension;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }
        
        public override void WriteSection(BinaryWriter binaryWriter) { }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public enum FilterMode : byte
    {
        FILTERNAFILTERMODE = 0,
        FILTERNEAREST = 1,
        FILTERLINEAR = 2,
        FILTERMIPNEAREST = 3,
        FILTERMIPLINEAR = 4,
        FILTERLINEARMIPNEAREST = 5,
        FILTERLINEARMIPLINEAR = 6
    }

    public enum AddressMode : byte
    {
        TEXTUREADDRESSNATEXTUREADDRESS = 0,
        TEXTUREADDRESSWRAP = 1,
        TEXTUREADDRESSMIRROR = 2,
        TEXTUREADDRESSCLAMP = 3,
        TEXTUREADDRESSBORDER = 4
    }

    public class TextureStruct_0001 : RWSection
    {
        public FilterMode filterMode;
        public AddressMode addressModeU;
        public AddressMode addressModeV;
        public ushort useMipLevels;
        
        public TextureStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            filterMode = (FilterMode)binaryReader.ReadByte();

            byte addressMode = binaryReader.ReadByte();
            addressModeU = (AddressMode)(addressMode & 0xF0);
            addressModeV = (AddressMode)(addressMode & 0x0F);

            useMipLevels = binaryReader.ReadUInt16();

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write((byte)filterMode);
            binaryWriter.Write((byte)((byte)addressModeV + 16 * (byte)addressModeU));
            binaryWriter.Write(useMipLevels);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class Texture_0006 : RWSection
    {
        public TextureStruct_0001 textureStruct;
        public String_0002 diffuseTextureName;
        public String_0002 alphaTextureName;
        public Extension_0003 textureExtension;
        
        public Texture_0006 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Texture;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section textureStructSection = (Section)binaryReader.ReadInt32();
            if (textureStructSection != Section.Struct) throw new Exception();
            textureStruct = new TextureStruct_0001().Read(binaryReader);

            Section diffuseTextureNameSection = (Section)binaryReader.ReadInt32();
            if (diffuseTextureNameSection != Section.String) throw new Exception();
            diffuseTextureName = new String_0002().Read(binaryReader);

            Section alphaTextureNameSection = (Section)binaryReader.ReadInt32();
            if (alphaTextureNameSection != Section.String) throw new Exception();
            alphaTextureName = new String_0002().Read(binaryReader);
            
            Section textureExtensionSection = (Section)binaryReader.ReadInt32();
            if (textureExtensionSection != Section.Extension) throw new Exception();
            textureExtension = new Extension_0003().Read(binaryReader);

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            textureStruct.Write(binaryWriter);
            diffuseTextureName.Write(binaryWriter);
            alphaTextureName.Write(binaryWriter);
            textureExtension.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
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

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(unusedFlags);
            binaryWriter.Write((int)color);
            binaryWriter.Write(unusedInt2);
            binaryWriter.Write(isTextured);
            binaryWriter.Write(ambient);
            binaryWriter.Write(specular);
            binaryWriter.Write(diffuse);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

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

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            materialStruct.Write(binaryWriter);
            texture.Write(binaryWriter);
            materialExtension.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }
    
    public class MaterialListStruct_0001 : RWSection
    {
        public int materialCount;
        
        public MaterialListStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            materialCount = binaryReader.ReadInt32();
            for (int i = 0; i < materialCount; i++)
            {
                binaryReader.ReadInt32();
            }

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(materialCount);
            for (int i = 0; i < materialCount; i++)
            {
                binaryWriter.Write(-1);
            }
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class MaterialList_0008 : RWSection
    {
        public MaterialListStruct_0001 materialListStruct;
        public Material_0007[] materialList;
        
        public MaterialList_0008 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section materialListStructSection = (Section)binaryReader.ReadInt32();
            if (materialListStructSection != Section.Struct) throw new Exception();
            materialListStruct = new MaterialListStruct_0001().Read(binaryReader);

            materialList = new Material_0007[materialListStruct.materialCount];

            for (int i = 0; i < materialListStruct.materialCount; i++)
            {
                Section materialSection = (Section)binaryReader.ReadInt32();
                if (materialSection != Section.Material) throw new Exception();
                materialList[i] = new Material_0007().Read(binaryReader);
            }

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            materialListStruct.Write(binaryWriter);
            for (int i = 0; i < materialList.Count(); i++)
            {
                materialList[i].Write(binaryWriter);
            }
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }
    
    public struct TextCoord
    {
        public float X;
        public float Y;
    }

    public struct Vertex3
    {
        public float X;
        public float Y;
        public float Z;
    }

    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static explicit operator int(Color v)
        {
            return BitConverter.ToInt32(new byte[] { v.R, v.G, v.B, v.A }, 0);
        }
    }

    public struct Triangle
    {
        public ushort materialIndex;
        public ushort vertex1;
        public ushort vertex2;
        public ushort vertex3;
    }

    public class AtomicStruct_0001 : RWSection
    {
        public int flags;
        public int numTriangles;
        public int numVertices;
        public float[] boxMaximum = new float[3];
        public float[] boxMinimum = new float[3];
        public int unknown1;
        public int unknown2;

        public Vertex3[] vertexArray;
        public Color[] colorArray;
        public TextCoord[] uvArray;
        public Triangle[] triangleArray;
        
        public AtomicStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;
            
            flags = binaryReader.ReadInt32();
            numTriangles = binaryReader.ReadInt32();
            numVertices = binaryReader.ReadInt32();
            boxMaximum = new float[3];
            boxMaximum[0] = binaryReader.ReadSingle();
            boxMaximum[1] = binaryReader.ReadSingle();
            boxMaximum[2] = binaryReader.ReadSingle();
            boxMinimum = new float[3];
            boxMinimum[0] = binaryReader.ReadSingle();
            boxMinimum[1] = binaryReader.ReadSingle();
            boxMinimum[2] = binaryReader.ReadSingle();
            unknown1 = binaryReader.ReadInt32();
            unknown2 = binaryReader.ReadInt32();
            
            binaryReader.BaseStream.Position = startSectionPosition + 11 * 4;

            vertexArray = new Vertex3[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                vertexArray[i].X = binaryReader.ReadSingle();
                vertexArray[i].Y = binaryReader.ReadSingle();
                vertexArray[i].Z = binaryReader.ReadSingle();
            }

            binaryReader.BaseStream.Position = startSectionPosition + 11 * 4 + 12 * numVertices;
            
            if (!ReadFileMethods.isCollision)
            {
                int supposedTotalSectionLenght = (11 * 4) + (12 + 4 + 8) * numVertices + 8 * numTriangles;
                bool twoVcolorArrays = false;
                bool twoUVArrays = false;

                if (sectionSize - supposedTotalSectionLenght == numVertices * 4) twoVcolorArrays = true;
                else if (sectionSize - supposedTotalSectionLenght == numVertices * 8) twoUVArrays = true;
                else if (sectionSize - supposedTotalSectionLenght == numVertices * 12)
                {
                    twoVcolorArrays = true;
                    twoUVArrays = true;
                }

                if (twoVcolorArrays) binaryReader.BaseStream.Position += 4 * numVertices;

                colorArray = new Color[numVertices];
                for (int i = 0; i < numVertices; i++)
                {
                    colorArray[i].R = binaryReader.ReadByte();
                    colorArray[i].G = binaryReader.ReadByte();
                    colorArray[i].B = binaryReader.ReadByte();
                    colorArray[i].A = binaryReader.ReadByte();
                }

                if (twoVcolorArrays)
                    binaryReader.BaseStream.Position = startSectionPosition + 11 * 4 + 20 * numVertices;
                else
                    binaryReader.BaseStream.Position = startSectionPosition + 11 * 4 + 16 * numVertices;

                uvArray = new TextCoord[numVertices];
                for (int i = 0; i < numVertices; i++)
                {
                    uvArray[i].X = binaryReader.ReadSingle();
                    uvArray[i].Y = binaryReader.ReadSingle();
                }
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize - 2 * 4 * numTriangles;

            if (ReadFileMethods.isShadow)
            {
                // shadow xbox
                triangleArray = new Triangle[numTriangles];
                for (int i = 0; i < numTriangles; i++)
                {
                    triangleArray[i].vertex1 = binaryReader.ReadUInt16();
                    triangleArray[i].vertex2 = binaryReader.ReadUInt16();
                    triangleArray[i].vertex3 = binaryReader.ReadUInt16();
                    triangleArray[i].materialIndex = binaryReader.ReadUInt16();
                }
            }
            else
            {
                // heroes
                triangleArray = new Triangle[numTriangles];
                for (int i = 0; i < numTriangles; i++)
                {
                    triangleArray[i].materialIndex = binaryReader.ReadUInt16();
                    triangleArray[i].vertex1 = binaryReader.ReadUInt16();
                    triangleArray[i].vertex2 = binaryReader.ReadUInt16();
                    triangleArray[i].vertex3 = binaryReader.ReadUInt16();
                }
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(flags);
            binaryWriter.Write(numTriangles);
            binaryWriter.Write(numVertices);
            binaryWriter.Write(boxMaximum[0]);
            binaryWriter.Write(boxMaximum[1]);
            binaryWriter.Write(boxMaximum[2]);
            binaryWriter.Write(boxMinimum[0]);
            binaryWriter.Write(boxMinimum[1]);
            binaryWriter.Write(boxMinimum[2]);
            binaryWriter.Write(unknown1);
            binaryWriter.Write(unknown2);

            for (int i = 0; i < vertexArray.Count(); i++)
            {
                binaryWriter.Write(vertexArray[i].X);
                binaryWriter.Write(vertexArray[i].Y);
                binaryWriter.Write(vertexArray[i].Z);
            }

            for (int i = 0; i < colorArray.Count(); i++)
            {
                binaryWriter.Write(colorArray[i].R);
                binaryWriter.Write(colorArray[i].G);
                binaryWriter.Write(colorArray[i].B);
                binaryWriter.Write(colorArray[i].A);
            }

            for (int i = 0; i < uvArray.Count(); i++)
            {
                binaryWriter.Write(vertexArray[i].X);
                binaryWriter.Write(vertexArray[i].Y);
            }

            for (int i = 0; i < triangleArray.Count(); i++)
            {
                binaryWriter.Write(triangleArray[i].materialIndex);
                binaryWriter.Write(triangleArray[i].vertex1);
                binaryWriter.Write(triangleArray[i].vertex2);
                binaryWriter.Write(triangleArray[i].vertex3);
            }
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public struct BinMesh
    {
        public int indexCount; // number of vertex indices in this mesh
        public int materialIndex; // material index
        public int[] vertexIndices; // vertex indices
    }

    public class BinMeshPLG_050E : RWSection
    {
        public int flags; // 0 = triangle lists, 1 = triangle strips
        public int numMeshes; // number of objects/meshes (usually same number of materials)
        public int totalIndexCount; // total number of indices

        BinMesh[] binMeshList;
        
        public BinMeshPLG_050E Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BinMeshPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            flags = binaryReader.ReadInt32();
            numMeshes = binaryReader.ReadInt32();
            totalIndexCount = binaryReader.ReadInt32();

            binMeshList = new BinMesh[numMeshes];

            for (int i = 0; i < numMeshes; i++)
            {
                binMeshList[i] = new BinMesh
                {
                    indexCount = binaryReader.ReadInt32(),
                    materialIndex = binaryReader.ReadInt32()
                };
                binMeshList[i].vertexIndices = new int[binMeshList[i].indexCount];

                for (int j = 0; j < binMeshList[i].vertexIndices.Count(); j++)
                    binMeshList[i].vertexIndices[j] = binaryReader.ReadInt32();                
            }

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(flags);
            binaryWriter.Write(numMeshes);
            binaryWriter.Write(totalIndexCount);

            for (int i = 0; i < binMeshList.Count(); i++)
            {
                binaryWriter.Write(binMeshList[i].indexCount);
                binaryWriter.Write(binMeshList[i].materialIndex);

                for (int j = 0; j < binMeshList[i].vertexIndices.Count(); j++)
                    binaryWriter.Write(binMeshList[i].vertexIndices[j]);
            }
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class AtomicExtension_0003 : RWSection
    {
        public BinMeshPLG_050E binMeshPLG;
        
        public AtomicExtension_0003 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Extension;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section binMeshPLGSection = (Section)binaryReader.ReadInt32();
            if (binMeshPLGSection != Section.BinMeshPLG) throw new Exception();
            binMeshPLG = new BinMeshPLG_050E().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }
        
        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binMeshPLG.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class AtomicSection_0009 : RWSection
    {
        public AtomicStruct_0001 atomicStruct;
        public AtomicExtension_0003 atomicExtension;
        
        public AtomicSection_0009 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.AtomicSection;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section atomicStructSection = (Section)binaryReader.ReadInt32();
            if (atomicStructSection != Section.Struct) throw new Exception();
            atomicStruct = new AtomicStruct_0001().Read(binaryReader);

            Section atomicExtensionSection = (Section)binaryReader.ReadInt32();
            if (atomicExtensionSection != Section.Extension) throw new Exception();
            atomicExtension = new AtomicExtension_0003().Read(binaryReader);
            
            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            atomicStruct.Write(binaryWriter);
            atomicExtension.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class PlaneStruct_0001 : RWSection
    {
        public int type;
        public float value;
        public int leftIsAtomic;
        public int rightIsAtomic;
        public float leftValue;
        public float rightValue;
        
        public PlaneStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            type = binaryReader.ReadInt32();
            value = binaryReader.ReadSingle();
            leftIsAtomic = binaryReader.ReadInt32();
            rightIsAtomic = binaryReader.ReadInt32();
            leftValue = binaryReader.ReadSingle();
            rightValue = binaryReader.ReadSingle();

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(type);
            binaryWriter.Write(value);
            binaryWriter.Write(leftIsAtomic);
            binaryWriter.Write(rightIsAtomic);
            binaryWriter.Write(leftValue);
            binaryWriter.Write(rightValue);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class PlaneSection_000A : RWSection
    {
        public PlaneStruct_0001 planeStruct;
        public RWSection leftSection;
        public RWSection rightSection;
        
        public PlaneSection_000A Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.PlaneSection;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section planeStructSection = (Section)binaryReader.ReadInt32();
            if (planeStructSection != Section.Struct) throw new Exception();
            planeStruct = new PlaneStruct_0001().Read(binaryReader);

            Section leftSectionSection = (Section)binaryReader.ReadInt32();
            if (leftSectionSection == Section.AtomicSection & planeStruct.leftIsAtomic == 1)
            {
                leftSection = new AtomicSection_0009().Read(binaryReader);
            }
            else if (leftSectionSection == Section.PlaneSection & planeStruct.leftIsAtomic == 0)
            {
                leftSection = new PlaneSection_000A().Read(binaryReader);
            }
            else throw new Exception();

            Section rightSectionSection = (Section)binaryReader.ReadInt32();
            if (rightSectionSection == Section.AtomicSection & planeStruct.rightIsAtomic == 1)
            {
                rightSection = new AtomicSection_0009().Read(binaryReader);
            }
            else if (rightSectionSection == Section.PlaneSection & planeStruct.rightIsAtomic == 0)
            {
                rightSection = new PlaneSection_000A().Read(binaryReader);
            }
            else throw new Exception();
            
            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            planeStruct.Write(binaryWriter);
            leftSection.Write(binaryWriter);
            rightSection.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }
    
    public class WorldStruct_0001 : RWSection
    {
        public int structVersion;
        public int unknown2;
        public int unknown3;
        public int unknown4;
        public int numTriangles;
        public int numVertices;
        public int numPlaneSections;
        public int numAtomicSections;
        public int unknown5;
        public int unknown6;
        public float[] boxMaximum = new float[3];
        public float[] boxMinimum = new float[3];
        
        public WorldStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            if (sectionSize == 0x40)
            {
                structVersion = binaryReader.ReadInt32();
                unknown2 = binaryReader.ReadInt32();
                unknown3 = binaryReader.ReadInt32();
                unknown4 = binaryReader.ReadInt32();
                numTriangles = binaryReader.ReadInt32();
                numVertices = binaryReader.ReadInt32();
                numPlaneSections = binaryReader.ReadInt32();
                numAtomicSections = binaryReader.ReadInt32();
                unknown5 = binaryReader.ReadInt32();
                unknown6 = binaryReader.ReadInt32();
                boxMaximum = new float[3];
                boxMaximum[0] = binaryReader.ReadSingle();
                boxMaximum[1] = binaryReader.ReadSingle();
                boxMaximum[2] = binaryReader.ReadSingle();
                boxMinimum = new float[3];
                boxMinimum[0] = binaryReader.ReadSingle();
                boxMinimum[1] = binaryReader.ReadSingle();
                boxMinimum[2] = binaryReader.ReadSingle();
            }
            else if (sectionSize == 0x34)
            {
                structVersion = binaryReader.ReadInt32();
                unknown2 = binaryReader.ReadInt32();
                unknown3 = binaryReader.ReadInt32();
                unknown4 = binaryReader.ReadInt32();
                boxMaximum = new float[3];
                boxMaximum[0] = binaryReader.ReadSingle();
                boxMaximum[1] = binaryReader.ReadSingle();
                boxMaximum[2] = binaryReader.ReadSingle();
                boxMinimum = boxMaximum;
                numTriangles = binaryReader.ReadInt32();
                numVertices = binaryReader.ReadInt32();
                numPlaneSections = binaryReader.ReadInt32();
                numAtomicSections = binaryReader.ReadInt32();
                unknown5 = binaryReader.ReadInt32();
                unknown6 = binaryReader.ReadInt32();
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(structVersion);
            binaryWriter.Write(unknown2);
            binaryWriter.Write(unknown3);
            binaryWriter.Write(unknown4);
            binaryWriter.Write(numTriangles);
            binaryWriter.Write(numVertices);
            binaryWriter.Write(numPlaneSections);
            binaryWriter.Write(numAtomicSections);
            binaryWriter.Write(unknown5);
            binaryWriter.Write(unknown6);
            binaryWriter.Write(boxMaximum[0]);
            binaryWriter.Write(boxMaximum[1]);
            binaryWriter.Write(boxMaximum[2]);
            binaryWriter.Write(boxMinimum[0]);
            binaryWriter.Write(boxMinimum[1]);
            binaryWriter.Write(boxMinimum[2]);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class World_000B : RWSection
    {
        public WorldStruct_0001 worldStruct;
        public MaterialList_0008 materialList;
        public RWSection firstWorldChunk;
        public Extension_0003 worldExtension;

        public World_000B Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.World;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section worldStructSection = (Section)binaryReader.ReadInt32();
            if (worldStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            worldStruct = new WorldStruct_0001().Read(binaryReader);

            Section materialListSection = (Section)binaryReader.ReadInt32();
            if (materialListSection != Section.MaterialList) throw new Exception();
            materialList = new MaterialList_0008().Read(binaryReader);

            Section firstWorldChunkSection = (Section)binaryReader.ReadInt32();
            if (firstWorldChunkSection == Section.AtomicSection)
            {
                firstWorldChunk = new AtomicSection_0009().Read(binaryReader);
            }
            else if (firstWorldChunkSection == Section.PlaneSection)
            {
                firstWorldChunk = new PlaneSection_000A().Read(binaryReader);
            }
            else throw new Exception();

            Section worldExtensionSection = (Section)binaryReader.ReadInt32();
            if (worldExtensionSection == Section.Extension)
                worldExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            worldStruct.Write(binaryWriter);
            materialList.Write(binaryWriter);
            firstWorldChunk.Write(binaryWriter);
            worldExtension.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class FrameList_000E : RWSection
    {
        // Not yet implemented

        public FrameList_000E Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.FrameList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            // Not yet implemented

            binaryWriter.BaseStream.Position += sectionSize;
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }
    
    public struct MorphTarget
    {
        public Vertex3 sphereCenter;
        public float radius;
        public int hasVertices;
        public int hasNormals;

        public Vertex3[] vertices;
        public Vertex3[] normals;
    }

    public enum GeometryFlags
    {
        isTristrip = 0x00000001,
        hasVertexPositions = 0x00000002,
        hasTextCoords = 0x00000004,
        hasVertexColors = 0x00000008,
        hasNormals = 0x00000010,
        hasLights = 0x00000020,
        modeulateMaterialColor = 0x00000040,
        hasTextCoords2 = 0x00000080,
        isNativeGeometry = 0x01000000
    }
    
    public class GeometryStruct_0001 : RWSection
    {
        UInt32 libraryIDUnpackVersion(UInt32 libid)
        {
            if ((libid & 0xFFFF0000) != 0)
                return (libid >> 14 & 0x3FF00) + 0x30000 |
                       (libid >> 16 & 0x3F);
            return libid << 8;
        }

        public int geometryFlags;
        public int numTriangles;
        public int numVertices;
        public int numMorphTargets;

        public float ambient;
        public float specular;
        public float diffuse;

        public Color[] vertexColors;
        public TextCoord[] textCoords;
        public Triangle[] triangles;
        public MorphTarget[] morphTargets;

        public GeometryStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            geometryFlags = binaryReader.ReadInt32();
            numTriangles = binaryReader.ReadInt32();
            numVertices = binaryReader.ReadInt32();
            numMorphTargets = binaryReader.ReadInt32();

            ambient = binaryReader.ReadSingle();
            specular = binaryReader.ReadSingle();
            diffuse = binaryReader.ReadSingle();

            if (ambient != 1f | specular != 1f | diffuse != 1f) binaryReader.BaseStream.Position -= 3 * 4;

            if ((geometryFlags & (int)GeometryFlags.isNativeGeometry) == 0)
            {
                if ((geometryFlags & (int)GeometryFlags.hasVertexColors) != 0)
                {
                    vertexColors = new Color[numVertices];
                    for (int i = 0; i < numVertices; i++)
                    {
                        vertexColors[i] = new Color()
                        {
                            R = binaryReader.ReadByte(),
                            G = binaryReader.ReadByte(),
                            B = binaryReader.ReadByte(),
                            A = binaryReader.ReadByte()
                        };
                    }
                }

                if ((geometryFlags & (int)GeometryFlags.hasTextCoords) != 0)
                {
                    textCoords = new TextCoord[numVertices];
                    for (int i = 0; i < numVertices; i++)
                    {
                        textCoords[i] = new TextCoord()
                        {
                            X = binaryReader.ReadSingle(),
                            Y = binaryReader.ReadSingle()
                        };
                    }
                }

                triangles = new Triangle[numTriangles];
                for (int i = 0; i < numTriangles; i++)
                {
                    triangles[i] = new Triangle()
                    {
                        vertex2 = binaryReader.ReadUInt16(),
                        vertex1 = binaryReader.ReadUInt16(),
                        materialIndex = binaryReader.ReadUInt16(),
                        vertex3 = binaryReader.ReadUInt16()
                    };
                }
            }

            morphTargets = new MorphTarget[numMorphTargets];
            for (int i = 0; i < numMorphTargets; i++)
            {
                MorphTarget m = new MorphTarget();

                m.sphereCenter.X = binaryReader.ReadSingle();
                m.sphereCenter.Y = binaryReader.ReadSingle();
                m.sphereCenter.Z = binaryReader.ReadSingle();
                m.radius = binaryReader.ReadSingle();
                m.hasVertices = binaryReader.ReadInt32();
                m.hasNormals = binaryReader.ReadInt32();

                if (m.hasVertices != 0)
                {
                    m.vertices = new Vertex3[numVertices];
                    for (int j = 0; j < numVertices; j++)
                    {
                        m.vertices[j] = new Vertex3()
                        {
                            X = binaryReader.ReadSingle(),
                            Y = binaryReader.ReadSingle(),
                            Z = binaryReader.ReadSingle()
                        };
                    }
                }

                if (m.hasNormals != 0)
                {
                    m.normals = new Vertex3[numVertices];
                    for (int j = 0; j < numVertices; j++)
                    {
                        m.normals[j] = new Vertex3()
                        {
                            X = binaryReader.ReadSingle(),
                            Y = binaryReader.ReadSingle(),
                            Z = binaryReader.ReadSingle()
                        };
                    }
                }

                morphTargets[i] = m;
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            //not done yet

            binaryWriter.BaseStream.Position += sectionSize;
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class Geometry_000F : RWSection
    {
        public GeometryStruct_0001 geometryStruct;
        public MaterialList_0008 materialList;
        public AtomicExtension_0003 geometryExtension;

        public Geometry_000F Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Geometry;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section geometryStructSection = (Section)binaryReader.ReadInt32();
            if (geometryStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryStruct = new GeometryStruct_0001().Read(binaryReader);

            Section materialListSection = (Section)binaryReader.ReadInt32();
            if (materialListSection != Section.MaterialList) throw new Exception(binaryReader.BaseStream.Position.ToString());
            materialList = new MaterialList_0008().Read(binaryReader);

            Section geometryExtensionSection = (Section)binaryReader.ReadInt32();
            if (geometryExtensionSection != Section.Extension) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryExtension = new AtomicExtension_0003().Read(binaryReader);
            
            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            geometryStruct.Write(binaryWriter);
            materialList.Write(binaryWriter);
            geometryExtension.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }
    
    public class ClumpStruct_0001 : RWSection
    {
        public int atomicCount;
        public int lightCount;
        public int cameraCount;

        public ClumpStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            atomicCount = binaryReader.ReadInt32();
            lightCount = binaryReader.ReadInt32();
            cameraCount = binaryReader.ReadInt32();

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(atomicCount);
            binaryWriter.Write(lightCount);
            binaryWriter.Write(cameraCount);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class Clump_0010 : RWSection
    {
        public ClumpStruct_0001 clumpStruct;
        public FrameList_000E frameList;
        public GeometryList_001A geometryList;
        public List<Atomic_0014> atomicList;
        public Extension_0003 clumpExtension;

        public Clump_0010 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Clump;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section clumpStructSection = (Section)binaryReader.ReadInt32();
            if (clumpStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            clumpStruct = new ClumpStruct_0001().Read(binaryReader);

            Section frameListSection = (Section)binaryReader.ReadInt32();
            if (frameListSection != Section.FrameList) throw new Exception(binaryReader.BaseStream.Position.ToString());
            frameList = new FrameList_000E().Read(binaryReader);

            Section geometryListSection = (Section)binaryReader.ReadInt32();
            if (geometryListSection != Section.GeometryList) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryList = new GeometryList_001A().Read(binaryReader);

            atomicList = new List<Atomic_0014>();
            for (int i = 0; i < clumpStruct.atomicCount; i++)
            {
                Section atomicListSection = (Section)binaryReader.ReadInt32();
                if (atomicListSection != Section.Atomic) throw new Exception(binaryReader.BaseStream.Position.ToString());
                    atomicList.Add(new Atomic_0014().Read(binaryReader));                
            }

            Section clumpExtensionSection = (Section)binaryReader.ReadInt32();
            if (clumpExtensionSection == Section.Extension)
                clumpExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            clumpStruct.Write(binaryWriter);
            frameList.Write(binaryWriter);
            geometryList.Write(binaryWriter);
            for (int i = 0; i < atomicList.Count; i++) atomicList[i].Write(binaryWriter);
            clumpExtension.Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class Atomic_0014 : RWSection
    {
        // Not yet implemented

        public Atomic_0014 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            // Not yet implemented

            binaryWriter.BaseStream.Position += sectionSize;
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class GeometryListStruct_0001 : RWSection
    {
        public int numberOfGeometries;

        public GeometryListStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            numberOfGeometries = binaryReader.ReadInt32();

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(numberOfGeometries);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class GeometryList_001A : RWSection
    {
        public GeometryListStruct_0001 geometryListStruct;
        public List<Geometry_000F> geometryList = new List<Geometry_000F>();

        public GeometryList_001A Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.GeometryList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section geometryListStructSection = (Section)binaryReader.ReadInt32();
            if (geometryListStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryListStruct = new GeometryListStruct_0001().Read(binaryReader);

            geometryList.Clear();
            for (int i = 0; i < geometryListStruct.numberOfGeometries; i++)
            {
                Section geometryListSection = (Section)binaryReader.ReadInt32();
                if (geometryListSection != Section.Geometry) throw new Exception(binaryReader.BaseStream.Position.ToString());
                geometryList.Add(new Geometry_000F().Read(binaryReader));
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            geometryListStruct.Write(binaryWriter);
            for (int i = 0; i < geometryList.Count; i++) geometryList[i].Write(binaryWriter);
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class ChunkGroupStart_0029 : RWSection
    {
        // Not yet implemented

        public ChunkGroupStart_0029 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            // Not yet implemented

            binaryWriter.BaseStream.Position += sectionSize;
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }

    public class ChunkGroupEnd_002A : RWSection
    {
        // Not yet implemented

        public ChunkGroupEnd_002A Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Atomic;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }

        public override void WriteSection(BinaryWriter binaryWriter)
        {
            // Not yet implemented

            binaryWriter.BaseStream.Position += sectionSize;
        }

        public override int SetandGetSize()
        {
            throw new NotImplementedException();
        }
    }
}