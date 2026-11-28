using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class GeometryListStruct_0001 : RWSection
    {
        public int numberOfGeometries { get; internal set; }

        public GeometryListStruct_0001(int numberOfGeometries)
        {
            this.numberOfGeometries = numberOfGeometries;
        }

        public GeometryListStruct_0001(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            numberOfGeometries = binaryReader.ReadInt32();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(numberOfGeometries));
        }
    }
}
