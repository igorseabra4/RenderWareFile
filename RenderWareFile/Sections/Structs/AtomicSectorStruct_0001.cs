using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class AtomicSectorStruct_0001 : RWSection
    {
        public int matListWindowBase;
        public int numTriangles;
        public int numVertices;
        public Vertex3 boxMaximum;
        public Vertex3 boxMinimum;
        public int collSectorPresent;
        public int unused;
                
        public Vertex3[] vertexArray;
        public Color[] colorArray;
        public Vertex2[] uvArray;
        public Triangle[] triangleArray;

        public bool isNativeData = false;

        public AtomicSectorStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            matListWindowBase = binaryReader.ReadInt32();
            numTriangles = binaryReader.ReadInt32();
            numVertices = binaryReader.ReadInt32();
            boxMaximum = new Vertex3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            boxMinimum = new Vertex3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            collSectorPresent = binaryReader.ReadInt32();
            unused = binaryReader.ReadInt32();

            if (binaryReader.BaseStream.Position == startSectionPosition + sectionSize)
                if (numVertices != 0 && numTriangles != 0)
                {
                    isNativeData = true;
                    return this;
                }

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

                uvArray = new Vertex2[numVertices];
                for (int i = 0; i < numVertices; i++)
                {
                    uvArray[i].X = binaryReader.ReadSingle();
                    uvArray[i].Y = binaryReader.ReadSingle();
                }
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize - 8 * numTriangles;

            if (ReadFileMethods.isShadow)
            {
                // shadow
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

            listBytes.AddRange(BitConverter.GetBytes(matListWindowBase));
            listBytes.AddRange(BitConverter.GetBytes(numTriangles));
            listBytes.AddRange(BitConverter.GetBytes(numVertices));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum.X));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Y));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Z));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum.X));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum.Y));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum.Z));
            listBytes.AddRange(BitConverter.GetBytes(collSectorPresent));
            listBytes.AddRange(BitConverter.GetBytes(unused));
            
            for (int i = 0; i < vertexArray.Length; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(vertexArray[i].X));
                listBytes.AddRange(BitConverter.GetBytes(vertexArray[i].Y));
                listBytes.AddRange(BitConverter.GetBytes(vertexArray[i].Z));
            }

            if (!ReadFileMethods.isCollision)
            {
                for (int i = 0; i < colorArray.Length; i++)
                {
                    listBytes.Add(colorArray[i].R);
                    listBytes.Add(colorArray[i].G);
                    listBytes.Add(colorArray[i].B);
                    listBytes.Add(colorArray[i].A);
                }

                for (int i = 0; i < uvArray.Length; i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(uvArray[i].X));
                    listBytes.AddRange(BitConverter.GetBytes(uvArray[i].Y));
                }
            }

            if (ReadFileMethods.isShadow)
                for (int i = 0; i < triangleArray.Length; i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex1));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex2));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex3));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].materialIndex));
                }
            else
                for (int i = 0; i < triangleArray.Length; i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].materialIndex));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex1));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex2));
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i].vertex3));
                }
        }
    }
}
