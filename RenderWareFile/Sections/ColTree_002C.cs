using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class ColTree_002C : RWSection
    {
        public RWSection colTreeStruct { get; set; }

        public ColTree_002C() { }
        public ColTree_002C(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.ColTree;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            Section colTreeStructSection = (Section)binaryReader.ReadInt32();

            if (colTreeStructSection == Section.Struct)
                colTreeStruct = new ColTreeStruct_0001(binaryReader);
            else throw new System.Exception();
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.ColTree;

            listBytes.AddRange(colTreeStruct.GetBytes(fileVersion));
        }
    }
}