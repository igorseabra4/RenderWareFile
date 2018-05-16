using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
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
            if (firstWorldChunkSection == Section.AtomicSector)
            {
                firstWorldChunk = new AtomicSector_0009().Read(binaryReader);
            }
            else if (firstWorldChunkSection == Section.PlaneSector)
            {
                firstWorldChunk = new PlaneSector_000A().Read(binaryReader);
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
}
