using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public struct MorphTarget
    {
        public Vertex3 sphereCenter;
        public float radius;
        public int hasVertices;
        public int hasNormals;

        public Vertex3[] vertices;
        public Vertex3[] normals;
    }

    public class GeometryStruct_0001 : RWSection
    {
        public short geometryFlags;
        public short geometryFlags2;
        public int numTriangles;
        public int numVertices;
        public int numMorphTargets;

        public float ambient;
        public float specular;
        public float diffuse;

        public Color[] vertexColors;
        public Vertex2[] textCoords;
        public Triangle[] triangles;
        public MorphTarget[] morphTargets;

        public GeometryStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            geometryFlags = binaryReader.ReadInt16();
            geometryFlags2 = binaryReader.ReadInt16();
            numTriangles = binaryReader.ReadInt32();
            numVertices = binaryReader.ReadInt32();
            numMorphTargets = binaryReader.ReadInt32();

            ambient = binaryReader.ReadSingle();
            specular = binaryReader.ReadSingle();
            diffuse = binaryReader.ReadSingle();

            if (ambient != 1f | specular != 1f | diffuse != 1f) binaryReader.BaseStream.Position -= 3 * 4;

            if (geometryFlags2 != 0x0101)
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
                    textCoords = new Vertex2[numVertices];
                    for (int i = 0; i < numVertices; i++)
                    {
                        textCoords[i] = new Vertex2()
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

            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(geometryFlags));
            listBytes.AddRange(BitConverter.GetBytes(geometryFlags2));
            listBytes.AddRange(BitConverter.GetBytes(numTriangles));
            listBytes.AddRange(BitConverter.GetBytes(numVertices));
            listBytes.AddRange(BitConverter.GetBytes(numMorphTargets));

            if (ambient == 1f | specular == 1f | diffuse == 1f)
            {
                listBytes.AddRange(BitConverter.GetBytes(ambient));
                listBytes.AddRange(BitConverter.GetBytes(specular));
                listBytes.AddRange(BitConverter.GetBytes(diffuse));
            }

            if (geometryFlags2 != 0x0101)
            {
                if ((geometryFlags & (int)GeometryFlags.hasVertexColors) != 0)
                {
                    for (int i = 0; i < numVertices; i++)
                    {
                        listBytes.Add(vertexColors[i].R);
                        listBytes.Add(vertexColors[i].G);
                        listBytes.Add(vertexColors[i].B);
                        listBytes.Add(vertexColors[i].A);
                    }
                }

                if ((geometryFlags & (int)GeometryFlags.hasTextCoords) != 0)
                {
                    for (int i = 0; i < numVertices; i++)
                    {
                        listBytes.AddRange(BitConverter.GetBytes(textCoords[i].X));
                        listBytes.AddRange(BitConverter.GetBytes(textCoords[i].Y));
                    }
                }

                for (int i = 0; i < numTriangles; i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(triangles[i].vertex2));
                    listBytes.AddRange(BitConverter.GetBytes(triangles[i].vertex1));
                    listBytes.AddRange(BitConverter.GetBytes(triangles[i].materialIndex));
                    listBytes.AddRange(BitConverter.GetBytes(triangles[i].vertex3));
                }
            }

            for (int i = 0; i < numMorphTargets; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].sphereCenter.X));
                listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].sphereCenter.Y));
                listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].sphereCenter.Z));
                listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].radius));
                listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].hasVertices));
                listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].hasNormals));

                if (morphTargets[i].hasVertices != 0)
                {
                    for (int j = 0; j < numVertices; j++)
                    {
                        listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].vertices[j].X));
                        listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].vertices[j].Y));
                        listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].vertices[j].Z));
                    }
                }

                if (morphTargets[i].hasNormals != 0)
                {
                    for (int j = 0; j < numVertices; j++)
                    {
                        listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].normals[j].X));
                        listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].normals[j].Y));
                        listBytes.AddRange(BitConverter.GetBytes(morphTargets[i].normals[j].Z));
                    }
                }
            }
        }
    }
}
