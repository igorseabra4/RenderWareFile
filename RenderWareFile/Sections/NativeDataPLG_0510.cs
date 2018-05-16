using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class NativeDataPLG_0510 : RWSection
    {
        public NativeDataStruct_0001 nativeDataStruct;
        
        public NativeDataPLG_0510 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.NativeDataPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section nativeStructSection = (Section)binaryReader.ReadInt32();
            if (nativeStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.Position.ToString());
            nativeDataStruct = new NativeDataStruct_0001().Read(binaryReader);

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.NativeDataPLG;
            listBytes.AddRange(nativeDataStruct.GetBytes(fileVersion));
        }
    }
}
