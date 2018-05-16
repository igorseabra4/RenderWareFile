using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareFile.Sections
{
    public enum BinMeshHeaderFlags : int
    {
        TriangleList = 0x00000000,
        TriangleStrip = 0x00000001,
        TriangleFan = 0x00000002,
        LineList = 0x00000004,
        PolyLine = 0x00000008,
        PointList = 0x00000010,
        PrimitiveMask = 0x000000FF,
        Unindexed = 0x00000100
    }

    public struct BinMesh
    {
        public int indexCount; // number of vertex indices in this mesh
        public int materialIndex; // material index
        public int[] vertexIndices; // vertex indices
    }

    public class BinMeshPLG_050E : RWSection
    {
        public BinMeshHeaderFlags binMeshHeaderFlags; 
        public int numMeshes; // number of objects/meshes (usually same number of materials)
        public int totalIndexCount; // total number of indices

        public BinMesh[] binMeshList;

        public bool isNativeData = false;

        public BinMeshPLG_050E Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BinMeshPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            binMeshHeaderFlags = (BinMeshHeaderFlags)binaryReader.ReadInt32();
            numMeshes = binaryReader.ReadInt32();
            totalIndexCount = binaryReader.ReadInt32();

            binMeshList = new BinMesh[numMeshes];

            General.MaterialList = new List<int>();

            for (int i = 0; i < numMeshes; i++)
            {
                binMeshList[i] = new BinMesh
                {
                    indexCount = binaryReader.ReadInt32(),
                    materialIndex = binaryReader.ReadInt32()
                };

                if (sectionSize != 12 + 8 * numMeshes)
                {
                    binMeshList[i].vertexIndices = new int[binMeshList[i].indexCount];

                    for (int j = 0; j < binMeshList[i].vertexIndices.Count(); j++)
                        binMeshList[i].vertexIndices[j] = binaryReader.ReadInt32();
                }
                else
                {
                    General.MaterialList.Add(binMeshList[i].materialIndex);
                    isNativeData = true;
                }
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BinMeshPLG;

            listBytes.AddRange(BitConverter.GetBytes((int)binMeshHeaderFlags));
            listBytes.AddRange(BitConverter.GetBytes(numMeshes));
            listBytes.AddRange(BitConverter.GetBytes(totalIndexCount));

            for (int i = 0; i < binMeshList.Count(); i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(binMeshList[i].indexCount));
                listBytes.AddRange(BitConverter.GetBytes(binMeshList[i].materialIndex));

                for (int j = 0; j < binMeshList[i].vertexIndices.Count(); j++)
                    listBytes.AddRange(BitConverter.GetBytes(binMeshList[i].vertexIndices[j]));
            }
        }
    }
}
