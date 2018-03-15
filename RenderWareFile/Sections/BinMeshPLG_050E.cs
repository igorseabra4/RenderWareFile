using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public class BinMeshPLG_050E : RWSection
    {
        public int isTristrips; // 0 = triangle lists, 1 = triangle strips
        public int numMeshes; // number of objects/meshes (usually same number of materials)
        public int totalIndexCount; // total number of indices

        BinMesh[] binMeshList;

        public BinMeshPLG_050E Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BinMeshPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            isTristrips = binaryReader.ReadInt32();
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

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BinMeshPLG;

            listBytes.AddRange(BitConverter.GetBytes(isTristrips));
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

    public struct BinMesh
    {
        public int indexCount; // number of vertex indices in this mesh
        public int materialIndex; // material index
        public int[] vertexIndices; // vertex indices
    }
}
