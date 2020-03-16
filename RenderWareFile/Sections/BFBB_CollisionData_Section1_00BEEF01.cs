using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public struct xClumpCollBSPBranchNode
    {
        public int leftInfo { get; set; }      
        public int rightInfo { get; set; } 
        public float leftValue { get; set; } 
        public float rightValue { get; set; } 
    }

    public struct xClumpCollBSPTriangle
    {
        public short atomIndex { get; set; } 
        public short meshVertIndex { get; set; } 
        public byte flags { get; set; } 
        public byte platData { get; set; } 
        public short matIndex { get; set; } 
    }

    public class BFBB_CollisionData_Section1_00BEEF01 : RWSection
    {
        public string CCOL { get; set; }
        public xClumpCollBSPBranchNode[] branchNodes { get; set; }
        public xClumpCollBSPTriangle[] triangles { get; set; }

        public BFBB_CollisionData_Section1_00BEEF01 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section1;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();
            
            CCOL = new string(binaryReader.ReadChars(4).ToArray());

            if (CCOL == "CCOL")
                DoNotSwitch = true;
            else
                DoNotSwitch = false;

            int numBranchNodes = SwitchToggleable(binaryReader.ReadInt32());
            int numTriangles = SwitchToggleable(binaryReader.ReadInt32());

            branchNodes = new xClumpCollBSPBranchNode[numBranchNodes];
            for (int i = 0; i < numBranchNodes; i++)
            {
                branchNodes[i] = new xClumpCollBSPBranchNode()
                {
                    leftInfo = SwitchToggleable(binaryReader.ReadInt32()),
                    rightInfo = SwitchToggleable(binaryReader.ReadInt32()),
                    leftValue = SwitchToggleable(binaryReader.ReadSingle()),
                    rightValue = SwitchToggleable(binaryReader.ReadSingle())
                };
            }

            triangles = new xClumpCollBSPTriangle[numTriangles];
            for (int i = 0; i < numTriangles; i++)
            {
                triangles[i] = new xClumpCollBSPTriangle()
                {
                    atomIndex = SwitchToggleable(binaryReader.ReadInt16()),
                    meshVertIndex = SwitchToggleable(binaryReader.ReadInt16()),
                    flags = binaryReader.ReadByte(),
                    platData = binaryReader.ReadByte(),
                    matIndex = SwitchToggleable(binaryReader.ReadInt16())
                };
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section1;

            foreach (char c in CCOL)
                listBytes.Add((byte)c);

            if (CCOL == "CCOL")
                DoNotSwitch = true;
            else
                DoNotSwitch = false;

            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes.Length)));
            listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(triangles.Length)));

            for (int i = 0; i < branchNodes.Length; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].leftInfo)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].rightInfo)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].leftValue)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].rightValue)));
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(triangles[i].atomIndex)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(triangles[i].meshVertIndex)));
                listBytes.Add(triangles[i].flags);
                listBytes.Add(triangles[i].platData);
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(triangles[i].matIndex)));
            }
        }
    }
}
