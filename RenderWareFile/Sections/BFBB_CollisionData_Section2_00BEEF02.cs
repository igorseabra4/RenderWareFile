using System;
using System.Collections.Generic;
using System.IO;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public struct xJSPNodeInfo
    {
        public int originalMatIndex { get; set; }
        public int nodeFlags { get; set; }
    }

    public class BFBB_CollisionData_Section2_00BEEF02 : RWSection
    {
        public int JSP_ { get; set; }
        public int version { get; set; }
        public int null1 { get; set; }
        public int null2 { get; set; }
        public int null3 { get; set; }
        public xJSPNodeInfo[] jspNodeList { get; set; }

        public BFBB_CollisionData_Section2_00BEEF02 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section2;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            JSP_ = SwitchToggleable(binaryReader.ReadInt32());

            version = SwitchToggleable(binaryReader.ReadInt32());
            int jspNodeCount = SwitchToggleable(binaryReader.ReadInt32());
            null1 = SwitchToggleable(binaryReader.ReadInt32());
            null2 = SwitchToggleable(binaryReader.ReadInt32());
            null3 = SwitchToggleable(binaryReader.ReadInt32());

            jspNodeList = new xJSPNodeInfo[jspNodeCount];
            for (int i = 0; i < jspNodeCount; i++)
                jspNodeList[i] = new xJSPNodeInfo()
                {
                    originalMatIndex = SwitchToggleable(binaryReader.ReadInt32()),
                    nodeFlags = SwitchToggleable(binaryReader.ReadInt32())
                };

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section2;

            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(JSP_)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(version)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(jspNodeList.Length)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(null1)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(null2)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(null3)));

            for (int i = 0; i < jspNodeList.Length; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(jspNodeList[i].originalMatIndex)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(jspNodeList[i].nodeFlags)));
            }
        }
    }
}
