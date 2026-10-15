using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenderWareFile.Sections
{
    public class Extension_0003 : RWSection
    {
        public RWSection[] extensionSectionList { get; set; }

        public Extension_0003()
        {
            extensionSectionList = new RWSection[0];
        }

        public Extension_0003(RWSection[] extensionSectionList)
        {
            this.extensionSectionList = extensionSectionList;
        }

        public Extension_0003(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Extension;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            long CurrentPosition = binaryReader.BaseStream.Position;

            var extensionSectionList = new List<RWSection>();
            while (binaryReader.BaseStream.Position < CurrentPosition + sectionSize)
            {
                Section currentSection = (Section)binaryReader.ReadInt32();
                if (currentSection == Section.BinMeshPLG) extensionSectionList.Add(new BinMeshPLG_050E(binaryReader));
                else if (currentSection == Section.NativeDataPLG) extensionSectionList.Add(new NativeDataPLG_0510(binaryReader));
                else if (currentSection == Section.CollisionPLG)
                {
                    binaryReader.BaseStream.Position += 8;
                    int version = binaryReader.ReadInt32();
                    binaryReader.BaseStream.Position -= 12;

                    if (version >= 0x36001)
                        extensionSectionList.Add(new CollisionPLG_011D(binaryReader));
                    else
                        extensionSectionList.Add(new CollisionPLG_011D_Pre36001(binaryReader));
                }
                else if (currentSection == Section.UserDataPLG) extensionSectionList.Add(new UserDataPLG_011F(binaryReader));
                else if (currentSection == Section.MaterialEffectsPLG) extensionSectionList.Add(new MaterialEffectsPLG_0120(binaryReader));
                else extensionSectionList.Add(new GenericSection(binaryReader, currentSection));
            }
            this.extensionSectionList = extensionSectionList.ToArray();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Extension;
            
            for (int i = 0; i < extensionSectionList.Length; i++)
                listBytes.AddRange(extensionSectionList[i].GetBytes(fileVersion));
        }

        public void Add(RWSection section)
        {
            var list = extensionSectionList.ToList();
            list.Add(section);
            extensionSectionList = list.ToArray();
        }

        public void RemoveAll(Predicate<RWSection> predicate)
        {
            var list = extensionSectionList.ToList();
            list.RemoveAll(predicate);
            extensionSectionList = list.ToArray();
        }
    }
}
