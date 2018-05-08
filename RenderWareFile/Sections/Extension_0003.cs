using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace RenderWareFile
{
    public class Extension_0003 : RWSection
    {
        public List<RWSection> extensionSectionList;

        public Extension_0003 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Extension;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long CurrentPosition = binaryReader.BaseStream.Position;

            extensionSectionList = new List<RWSection>();
            while (binaryReader.BaseStream.Position < CurrentPosition + sectionSize)
            {
                Section currentSection = (Section)binaryReader.ReadInt32();
                if (currentSection == Section.BinMeshPLG) extensionSectionList.Add(new BinMeshPLG_050E().Read(binaryReader));
                else if (currentSection == Section.NativeDataPLG) extensionSectionList.Add(new NativeDataPLG_0510().Read(binaryReader));
                else extensionSectionList.Add(new GenericSection().Read(binaryReader, currentSection));
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Extension;

            if (extensionSectionList != null)
                for (int i = 0; i < extensionSectionList.Count(); i++)
                    listBytes.AddRange(extensionSectionList[i].GetBytes(fileVersion));
        }
    }
}
