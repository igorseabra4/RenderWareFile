using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderWareFile
{
    public class GeometryList_001A : RWSection
    {
        public GeometryListStruct_0001 geometryListStruct;
        public List<Geometry_000F> geometryList = new List<Geometry_000F>();

        public GeometryList_001A Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.GeometryList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section geometryListStructSection = (Section)binaryReader.ReadInt32();
            if (geometryListStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            geometryListStruct = new GeometryListStruct_0001().Read(binaryReader);

            geometryList.Clear();
            for (int i = 0; i < geometryListStruct.numberOfGeometries; i++)
            {
                Section geometryListSection = (Section)binaryReader.ReadInt32();
                if (geometryListSection != Section.Geometry) throw new Exception(binaryReader.BaseStream.Position.ToString());
                geometryList.Add(new Geometry_000F().Read(binaryReader));
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.GeometryList;

            listBytes.AddRange(geometryListStruct.GetBytes(fileVersion));
            for (int i = 0; i < geometryList.Count; i++)
                listBytes.AddRange(geometryList[i].GetBytes(fileVersion));
        }        
    }

    public class GeometryListStruct_0001 : RWSection
    {
        public int numberOfGeometries;

        public GeometryListStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            numberOfGeometries = binaryReader.ReadInt32();

            return this;
        }
        
        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(numberOfGeometries));
        }
    }
}
