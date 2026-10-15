using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class GeometryList_001A : RWSection
    {
        public GeometryListStruct_0001 geometryListStruct { get; internal set; }
        public Geometry_000F[] geometryList { get; set; }

        public GeometryList_001A(Geometry_000F[] geometryList)
        {
            geometryListStruct = new GeometryListStruct_0001(geometryList.Length);
            this.geometryList = geometryList;
        }

        public GeometryList_001A(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.GeometryList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section geometryListStructSection = (Section)binaryReader.ReadInt32();
            if (geometryListStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryListStruct = new GeometryListStruct_0001(binaryReader);

            geometryList = new Geometry_000F[geometryListStruct.numberOfGeometries];
            for (int i = 0; i < geometryList.Length; i++)
            {
                Section geometryListSection = (Section)binaryReader.ReadInt32();
                if (geometryListSection != Section.Geometry) throw new Exception(binaryReader.BaseStream.Position.ToString());
                geometryList[i] = new Geometry_000F(binaryReader);
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.GeometryList;

            listBytes.AddRange(geometryListStruct.GetBytes(fileVersion));
            for (int i = 0; i < geometryList.Length; i++)
                listBytes.AddRange(geometryList[i].GetBytes(fileVersion));
        }
    }
}
