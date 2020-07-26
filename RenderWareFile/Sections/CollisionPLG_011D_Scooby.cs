using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public enum ScoobySectorType : byte
    {
        Leaf = 0x01,
        Branch = 0x02,
    }

    public struct Split_Scooby
    {
        public ScoobySectorType positiveType;
        public ScoobySectorType negativeType;
        public SectorType splitDirection;
        public byte padding;

        // index of startIndex_amountOfTriangles entry in list if leaf node, index of split in split list for branch node
        public short positiveIndex; 
        public short negativeIndex;

        public float negativeSplitPos;
        public float positiveSplitPos;
    }

    public class CollisionPLG_011D_Scooby : RWSection
    {
        public Split_Scooby[] splits;
        public short[][] startIndex_amountOfTriangles;
        public int[] triangles;

        public CollisionPLG_011D_Scooby Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.CollisionPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            int numLeafNodes = binaryReader.ReadInt32();
            int numTriangles = binaryReader.ReadInt32();

            splits = new Split_Scooby[numLeafNodes - 1];
            
            for (int i = 0; i < numLeafNodes - 1; i++)
                splits[i] = new Split_Scooby
                {
                    positiveType = (ScoobySectorType)binaryReader.ReadByte(),
                    negativeType = (ScoobySectorType)binaryReader.ReadByte(),
                    splitDirection = (SectorType)binaryReader.ReadByte(),
                    padding = binaryReader.ReadByte(),

                    positiveIndex = binaryReader.ReadInt16(),
                    negativeIndex = binaryReader.ReadInt16(),

                    positiveSplitPos = binaryReader.ReadSingle(),
                    negativeSplitPos = binaryReader.ReadSingle()
                };

            startIndex_amountOfTriangles = new short[numLeafNodes][];

            for (int i = 0; i < numLeafNodes; i++)
                startIndex_amountOfTriangles[i] = new short[]
                {
                    binaryReader.ReadInt16(),
                    binaryReader.ReadInt16()
                };

            triangles = new int[numTriangles];

            for (int i = 0; i < numTriangles; i++)
                triangles[i] = binaryReader.ReadInt32();

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.CollisionPLG;

            listBytes.AddRange(BitConverter.GetBytes(splits.Length + 1));
            listBytes.AddRange(BitConverter.GetBytes(triangles.Length));

            foreach (var split in splits)
            {
                listBytes.Add((byte)split.positiveType);
                listBytes.Add((byte)split.negativeType);
                listBytes.Add((byte)split.splitDirection);
                listBytes.Add(split.padding);

                listBytes.AddRange(BitConverter.GetBytes(split.positiveIndex));
                listBytes.AddRange(BitConverter.GetBytes(split.negativeIndex));

                listBytes.AddRange(BitConverter.GetBytes(split.positiveSplitPos));
                listBytes.AddRange(BitConverter.GetBytes(split.negativeSplitPos));
            }

            foreach (var i in startIndex_amountOfTriangles)
                foreach (var j in i)
                    listBytes.AddRange(BitConverter.GetBytes(j));

            foreach (var i in triangles)
                listBytes.AddRange(BitConverter.GetBytes(i));
        }
    }
}
