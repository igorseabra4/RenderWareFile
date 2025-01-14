using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public enum SectorType : ushort
    {
        rpCOLLBSP_LEAF_NODE = 0x01,
        rpCOLLBSP_BRANCH_NODE = 0x02,
    }

    public struct RpCollBSPBranchNode
    {
        public SectorAxis type;
        public SectorType leftType;
        public SectorType rightType;
        public ushort leftNode;
        public ushort rightNode;
        public float leftValue;
        public float rightValue;
    }

    public struct RpCollBSPLeafNode
    {
        public ushort firstPolygon;
        public ushort numPolygons;
    }

    public class CollisionPLG_011D_Pre36001 : RWSection
    {
        public RpCollBSPBranchNode[] branchNodes;
        public RpCollBSPLeafNode[] leafNodes;
        public int[] triangleMap;

        public CollisionPLG_011D_Pre36001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.CollisionPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            int numLeafNodes = binaryReader.ReadInt32();
            int numTriangles = binaryReader.ReadInt32();

            branchNodes = new RpCollBSPBranchNode[numLeafNodes - 1];

            for (int i = 0; i < numLeafNodes - 1; i++)
                branchNodes[i] = new RpCollBSPBranchNode
                {
                    rightType = (SectorType)binaryReader.ReadByte(),
                    leftType = (SectorType)binaryReader.ReadByte(),
                    type = (SectorAxis)binaryReader.ReadUInt16(),

                    rightNode = binaryReader.ReadUInt16(),
                    leftNode = binaryReader.ReadUInt16(),

                    leftValue = binaryReader.ReadSingle(),
                    rightValue = binaryReader.ReadSingle()
                };

            leafNodes = new RpCollBSPLeafNode[numLeafNodes];

            for (int i = 0; i < numLeafNodes; i++)
                leafNodes[i] = new RpCollBSPLeafNode
                {
                    firstPolygon = binaryReader.ReadUInt16(),
                    numPolygons = binaryReader.ReadUInt16()
                };

            triangleMap = new int[numTriangles];

            for (int i = 0; i < numTriangles; i++)
                triangleMap[i] = binaryReader.ReadInt32();

            return this;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.CollisionPLG;

            listBytes.AddRange(BitConverter.GetBytes(branchNodes.Length + 1));
            listBytes.AddRange(BitConverter.GetBytes(triangleMap.Length));

            foreach (var split in branchNodes)
            {
                listBytes.Add((byte)split.rightType);
                listBytes.Add((byte)split.leftType);
                listBytes.AddRange(BitConverter.GetBytes((ushort)split.type));

                listBytes.AddRange(BitConverter.GetBytes(split.rightNode));
                listBytes.AddRange(BitConverter.GetBytes(split.leftNode));

                listBytes.AddRange(BitConverter.GetBytes(split.leftValue));
                listBytes.AddRange(BitConverter.GetBytes(split.rightValue));
            }

            foreach (var leaf in leafNodes)
            {
                listBytes.AddRange(BitConverter.GetBytes(leaf.firstPolygon));
                listBytes.AddRange(BitConverter.GetBytes(leaf.numPolygons));
            }

            foreach (var i in triangleMap)
                listBytes.AddRange(BitConverter.GetBytes(i));
        }
    }
}
