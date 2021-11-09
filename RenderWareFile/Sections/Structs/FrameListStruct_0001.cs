using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public struct Frame
    {
        public Matrix3x3 rotationMatrix;
        public Vertex3 position;
        public int parentFrame;
        public int unknown;
    }

    public class FrameListStruct_0001 : RWSection
    {
        public List<Frame> frames;

        public FrameListStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            int frameCount = binaryReader.ReadInt32();
            frames = new List<Frame>();

            for (int i = 0; i < frameCount; i++)
                frames.Add(new Frame()
                {
                    rotationMatrix = new Matrix3x3
                    {
                        M11 = binaryReader.ReadSingle(),
                        M12 = binaryReader.ReadSingle(),
                        M13 = binaryReader.ReadSingle(),
                        M21 = binaryReader.ReadSingle(),
                        M22 = binaryReader.ReadSingle(),
                        M23 = binaryReader.ReadSingle(),
                        M31 = binaryReader.ReadSingle(),
                        M32 = binaryReader.ReadSingle(),
                        M33 = binaryReader.ReadSingle()
                    },
                    position = new Vertex3
                    {
                        X = binaryReader.ReadSingle(),
                        Y = binaryReader.ReadSingle(),
                        Z = binaryReader.ReadSingle()
                    },
                    parentFrame = binaryReader.ReadInt32(),
                    unknown = binaryReader.ReadInt32()
                });

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(frames.Count));
            foreach (Frame i in frames)
            {
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M11));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M12));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M13));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M21));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M22));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M23));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M31));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M32));
                listBytes.AddRange(BitConverter.GetBytes(i.rotationMatrix.M33));
                listBytes.AddRange(BitConverter.GetBytes(i.position.X));
                listBytes.AddRange(BitConverter.GetBytes(i.position.Y));
                listBytes.AddRange(BitConverter.GetBytes(i.position.Z));
                listBytes.AddRange(BitConverter.GetBytes(i.parentFrame));
                listBytes.AddRange(BitConverter.GetBytes(i.unknown));
            }
        }
    }
}