using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class CollisionPLG_011D : RWSection
    {
        public int version { get; set; }
        public RWSection colTree { get; set; }

        public CollisionPLG_011D() { }
        public CollisionPLG_011D(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.CollisionPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            version = binaryReader.ReadInt32();

            Section colTreeSection = (Section)binaryReader.ReadInt32();
            if (colTreeSection == Section.ColTree)
                colTree = new ColTree_002C(binaryReader);
            else throw new Exception();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.CollisionPLG;

            listBytes.AddRange(BitConverter.GetBytes(version));
            listBytes.AddRange(colTree.GetBytes(fileVersion));
        }
    }
}
