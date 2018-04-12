using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static RenderWareFile.EndianFunctions;

namespace RenderWareFile
{
    public enum NativeDataType
    {
        OpenGL = 2,
        PS2 = 4,
        XBOX = 5,
        GameCube = 6
    }

    public class NativeDataPLG_0510 : RWSection
    {
        public NativeDataType platformType;

        public NativeDataGC nativeDataGC;        

        public NativeDataPLG_0510 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.NativeDataPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            platformType = (NativeDataType)binaryReader.ReadInt32();
            switch (platformType)
            {
                case NativeDataType.GameCube:
                    nativeDataGC = new NativeDataGC(binaryReader);
                    break;
                default:
                    throw new Exception();
            }

            binaryReader.BaseStream.Position += sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.NativeDataPLG;
            listBytes.AddRange(BitConverter.GetBytes((int)platformType));

            switch (platformType)
            {
                case NativeDataType.GameCube:
                    listBytes.AddRange(nativeDataGC.GetBytes());
                    break;
                default:
                    throw new Exception();
            }
        }
    }

    public enum Declarations : byte
    {
        Vertex = 0x09,
        Color = 0x0B, 
        TextCoord = 0x0D
    }

    public class Declaration
    {
        public int startOffset;
        public Declarations declarationType;
        public byte sizeOfEntry;
        public short unknown;

        public List<object> entryList;
    }

    public class TriangleList
    {
        public short setting;
        public byte entryAmount;
        public List<byte[]> entries;
    }

    public class NativeDataGC
    {
        public int headerLenght;
        public int dataLenght;
        public short unknown1;
        public short meshIndex;
        public int unknown2;
        public int declarationAmount;
        public Declaration[] declarations;
        public int unknown3;
        public int triangleSectionLenght;

        public List<TriangleList> TriangleListList;

        public NativeDataGC(BinaryReader binaryReader)
        {
            headerLenght = binaryReader.ReadInt32();
            dataLenght = binaryReader.ReadInt32();
            unknown1 = binaryReader.ReadInt16();
            meshIndex = binaryReader.ReadInt16();
            unknown2 = binaryReader.ReadInt32();
            declarationAmount = binaryReader.ReadInt32();

            declarations = new Declaration[declarationAmount];
            for (int i = 0; i < declarationAmount; i++)
            {
                declarations[i].startOffset = binaryReader.ReadInt32();
                declarations[i].declarationType = (Declarations)binaryReader.ReadByte();
                declarations[i].sizeOfEntry = binaryReader.ReadByte();
                declarations[i].unknown = binaryReader.ReadInt16();
            }

            unknown3 = binaryReader.ReadInt32();
            triangleSectionLenght = binaryReader.ReadInt32();
            long headerEndPosition = binaryReader.BaseStream.Position;

            TriangleListList = new List<TriangleList>();

            while (binaryReader.BaseStream.Position < headerEndPosition + triangleSectionLenght)
            {
                short setting = binaryReader.ReadInt16();
                byte entryAmount = binaryReader.ReadByte();
                List<byte[]> entries = new List<byte[]>();

                for (int i = 0; i < entryAmount; i++)
                    entries.Add(binaryReader.ReadBytes(declarationAmount));

                TriangleListList.Add(new TriangleList() { setting = setting, entryAmount = entryAmount, entries = entries });
            }

            foreach (Declaration d in declarations)
            {
                binaryReader.BaseStream.Position = headerEndPosition + d.startOffset;

                d.entryList = new List<object>();
                if (d.declarationType == Declarations.Vertex)
                {
                    while (true)
                    {
                        Vertex3 v = new Vertex3(Switch(binaryReader.ReadSingle()), Switch(binaryReader.ReadSingle()), Switch(binaryReader.ReadSingle()));
                        if (v.X != 0f & v.Y != 0f & v.Z != 0f)
                            d.entryList.Add(v);
                        else break;
                    }
                }
                else if (d.declarationType == Declarations.Color)
                {
                    while (true)
                    {
                        Color c = new Color(binaryReader.ReadBytes(4));
                        if (c.R != 0 & c.G != 0 & c.B != 0 & c.A != 0)
                            d.entryList.Add(c);
                        else break;
                    }
                }
                else if (d.declarationType == Declarations.TextCoord)
                {
                    while (true)
                    {
                        TextCoord v = new TextCoord(Switch(binaryReader.ReadSingle()), Switch(binaryReader.ReadSingle()));
                        if (v.X != 0f & v.Y != 0f)
                            d.entryList.Add(v);
                        else break;
                    }
                }
            }
        }

        public List<byte> GetBytes()
        {
            throw new Exception();
            // return new List<byte>();
        }
    }
}
