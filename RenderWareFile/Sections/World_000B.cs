using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class World_000B : RWSection
    {
        public WorldStruct_0001 worldStruct { get; set; }
        public MaterialList_0008 materialList { get; set; }
        public RWSection firstWorldChunk { get; set; }
        public Extension_0003 worldExtension { get; set; }

        public World_000B() { }
        public World_000B(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.World;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section worldStructSection = (Section)binaryReader.ReadInt32();
            if (worldStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            worldStruct = new WorldStruct_0001(binaryReader);

            Section materialListSection = (Section)binaryReader.ReadInt32();
            if (materialListSection != Section.MaterialList) throw new Exception();
            materialList = new MaterialList_0008(binaryReader);

            Section firstWorldChunkSection = (Section)binaryReader.ReadInt32();
            if (firstWorldChunkSection == Section.AtomicSector)
            {
                firstWorldChunk = new AtomicSector_0009(binaryReader);
            }
            else if (firstWorldChunkSection == Section.PlaneSector)
            {
                firstWorldChunk = new PlaneSector_000A(binaryReader);
            }
            else throw new Exception();

            Section worldExtensionSection = (Section)binaryReader.ReadInt32();
            if (worldExtensionSection == Section.Extension)
                worldExtension = new Extension_0003(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.World;

            listBytes.AddRange(worldStruct.GetBytes(fileVersion));
            listBytes.AddRange(materialList.GetBytes(fileVersion));
            listBytes.AddRange(firstWorldChunk.GetBytes(fileVersion));
            listBytes.AddRange(worldExtension.GetBytes(fileVersion));
        }
    }
}
