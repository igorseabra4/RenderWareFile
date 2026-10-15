using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public class Frame
    {
        public Matrix3x3 rotationMatrix { get; set; }
        public Vertex3 position { get; set; }
        public int parentFrame { get; set; }
        public int unknown { get; set; }

        public Frame(Matrix3x3? rotationMatrix = null, Vertex3? position = null, int parentFrame = -1, int unknown = 0)
        {
            this.rotationMatrix = rotationMatrix ?? Matrix3x3.Identity;
            this.position = position ?? new Vertex3();
            this.parentFrame = parentFrame;
            this.unknown = unknown;
        }
    }

    public class FrameListStruct_0001 : RWSection
    {
        public Frame[] frames { get; set; }

        public FrameListStruct_0001(Frame[] frames) 
        {
            this.frames = frames;
        }
        public FrameListStruct_0001(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            frames = new Frame[binaryReader.ReadInt32()];
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new Frame()
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
                };
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes(frames.Length));
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