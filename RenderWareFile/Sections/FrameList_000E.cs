using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class FrameList_000E : RWSection
    {
        public FrameListStruct_0001 frameListStruct;
        public List<Extension_0003> extensionList;

        public FrameList_000E Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.FrameList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            Section frameListStructSection = (Section)binaryReader.ReadInt32();
            if (frameListStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.ToString());
            frameListStruct = new FrameListStruct_0001().Read(binaryReader);

            extensionList = new List<Extension_0003>();
            for (int i = 0; i < frameListStruct.frames.Count; i++)
            {
                Section frameListExtensionSection = (Section)binaryReader.ReadInt32();
                if (frameListExtensionSection != Section.Extension) throw new Exception(binaryReader.BaseStream.ToString());
                extensionList.Add(new Extension_0003().Read(binaryReader));
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.FrameList;

            listBytes.AddRange(frameListStruct.GetBytes(fileVersion));
            foreach (Extension_0003 i in extensionList)
                listBytes.AddRange(i.GetBytes(fileVersion));
        }
    }
}