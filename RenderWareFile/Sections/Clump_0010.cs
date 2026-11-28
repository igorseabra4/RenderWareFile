using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class Clump_0010 : RWSection
    {
        public ClumpStruct_0001 clumpStruct { get; internal set; }
        public FrameList_000E frameList { get; set; }
        public GeometryList_001A geometryList { get; set; }
        public Atomic_0014[] atomicList { get; set; }
        public Extension_0003 clumpExtension { get; set; }

        public Clump_0010(FrameList_000E frameList, GeometryList_001A geometryList, Atomic_0014[] atomicList, Extension_0003 clumpExtension)
        {
            this.clumpStruct = new ClumpStruct_0001(atomicList.Length, 0, 0);
            this.frameList = frameList;
            this.geometryList = geometryList;
            this.atomicList = atomicList;
            this.clumpExtension = clumpExtension;
        }

        public Clump_0010(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Clump;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            Section clumpStructSection = (Section)binaryReader.ReadInt32();
            if (clumpStructSection != Section.Struct) throw new Exception(binaryReader.BaseStream.ToString());
            clumpStruct = new ClumpStruct_0001(binaryReader);

            Section frameListSection = (Section)binaryReader.ReadInt32();
            if (frameListSection != Section.FrameList) throw new Exception(binaryReader.BaseStream.ToString());
            frameList = new FrameList_000E(binaryReader);

            Section geometryListSection = (Section)binaryReader.ReadInt32();
            if (geometryListSection != Section.GeometryList) throw new Exception(binaryReader.BaseStream.ToString());
            geometryList = new GeometryList_001A(binaryReader);

            atomicList = new Atomic_0014[clumpStruct.atomicCount];
            for (int i = 0; i < atomicList.Length; i++)
            {
                Section atomicListSection = (Section)binaryReader.ReadInt32();
                if (atomicListSection != Section.Atomic) throw new Exception(binaryReader.BaseStream.Position.ToString());
                atomicList[i] = new Atomic_0014(binaryReader);
            }

            Section clumpExtensionSection = (Section)binaryReader.ReadInt32();
            if (clumpExtensionSection == Section.Extension)
                clumpExtension = new Extension_0003(binaryReader);

            //binaryReader.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Clump;

            listBytes.AddRange(clumpStruct.GetBytes(fileVersion));
            listBytes.AddRange(frameList.GetBytes(fileVersion));
            listBytes.AddRange(geometryList.GetBytes(fileVersion));
            for (int i = 0; i < atomicList.Length; i++)
                listBytes.AddRange(atomicList[i].GetBytes(fileVersion));
            listBytes.AddRange(clumpExtension.GetBytes(fileVersion));
        }
    }
}
