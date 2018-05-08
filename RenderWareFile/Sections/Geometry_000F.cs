using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public enum GeometryFlags
    {
        isTristrip = 0x0001,
        hasVertexPositions = 0x0002,
        hasTextCoords = 0x0004,
        hasVertexColors = 0x0008,
        hasNormals = 0x0010,
        hasLights = 0x0020,
        modeulateMaterialColor = 0x00040,
        hasTextCoords2 = 0x0080
    }

    public class Geometry_000F : RWSection
    {
        public GeometryStruct_0001 geometryStruct;
        public MaterialList_0008 materialList;
        public Extension_0003 geometryExtension;

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
            geometryExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }
        
        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Geometry;

            listBytes.AddRange(geometryStruct.GetBytes(fileVersion));
            listBytes.AddRange(materialList.GetBytes(fileVersion));
            listBytes.AddRange(geometryExtension.GetBytes(fileVersion));
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
        public TextCoord[] textCoords;
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
