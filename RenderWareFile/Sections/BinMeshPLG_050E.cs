using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareFile.Sections
{
    public struct BinMesh
    {
        public int indexCount { get; set; } // number of vertex indices in this mesh
        public int materialIndex { get; set; } // material index
        public int[] vertexIndices { get; set; } // vertex indices
    }

    public class BinMeshPLG_050E : RWSection
    {
        public BinMeshHeaderFlags binMeshHeaderFlags { get; set; }
        public int numMeshes { get; set; } // number of objects/meshes (usually same number of materials)
        public int totalIndexCount { get; set; } // total number of indices

        public BinMesh[] binMeshList { get; set; }

        public bool isNativeData { get; internal set; } = false;

        public BinMeshPLG_050E() { }

        public BinMeshPLG_050E(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BinMeshPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            binMeshHeaderFlags = (BinMeshHeaderFlags)binaryReader.ReadInt32();
            numMeshes = binaryReader.ReadInt32();
            totalIndexCount = binaryReader.ReadInt32();

            binMeshList = new BinMesh[numMeshes];

            Shared.MaterialList = new List<int>();

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
                    Shared.MaterialList.Add(binMeshList[i].materialIndex);
                    isNativeData = true;
                }
            }
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BinMeshPLG;

            listBytes.AddRange(BitConverter.GetBytes((int)binMeshHeaderFlags));
            listBytes.AddRange(BitConverter.GetBytes(numMeshes));
            listBytes.AddRange(BitConverter.GetBytes(totalIndexCount));

            for (int i = 0; i < binMeshList.Count(); i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(binMeshList[i].indexCount));
                listBytes.AddRange(BitConverter.GetBytes(binMeshList[i].materialIndex));

                if (!isNativeData)
                    for (int j = 0; j < binMeshList[i].vertexIndices.Count(); j++)
                        listBytes.AddRange(BitConverter.GetBytes(binMeshList[i].vertexIndices[j]));
            }
        }
    }
}
