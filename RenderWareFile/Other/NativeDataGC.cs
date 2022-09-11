using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public enum Declarations : byte
    {
        Vertex = 0x09,
        Normal = 0x0A,
        Color = 0x0B,
        TextCoord = 0x0D
    }

    public enum ByteTypes : byte
    {
        OneByte = 0x02,
        TwoBytes = 0x03
    }

    public abstract class Declaration
    {
        public int startOffset;
        public Declarations declarationType;
        public byte sizeOfEntry;
        public ByteTypes byteType;
        public byte unknown2;
    }

    public class Vertex3Declaration : Declaration
    {
        public List<Vertex3> entryList;
    }

    public class Vertex2Declaration : Declaration
    {
        public List<Vertex2> entryList;
    }

    public class ColorDeclaration : Declaration
    {
        public List<Color> entryList;
    }

    public class TriangleDeclaration
    {
        public int startOffset;
        public int size;
        public List<TriangleList> TriangleListList;

        public int MaterialIndex;
    }

    public class TriangleList
    {
        public byte setting;
        public byte setting2;
        public byte entryAmount;

        public List<int[]> entries;
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
        public TriangleDeclaration[] triangleDeclarations;

        public NativeDataGC(BinaryReader binaryReader, bool fixFlag)
        {
            headerLenght = binaryReader.ReadInt32(); // counting from after dataLenght
            dataLenght = binaryReader.ReadInt32(); // counting from after headerEndPosition

            long nativeDataStart = binaryReader.BaseStream.Position; // from here the file is little endian

            unknown1 = Switch(binaryReader.ReadInt16());
            meshIndex = Switch(binaryReader.ReadInt16());
            unknown2 = Switch(binaryReader.ReadInt32());
            declarationAmount = Switch(binaryReader.ReadInt32());

            declarations = new Declaration[declarationAmount];
            for (int i = 0; i < declarationAmount; i++)
            {
                var startOffset = Switch(binaryReader.ReadInt32());
                var declarationType = (Declarations)binaryReader.ReadByte();
                var sizeOfEntry = binaryReader.ReadByte();
                var byteType = (ByteTypes)binaryReader.ReadByte();
                var unknown2 = binaryReader.ReadByte();

                switch (declarationType)
                {
                    case Declarations.Vertex:
                    case Declarations.Normal:
                        declarations[i] = new Vertex3Declaration()
                        {
                            startOffset = startOffset,
                            declarationType = declarationType,
                            sizeOfEntry = sizeOfEntry,
                            byteType = byteType,
                            unknown2 = unknown2,
                        };
                        break;
                    case Declarations.TextCoord:
                        declarations[i] = new Vertex2Declaration()
                        {
                            startOffset = startOffset,
                            declarationType = declarationType,
                            sizeOfEntry = sizeOfEntry,
                            byteType = byteType,
                            unknown2 = unknown2,
                        };
                        break;
                    case Declarations.Color:
                        declarations[i] = new ColorDeclaration()
                        {
                            startOffset = startOffset,
                            declarationType = declarationType,
                            sizeOfEntry = sizeOfEntry,
                            byteType = byteType,
                            unknown2 = unknown2,
                        };
                        break;
                }
            }

            List<TriangleDeclaration> list = new List<TriangleDeclaration>();

            foreach (int i in MaterialList)
            {
                list.Add(new TriangleDeclaration()
                {
                    startOffset = Switch(binaryReader.ReadInt32()),
                    size = Switch(binaryReader.ReadInt32()),
                    MaterialIndex = i,
                    TriangleListList = new List<TriangleList>()
                });
            }

            triangleDeclarations = list.ToArray();

            long headerEndPosition = binaryReader.BaseStream.Position;

            for (int i = 0; i < triangleDeclarations.Length; i++)
            {
                binaryReader.BaseStream.Position = headerEndPosition + triangleDeclarations[i].startOffset;

                while (!(binaryReader.BaseStream.Position == headerEndPosition + triangleDeclarations[i].startOffset + triangleDeclarations[i].size))
                {
                    byte setting = binaryReader.ReadByte();

                    if (setting == 0) continue;
                    else if (setting != 0x98) throw new Exception();

                    byte setting2 = binaryReader.ReadByte();

                    byte entryAmount = binaryReader.ReadByte();
                    List<int[]> entries = new List<int[]>();

                    for (int j = 0; j < entryAmount; j++)
                    {
                        List<int> objectList = new List<int>();

                        if (fixFlag) binaryReader.BaseStream.Position += 1;

                        for (int k = 0; k < declarationAmount; k++)
                        {
                            if (declarations[k].byteType == ByteTypes.OneByte)
                                objectList.Add(binaryReader.ReadByte());
                            else if (declarations[k].byteType == ByteTypes.TwoBytes)
                                objectList.Add(Switch(binaryReader.ReadInt16()));
                            else throw new Exception();
                        }

                        entries.Add(objectList.ToArray());
                    }

                    triangleDeclarations[i].TriangleListList.Add(new TriangleList() { setting = setting, setting2 = setting2, entryAmount = entryAmount, entries = entries });
                }
            }

            for (int d = 0; d < declarations.Count(); d++)
            {
                binaryReader.BaseStream.Position = headerEndPosition + declarations[d].startOffset;

                byte[] data = d + 1 < declarations.Length ?
                    binaryReader.ReadBytes(declarations[d + 1].startOffset - declarations[d].startOffset) :
                    binaryReader.ReadBytes(dataLenght - declarations[d].startOffset);

                if (declarations[d].declarationType == Declarations.Vertex || declarations[d].declarationType == Declarations.Normal)
                {
                    var dec = (Vertex3Declaration)declarations[d];
                    dec.entryList = new List<Vertex3>();
                    for (int i = 0; i + 11 < data.Count(); i += 12)
                    {
                        Vertex3 v = new Vertex3(
                            BitConverter.ToSingle(new byte[] { data[i + 3], data[i + 2], data[i + 1], data[i] }, 0),
                            BitConverter.ToSingle(new byte[] { data[i + 7], data[i + 6], data[i + 5], data[i + 4] }, 0),
                            BitConverter.ToSingle(new byte[] { data[i + 11], data[i + 10], data[i + 9], data[i + 8] }, 0));
                        dec.entryList.Add(v);
                    }
                }
                else if (declarations[d].declarationType == Declarations.Color)
                {
                    var dec = (ColorDeclaration)declarations[d];
                    dec.entryList = new List<Color>();
                    for (int i = 0; i < data.Count(); i += 0x4)
                    {
                        Color v = new Color(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] });
                        dec.entryList.Add(v);
                    }
                }
                else if (declarations[d].declarationType == Declarations.TextCoord)
                {
                    var dec = (Vertex2Declaration)declarations[d];
                    dec.entryList = new List<Vertex2>();
                    for (int i = 0; i < data.Count(); i += 0x8)
                    {
                        Vertex2 v = new Vertex2(
                            BitConverter.ToSingle(new byte[] { data[i + 3], data[i + 2], data[i + 1], data[i] }, 0),
                            BitConverter.ToSingle(new byte[] { data[i + 7], data[i + 6], data[i + 5], data[i + 4] }, 0));
                        dec.entryList.Add(v);
                    }
                }
            }
        }

        public List<byte> GetBytes()
        {
            List<byte> listData = new List<byte>();

            foreach (TriangleDeclaration td in triangleDeclarations)
            {
                td.startOffset = listData.Count();
                foreach (TriangleList tl in td.TriangleListList)
                {
                    listData.Add(tl.setting);
                    listData.Add(tl.setting2);
                    tl.entryAmount = (byte)tl.entries.Count();
                    listData.Add(tl.entryAmount);

                    for (int i = 0; i < tl.entryAmount; i++)
                    {
                        for (int j = 0; j < tl.entries[i].Length; j++)
                        {
                            if (declarations[j].byteType == ByteTypes.OneByte)
                                listData.Add((byte)tl.entries[i][j]);
                            else if (declarations[j].byteType == ByteTypes.TwoBytes)
                                listData.AddRange(BitConverter.GetBytes((short)tl.entries[i][j]).Reverse());
                        }
                    }
                }

                while (listData.Count() % 0x20 != 0)
                    listData.Add(0);

                td.size = listData.Count() - td.startOffset;
            }

            foreach (var d in declarations)
            {
                d.startOffset = listData.Count();
                if (d.declarationType == Declarations.Vertex || d.declarationType == Declarations.Normal)
                {
                    foreach (var v in ((Vertex3Declaration)d).entryList)
                    {
                        listData.AddRange(BitConverter.GetBytes(v.X).Reverse());
                        listData.AddRange(BitConverter.GetBytes(v.Y).Reverse());
                        listData.AddRange(BitConverter.GetBytes(v.Z).Reverse());
                    }
                    d.sizeOfEntry = 0xC;
                }
                else if (d.declarationType == Declarations.Color)
                {
                    foreach (var c in ((ColorDeclaration)d).entryList)
                    {
                        listData.Add(c.R);
                        listData.Add(c.G);
                        listData.Add(c.B);
                        listData.Add(c.A);
                    }
                    d.sizeOfEntry = 0x4;
                }
                else if (d.declarationType == Declarations.TextCoord)
                {
                    foreach (var tc in ((Vertex2Declaration)d).entryList)
                    {
                        listData.AddRange(BitConverter.GetBytes(tc.X).Reverse());
                        listData.AddRange(BitConverter.GetBytes(tc.Y).Reverse());
                    }
                    d.sizeOfEntry = 0x8;
                }

                while (listData.Count() % 0x20 != 0)
                    listData.Add(0);
            }

            dataLenght = listData.Count();
            declarationAmount = declarations.Count();
            headerLenght = 12 + 8 * declarationAmount + 8 * triangleDeclarations.Length;

            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(headerLenght));
            list.AddRange(BitConverter.GetBytes(dataLenght));

            list.AddRange(BitConverter.GetBytes(unknown1).Reverse());
            list.AddRange(BitConverter.GetBytes(meshIndex).Reverse());
            list.AddRange(BitConverter.GetBytes(unknown2).Reverse());
            list.AddRange(BitConverter.GetBytes(declarationAmount).Reverse());

            for (int i = 0; i < declarationAmount; i++)
            {
                list.AddRange(BitConverter.GetBytes(declarations[i].startOffset).Reverse());
                list.Add((byte)declarations[i].declarationType);
                list.Add(declarations[i].sizeOfEntry);
                list.Add((byte)declarations[i].byteType);
                list.Add(declarations[i].unknown2);
            }

            for (int i = 0; i < triangleDeclarations.Length; i++)
            {
                list.AddRange(BitConverter.GetBytes(triangleDeclarations[i].startOffset).Reverse());
                list.AddRange(BitConverter.GetBytes(triangleDeclarations[i].size).Reverse());
            }

            list.AddRange(listData);
            return list;
        }
    }
}
