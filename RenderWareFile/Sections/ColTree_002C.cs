using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class ColTree_002C : RWSection
    {
        public RWSection colTreeStruct;

        public ColTree_002C Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.ColTree;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();
            
            Section colTreeStructSection = (Section)binaryReader.ReadInt32();

            if (colTreeStructSection == Section.Struct)
                colTreeStruct = new ColTreeStruct_0001().Read(binaryReader);
            else throw new System.Exception();
            
            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.ColTree;

            listBytes.AddRange(colTreeStruct.GetBytes(fileVersion));
        }
    }
}