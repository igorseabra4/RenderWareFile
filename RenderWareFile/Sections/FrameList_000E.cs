using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class FrameList_000E : RWSection
    {
        public FrameListStruct_0001 frameListStruct { get; internal set; }
        public Extension_0003[] extensions { get; set; }

        public FrameList_000E(Frame[] frames, Extension_0003[] extensions)
        {
            this.frameListStruct = new FrameListStruct_0001(frames);
            this.extensions = extensions;
        }

        public FrameList_000E(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.FrameList;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            Section frameListStructSection = (Section)binaryReader.ReadInt32();
            if (frameListStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.ToString());
            frameListStruct = new FrameListStruct_0001(binaryReader);

            extensions = new Extension_0003[frameListStruct.frames.Length];
            for (int i = 0; i < extensions.Length; i++)
            {
                Section frameListExtensionSection = (Section)binaryReader.ReadInt32();
                if (frameListExtensionSection != Section.Extension) throw new Exception(binaryReader.BaseStream.ToString());
                extensions[i] = new Extension_0003(binaryReader);
            }
        }


        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.FrameList;

            listBytes.AddRange(frameListStruct.GetBytes(fileVersion));
            foreach (Extension_0003 i in extensions)
                listBytes.AddRange(i.GetBytes(fileVersion));
        }
    }
}