using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public enum GeometryFlags
    {
        isTristrip = 0x0001,
        hasVertexPositions = 0x0002,
        hasTextCoords = 0x0004,
        hasVertexColors = 0x0008,
        hasNormals = 0x0010,
        hasLights = 0x0020,
        modeulateMaterialColor = 0x00040,
        hasTextCoords2 = 0x0080
    }

    public enum GeometryFlags2
    {
        isNativeGeometry = 0x0100
    }

    public class Geometry_000F : RWSection
    {
        public GeometryStruct_0001 geometryStruct;
        public MaterialList_0008 materialList;
        public Extension_0003 geometryExtension;

        public Geometry_000F Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Geometry;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section geometryStructSection = (Section)binaryReader.ReadInt32();
            if (geometryStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryStruct = new GeometryStruct_0001().Read(binaryReader);

            Section materialListSection = (Section)binaryReader.ReadInt32();
            if (materialListSection != Section.MaterialList) throw new Exception(binaryReader.BaseStream.Position.ToString());
            materialList = new MaterialList_0008().Read(binaryReader);

            Section geometryExtensionSection = (Section)binaryReader.ReadInt32();
            if (geometryExtensionSection != Section.Extension) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryExtension = new Extension_0003().Read(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }
        
        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Geometry;

            listBytes.AddRange(geometryStruct.GetBytes(fileVersion));
            listBytes.AddRange(materialList.GetBytes(fileVersion));
            listBytes.AddRange(geometryExtension.GetBytes(fileVersion));
        }
    }
}
