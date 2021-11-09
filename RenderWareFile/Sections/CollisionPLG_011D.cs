using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class CollisionPLG_011D : RWSection
    {
        public int unknownValue;
        public RWSection colTree;

        public CollisionPLG_011D Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.CollisionPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            if (ReadFileMethods.isShadow & ReadFileMethods.isCollision)
            {
                unknownValue = binaryReader.ReadInt32();

                Section colTreeSection = (Section)binaryReader.ReadInt32();
                if (colTreeSection == Section.ColTree)
                    colTree = new ColTree_002C().Read(binaryReader);
                else throw new Exception();
            }
            else
            {
                binaryReader.BaseStream.Position += sectionSize;
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.CollisionPLG;

            listBytes.AddRange(BitConverter.GetBytes(unknownValue));
            listBytes.AddRange(colTree.GetBytes(fileVersion));
        }
    }
}
