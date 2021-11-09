using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class Split
    {
        public Sector negativeSector;
        public Sector positiveSector;

        public Split()
        {
            negativeSector = new Sector();
            positiveSector = new Sector();
        }
    }

    public enum SectorType : byte
    {
        PositiveX = 0x00,
        NegativeX = 0x01,
        PositiveY = 0x04,
        NegativeY = 0x05,
        PositiveZ = 0x08,
        NegativeZ = 0x09
    }

    public class Sector
    {
        public SectorType type;
        public byte triangleAmount;
        public ushort referenceIndex;
        public float splitPosition;

        public Vertex3 Min;
        public Vertex3 Max;
        public List<ushort> TriangleIndexList;

        public Sector()
        {
            TriangleIndexList = new List<ushort>();
        }
    }

    public class ColTreeStruct_0001 : RWSection
    {
        public int useMap;
        public Vertex3 boxMinimum;
        public Vertex3 boxMaximum;
        public int numTriangles;
        public int numSplits;

        public Split[] splitArray;
        public ushort[] triangleArray;

        public ColTreeStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            useMap = binaryReader.ReadInt32();
            boxMinimum = new Vertex3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            boxMaximum = new Vertex3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            numTriangles = binaryReader.ReadInt32();
            numSplits = binaryReader.ReadInt32();

            splitArray = new Split[numSplits];

            for (int i = 0; i < numSplits; i++)
            {
                splitArray[i] = new Split();
                splitArray[i].negativeSector = new Sector
                {
                    type = (SectorType)binaryReader.ReadByte(),
                    triangleAmount = binaryReader.ReadByte(),
                    referenceIndex = binaryReader.ReadUInt16(),
                    splitPosition = binaryReader.ReadSingle()
                };

                splitArray[i].positiveSector = new Sector
                {
                    type = (SectorType)binaryReader.ReadByte(),
                    triangleAmount = binaryReader.ReadByte(),
                    referenceIndex = binaryReader.ReadUInt16(),
                    splitPosition = binaryReader.ReadSingle()
                };
            }

            triangleArray = new ushort[numTriangles];

            for (int i = 0; i < numTriangles; i++)
            {
                triangleArray[i] = binaryReader.ReadUInt16();
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(useMap));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum.X));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum.Y));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum.Z));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum.X));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Y));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Z));
            listBytes.AddRange(BitConverter.GetBytes(numTriangles));
            listBytes.AddRange(BitConverter.GetBytes(numSplits));

            for (int i = 0; i < numSplits; i++)
            {
                listBytes.Add((byte)splitArray[i].negativeSector.type);
                listBytes.Add(splitArray[i].negativeSector.triangleAmount);
                listBytes.AddRange(BitConverter.GetBytes(splitArray[i].negativeSector.referenceIndex));
                listBytes.AddRange(BitConverter.GetBytes(splitArray[i].negativeSector.splitPosition));

                listBytes.Add((byte)splitArray[i].positiveSector.type);
                listBytes.Add(splitArray[i].positiveSector.triangleAmount);
                listBytes.AddRange(BitConverter.GetBytes(splitArray[i].positiveSector.referenceIndex));
                listBytes.AddRange(BitConverter.GetBytes(splitArray[i].positiveSector.splitPosition));
            }

            if (triangleArray != null)
                for (int i = 0; i < numTriangles; i++)
                {
                    listBytes.AddRange(BitConverter.GetBytes(triangleArray[i]));
                }
        }
    }
}