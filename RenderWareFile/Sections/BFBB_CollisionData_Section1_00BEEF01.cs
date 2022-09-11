using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public enum ClumpCollType
    {
        Null = 0,
        Leaf = 1,
        Branch = 2
    }

    public enum ClumpDirection
    {
        X = 0,
        Y = 1,
        Z = 2,
        Unknown = 3
    }

    public struct xClumpCollBSPBranchNode
    {
        private static (int, ClumpCollType, ClumpDirection, int) UnpackInfo(int info) =>
            (info >> 12,
            (ClumpCollType)(info & 0b11),
            (ClumpDirection)((info & 0b1100) >> 2),
            (info & 0b11110000) >> 4);

        private static int PackInfo(int index, ClumpCollType type, ClumpDirection unk1, int unk2) =>
            (index << 12) |
            (((int)type) & 0b11) |
            ((((int)unk1) & 0b11) << 2) |
            ((unk2 & 0b1111) << 4);

        public int LeftListIndex { get; set; }
        public ClumpCollType LeftType { get; set; }
        public ClumpDirection LeftDirection { get; set; }
        public int LeftUnk { get; set; }

        [Browsable(false)]
        public int LeftInfo
        {
            get => PackInfo(LeftListIndex, LeftType, LeftDirection, LeftUnk);
            set
            {
                var info = UnpackInfo(value);
                LeftListIndex = info.Item1;
                LeftType = info.Item2;
                LeftDirection = info.Item3;
                LeftUnk = info.Item4;
            }
        }

        public int RightListIndex { get; set; }
        public ClumpCollType RightType { get; set; }
        public ClumpDirection RightDirection { get; set; }
        public int RightUnk { get; set; }

        [Browsable(false)]
        public int RightInfo
        {
            get => PackInfo(RightListIndex, RightType, RightDirection, RightUnk);
            set
            {
                var info = UnpackInfo(value);
                RightListIndex = info.Item1;
                RightType = info.Item2;
                RightDirection = info.Item3;
                RightUnk = info.Item4;
            }
        }

        public float LeftValue { get; set; }
        public float RightValue { get; set; }
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
                    LeftInfo = SwitchToggleable(binaryReader.ReadInt32()),
                    RightInfo = SwitchToggleable(binaryReader.ReadInt32()),
                    LeftValue = SwitchToggleable(binaryReader.ReadSingle()),
                    RightValue = SwitchToggleable(binaryReader.ReadSingle())
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
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].LeftInfo)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].RightInfo)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].LeftValue)));
                listBytes.AddRange(BitConverter.GetBytes(SwitchToggleable(branchNodes[i].RightValue)));
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
