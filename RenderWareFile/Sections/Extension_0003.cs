using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RenderWareFile
{
    public class Extension_0003 : RWSection
    {
        List<RWSection> ExtensionSectionList;

        public Extension_0003 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Extension;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long CurrentPosition = binaryReader.BaseStream.Position;

            ExtensionSectionList = new List<RWSection>();
            while (binaryReader.BaseStream.Position < CurrentPosition + sectionSize)
            {
                Section currentSection = (Section)binaryReader.ReadInt32();
                if (currentSection == Section.NativeDataPLG) ExtensionSectionList.Add(new NativeDataPLG_0510().Read(binaryReader));
                else ExtensionSectionList.Add(new GenericSection().Read(binaryReader, currentSection));
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Extension;
        }
    }
}
