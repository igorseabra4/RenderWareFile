using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
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

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.AtomicSection;

            listBytes.AddRange(atomicStruct.GetBytes(fileVersion));
            listBytes.AddRange(atomicExtension.GetBytes(fileVersion));
        }
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

                if (sectionSize - supposedTotalSectionLenght == numVertices * 4) twoVcolorArrays = true;
                else if (sectionSize - supposedTotalSectionLenght == numVertices * 12) twoVcolorArrays = true;

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

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize - 8 * numTriangles;

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

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(flags));
            listBytes.AddRange(BitConverter.GetBytes(numTriangles));
            listBytes.AddRange(BitConverter.GetBytes(numVertices));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum[0]));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum[1]));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum[2]));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum[0]));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum[1]));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum[2]));
            listBytes.AddRange(BitConverter.GetBytes(unknown1));
            listBytes.AddRange(BitConverter.GetBytes(unknown2));

            for (int i = 0; i < vertexArray.Count(); i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(vertexArray[i].X));
                listBytes.AddRange(BitConverter.GetBytes(vertexArray[i].Y));
                listBytes.AddRange(BitConverter.GetBytes(vertexArray[i].Z));
            }

            if (!ReadFileMethods.isCollision)
            {
                for (int i = 0; i < colorArray.Count(); i++)
                {
                    listBytes.Add(colorArray[i].R);
                    listBytes.Add(colorArray[i].G);
                    listBytes.Add(colorArray[i].B);
                    listBytes.Add(colorArray[i].A);
                }

                for (int i = 0; i < uvArray.Count(); i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(uvArray[i].X));
                    listBytes.AddRange(BitConverter.GetBytes(uvArray[i].Y));
                }
            }

            if (ReadFileMethods.isShadow)
                for (int i = 0; i < triangleArray.Count(); i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex1));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex2));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex3));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].materialIndex));
                }
            else
                for (int i = 0; i < triangleArray.Count(); i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].materialIndex));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex1));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex2));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex3));
                }
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

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Extension;

            listBytes.AddRange(binMeshPLG.GetBytes(fileVersion));
        }
    }
}
