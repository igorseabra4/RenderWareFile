using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public enum WorldFlags
    {
        UseTriangleStrips = 0x00000001,
        HasVertexPositions = 0x00000002,
        HasOneSetOfTextCoords = 0x00000004,
        HasVertexColors = 0x00000008,
        HasNormals = 0x00000010,
        UseLighting = 0x00000020,
        ModulateMaterialColors = 0x00000040,
        HasMultipleSetsOfTextCoords = 0x00000080,
        IsNativeGeometry = 0x01000000,
        IsNativeInstance = 0x02000000,
        FlagsMask = 0x000000FF,
        NativeFlagsMask = 0x0F000000,
        WorldSectorsOverlap = 0x40000000
    }

    public class WorldStruct_0001 : RWSection
    {
        public int rootIsWorldSector;
        public Vertex3 inverseOrigin;
        public uint numTriangles;
        public uint numVertices;
        public uint numPlaneSectors;
        public uint numAtomicSectors;
        public uint colSectorSize;
        public WorldFlags worldFlags;
        public Vertex3 boxMaximum;
        public Vertex3 boxMinimum;

        public WorldStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            if (sectionSize == 0x40)
            {
                rootIsWorldSector = binaryReader.ReadInt32();
                inverseOrigin.X = binaryReader.ReadSingle();
                inverseOrigin.Y = binaryReader.ReadSingle();
                inverseOrigin.Z = binaryReader.ReadSingle();
                numTriangles = binaryReader.ReadUInt32();
                numVertices = binaryReader.ReadUInt32();
                numPlaneSectors = binaryReader.ReadUInt32();
                numAtomicSectors = binaryReader.ReadUInt32();
                colSectorSize = binaryReader.ReadUInt32();
                worldFlags = (WorldFlags)binaryReader.ReadUInt32();
                boxMaximum = new Vertex3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                boxMinimum = new Vertex3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            }
            else if (sectionSize == 0x34)
            {
                rootIsWorldSector = binaryReader.ReadInt32();
                inverseOrigin.X = binaryReader.ReadSingle();
                inverseOrigin.Y = binaryReader.ReadSingle();
                inverseOrigin.Z = binaryReader.ReadSingle();
                boxMaximum = new Vertex3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                boxMinimum = boxMaximum;
                numTriangles = binaryReader.ReadUInt32();
                numVertices = binaryReader.ReadUInt32();
                numPlaneSectors = binaryReader.ReadUInt32();
                numAtomicSectors = binaryReader.ReadUInt32();
                colSectorSize = binaryReader.ReadUInt32();
                worldFlags = (WorldFlags)binaryReader.ReadUInt32();
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;
            
            if (fileVersion == 0x0310)
            {
                listBytes.AddRange(BitConverter.GetBytes(rootIsWorldSector));
                listBytes.AddRange(BitConverter.GetBytes(inverseOrigin.X));
                listBytes.AddRange(BitConverter.GetBytes(inverseOrigin.Y));
                listBytes.AddRange(BitConverter.GetBytes(inverseOrigin.Z));
                listBytes.AddRange(BitConverter.GetBytes(boxMaximum.X));
                listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Y));
                listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Z));
                listBytes.AddRange(BitConverter.GetBytes(numTriangles));
                listBytes.AddRange(BitConverter.GetBytes(numVertices));
                listBytes.AddRange(BitConverter.GetBytes(numPlaneSectors));
                listBytes.AddRange(BitConverter.GetBytes(numAtomicSectors));
                listBytes.AddRange(BitConverter.GetBytes(colSectorSize));
                listBytes.AddRange(BitConverter.GetBytes((int)worldFlags));
            }
            else
            {
                listBytes.AddRange(BitConverter.GetBytes(rootIsWorldSector));
                listBytes.AddRange(BitConverter.GetBytes(inverseOrigin.X));
                listBytes.AddRange(BitConverter.GetBytes(inverseOrigin.Y));
                listBytes.AddRange(BitConverter.GetBytes(inverseOrigin.Z));
                listBytes.AddRange(BitConverter.GetBytes(numTriangles));
                listBytes.AddRange(BitConverter.GetBytes(numVertices));
                listBytes.AddRange(BitConverter.GetBytes(numPlaneSectors));
                listBytes.AddRange(BitConverter.GetBytes(numAtomicSectors));
                listBytes.AddRange(BitConverter.GetBytes(colSectorSize));
                listBytes.AddRange(BitConverter.GetBytes((int)worldFlags));
                listBytes.AddRange(BitConverter.GetBytes(boxMaximum.X));
                listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Y));
                listBytes.AddRange(BitConverter.GetBytes(boxMaximum.Z));
                listBytes.AddRange(BitConverter.GetBytes(boxMinimum.X));
                listBytes.AddRange(BitConverter.GetBytes(boxMinimum.Y));
                listBytes.AddRange(BitConverter.GetBytes(boxMinimum.Z));
            }
        }
    }
}
