using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace RenderWareFile.Sections
{
    public class String_0002 : RWSection
    {
        public String_0002()
        {
            stringString = "";
        }

        public String_0002(string value)
        {
            stringString = value;
        }

        public string stringString { get; private set; }

        public String_0002 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.String;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            stringString = Shared.ReadFromZeroTerminatedString(binaryReader);

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.String;

            foreach(char i in stringString)
                listBytes.Add((byte)i);

            if (stringString.Length % 4 == 0) listBytes.AddRange(new byte[] { 0, 0, 0, 0});
            if (stringString.Length % 4 == 1) listBytes.AddRange(new byte[] { 0, 0, 0 });
            if (stringString.Length % 4 == 2) listBytes.AddRange(new byte[] { 0, 0 });
            if (stringString.Length % 4 == 3) listBytes.Add(0);
        }
    }
}
