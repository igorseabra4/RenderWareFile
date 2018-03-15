using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public class World_000B : RWSection
    {
        public WorldStruct_0001 worldStruct;
        public MaterialList_0008 materialList;
        public RWSection firstWorldChunk;
        public Extension_0003 worldExtension;

        public World_000B Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.World;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section worldStructSection = (Section)binaryReader.ReadInt32();
            if (worldStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            worldStruct = new WorldStruct_0001().Read(binaryReader);

            Section materialListSection = (Section)binaryReader.ReadInt32();
            if (materialListSection != Section.MaterialList) throw new Exception();
            materialList = new MaterialList_0008().Read(binaryReader);

            Section firstWorldChunkSection = (Section)binaryReader.ReadInt32();
            if (firstWorldChunkSection == Section.AtomicSection)
            {
                firstWorldChunk = new AtomicSection_0009().Read(binaryReader);
            }
            else if (firstWorldChunkSection == Section.PlaneSection)
            {
                firstWorldChunk = new PlaneSection_000A().Read(binaryReader);
            }
            else throw new Exception();

            Section worldExtensionSection = (Section)binaryReader.ReadInt32();
            if (worldExtensionSection == Section.Extension)
                worldExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.World;

            listBytes.AddRange(worldStruct.GetBytes(fileVersion));
            listBytes.AddRange(materialList.GetBytes(fileVersion));
            listBytes.AddRange(firstWorldChunk.GetBytes(fileVersion));
            listBytes.AddRange(worldExtension.GetBytes(fileVersion));
        }
    }

    public class WorldStruct_0001 : RWSection
    {
        public int structVersion;
        public int unknown2;
        public int unknown3;
        public int unknown4;
        public int numTriangles;
        public int numVertices;
        public int numPlaneSections;
        public int numAtomicSections;
        public int unknown5;
        public int unknown6;
        public float[] boxMaximum = new float[3];
        public float[] boxMinimum = new float[3];

        public WorldStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            if (sectionSize == 0x40)
            {
                structVersion = binaryReader.ReadInt32();
                unknown2 = binaryReader.ReadInt32();
                unknown3 = binaryReader.ReadInt32();
                unknown4 = binaryReader.ReadInt32();
                numTriangles = binaryReader.ReadInt32();
                numVertices = binaryReader.ReadInt32();
                numPlaneSections = binaryReader.ReadInt32();
                numAtomicSections = binaryReader.ReadInt32();
                unknown5 = binaryReader.ReadInt32();
                unknown6 = binaryReader.ReadInt32();
                boxMaximum = new float[3];
                boxMaximum[0] = binaryReader.ReadSingle();
                boxMaximum[1] = binaryReader.ReadSingle();
                boxMaximum[2] = binaryReader.ReadSingle();
                boxMinimum = new float[3];
                boxMinimum[0] = binaryReader.ReadSingle();
                boxMinimum[1] = binaryReader.ReadSingle();
                boxMinimum[2] = binaryReader.ReadSingle();
            }
            else if (sectionSize == 0x34)
            {
                structVersion = binaryReader.ReadInt32();
                unknown2 = binaryReader.ReadInt32();
                unknown3 = binaryReader.ReadInt32();
                unknown4 = binaryReader.ReadInt32();
                boxMaximum = new float[3];
                boxMaximum[0] = binaryReader.ReadSingle();
                boxMaximum[1] = binaryReader.ReadSingle();
                boxMaximum[2] = binaryReader.ReadSingle();
                boxMinimum = boxMaximum;
                numTriangles = binaryReader.ReadInt32();
                numVertices = binaryReader.ReadInt32();
                numPlaneSections = binaryReader.ReadInt32();
                numAtomicSections = binaryReader.ReadInt32();
                unknown5 = binaryReader.ReadInt32();
                unknown6 = binaryReader.ReadInt32();
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(structVersion));
            listBytes.AddRange(BitConverter.GetBytes(unknown2));
            listBytes.AddRange(BitConverter.GetBytes(unknown3));
            listBytes.AddRange(BitConverter.GetBytes(unknown4));
            listBytes.AddRange(BitConverter.GetBytes(numTriangles));
            listBytes.AddRange(BitConverter.GetBytes(numVertices));
            listBytes.AddRange(BitConverter.GetBytes(numPlaneSections));
            listBytes.AddRange(BitConverter.GetBytes(numAtomicSections));
            listBytes.AddRange(BitConverter.GetBytes(unknown5));
            listBytes.AddRange(BitConverter.GetBytes(unknown6));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum[0]));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum[1]));
            listBytes.AddRange(BitConverter.GetBytes(boxMaximum[2]));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum[0]));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum[1]));
            listBytes.AddRange(BitConverter.GetBytes(boxMinimum[2]));
        }
    }
}
