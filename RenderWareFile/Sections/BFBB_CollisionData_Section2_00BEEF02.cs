using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public struct Struct3
    {
        public int unknown1;
        public int unknown2;
    }

    public class BFBB_CollisionData_Section2_00BEEF02 : RWSection
    {
        public string JSP_;
        public int unknownAmount1;
        public int unknownAmount2;
        public int null1;
        public int null2;
        public int null3;
        public List<Struct3> list3;
                
        public BFBB_CollisionData_Section2_00BEEF02 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section2;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            long startSectionPosition = binaryReader.BaseStream.Position;

            JSP_ = new string(binaryReader.ReadChars(4).ToArray());
            
            unknownAmount1 = SwitchToggleable(binaryReader.ReadInt32());
            unknownAmount2 = SwitchToggleable(binaryReader.ReadInt32());
            null1 = SwitchToggleable(binaryReader.ReadInt32());
            null2 = SwitchToggleable(binaryReader.ReadInt32());
            null3 = SwitchToggleable(binaryReader.ReadInt32());

            list3 = new List<Struct3>(unknownAmount2);
            for (int i = 0; i < unknownAmount2; i++)
            {
                list3.Add(new Struct3()
                {
                    unknown1 = SwitchToggleable(binaryReader.ReadInt32()),
                    unknown2 = SwitchToggleable(binaryReader.ReadInt32())
                });
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section2;

            foreach (byte b in JSP_)
                listBytes.Add(b);
            
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(unknownAmount1)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(unknownAmount2)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(null1)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(null2)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(null3)));
            
            for (int i = 0; i < unknownAmount2; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(list3[i].unknown1)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(list3[i].unknown2)));
            }
        }
    }
}
