using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public class BFBB_CollisionData_Section3_00BEEF03 : RWSection
    {
        public List<Vertex3> vertexList;
                
        public BFBB_CollisionData_Section3_00BEEF03 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section3;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            int vCount = Switch(binaryReader.ReadInt32());
            vertexList = new List<Vertex3>(vCount);
            for (int i = 0; i < vCount; i++)
                vertexList.Add(new Vertex3(Switch(binaryReader.ReadSingle()), Switch(binaryReader.ReadSingle()), Switch(binaryReader.ReadSingle())));

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.BFBB_CollisionData_Section3;

            listBytes.AddRange(BitConverter.GetBytes(Switch(vertexList.Count)));
            foreach (Vertex3 v in vertexList)
            {
                listBytes.AddRange(BitConverter.GetBytes(Switch(v.X)));
                listBytes.AddRange(BitConverter.GetBytes(Switch(v.Y)));
                listBytes.AddRange(BitConverter.GetBytes(Switch(v.Z)));
            }
        }
    }
}
