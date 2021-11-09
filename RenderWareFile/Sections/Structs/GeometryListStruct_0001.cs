using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
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
