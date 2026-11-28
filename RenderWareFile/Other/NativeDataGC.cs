using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public enum rwGCNVertexAttribute : byte
    {
        rwGCNVA_PNMTXIDX = 0x0,
        rwGCNVA_TEX0MTXIDX = 0x1,
        rwGCNVA_TEX1MTXIDX = 0x2,
        rwGCNVA_TEX2MTXIDX = 0x3,
        rwGCNVA_TEX3MTXIDX = 0x4,
        rwGCNVA_TEX4MTXIDX = 0x5,
        rwGCNVA_TEX5MTXIDX = 0x6,
        rwGCNVA_TEX6MTXIDX = 0x7,
        rwGCNVA_TEX7MTXIDX = 0x8,
        rwGCNVA_POS = 0x09,
        rwGCNVA_NRM = 0x0A,
        rwGCNVA_CLR0 = 0x0B,
        rwGCNVA_CLR1 = 0x0C,
        rwGCNVA_TEX0 = 0x0D,
        rwGCNVA_TEX1 = 0x0E,
        rwGCNVA_TEX2 = 0xF,
        rwGCNVA_TEX3 = 0x10,
        rwGCNVA_TEX4 = 0x11,
        rwGCNVA_TEX5 = 0x12,
        rwGCNVA_TEX6 = 0x13,
        rwGCNVA_TEX7 = 0x14,
        rwGCNVA_NBT = 0x19,
        rwGCNVA_MAX_ATTR = 0x1A,
        rwGCNVA_NULL = 0xFF,
    }

    public enum rwGCNAttrType : byte
    {
        NONE = 0x0,
        DIRECT = 0x01,
        INDEX8 = 0x02,
        INDEX16 = 0x03
    }

    public enum GXPrimitive : byte
    {
        GX_POINTS = 0xB8,
        GX_LINES = 0xA8,
        GX_LINESTRIP = 0xB0,
        GX_TRIANGLES = 0x90,
        GX_TRIANGLESTRIP = 0x98,
        GX_TRIANGLEFAN = 0xA0,
        GX_QUADS = 0x80
    }

    public abstract class VertexAttribute
    {
        public int startOffset { get; set; }
        public rwGCNVertexAttribute attr { get; set; }
        public byte stride { get; set; }
        public rwGCNAttrType indexType { get; set; }
        public byte pad { get; set; }
    }

    public class VertexAttribute<T> : VertexAttribute
    {
        public List<T> entries { get; set; }

        public VertexAttribute(int startOffset, rwGCNVertexAttribute attr, byte stride, rwGCNAttrType indexType, byte pad = 0)
        {
            this.startOffset = startOffset;
            this.attr = attr;
            this.stride = stride;
            this.indexType = indexType;
            this.pad = pad;
        }

        public VertexAttribute(T[] entries, rwGCNVertexAttribute attr)
        {
            this.entries = entries.ToList();
            this.attr = attr;

            switch (attr)
            {
                case rwGCNVertexAttribute.rwGCNVA_POS:
                case rwGCNVertexAttribute.rwGCNVA_NRM:
                    this.stride = 0xC;
                    break;
                case rwGCNVertexAttribute.rwGCNVA_CLR0:
                case rwGCNVertexAttribute.rwGCNVA_CLR1:
                    this.stride = 0x4;
                    break;
                case rwGCNVertexAttribute.rwGCNVA_TEX0:
                case rwGCNVertexAttribute.rwGCNVA_TEX1:
                    this.stride = 0x8;
                    break;
            }
            this.indexType = entries.Length <= 255 ? rwGCNAttrType.INDEX8 : rwGCNAttrType.INDEX16;
        }
    }

    public class TriangleDeclaration
    {
        public int startOffset { get; set; }
        public int size { get; set; }
        public List<TriangleStrip> strips { get; set; }
        public int MaterialIndex { get; set; }
    }

    public class TriangleStrip
    {
        public GXPrimitive primitive { get; set; }
        public short numIndices { get; set; }
        public List<ushort[]> indices { get; set; }
    }

    public class NativeDataGC
    {
        public short token { get; set; }
        public short serialNumber { get; set; }
        public int flags { get; set; }
        public VertexAttribute[] attr { get; set; }
        public TriangleDeclaration[] displayList { get; set; }

        public NativeDataGC() { }

        public NativeDataGC(BinaryReader binaryReader, bool fixFlag)
        {
            int headerLength = binaryReader.ReadInt32();
            int dataLength = binaryReader.ReadInt32();

            long nativeDataStart = binaryReader.BaseStream.Position; // from here the file is little endian

            token = Switch(binaryReader.ReadInt16());
            serialNumber = Switch(binaryReader.ReadInt16());
            flags = Switch(binaryReader.ReadInt32());
            var numAttrArrays = Switch(binaryReader.ReadInt32());

            attr = new VertexAttribute[numAttrArrays];
            for (int i = 0; i < numAttrArrays; i++)
            {
                var startOffset = Switch(binaryReader.ReadInt32());
                var attr = (rwGCNVertexAttribute)binaryReader.ReadByte();
                var stride = binaryReader.ReadByte();
                var indexType = (rwGCNAttrType)binaryReader.ReadByte();
                var pad = binaryReader.ReadByte();

                switch (attr)
                {
                    case rwGCNVertexAttribute.rwGCNVA_POS:
                    case rwGCNVertexAttribute.rwGCNVA_NRM:
                        this.attr[i] = new VertexAttribute<Vertex3>(startOffset, attr, stride, indexType, pad);
                        break;
                    case rwGCNVertexAttribute.rwGCNVA_TEX0:
                    case rwGCNVertexAttribute.rwGCNVA_TEX1:
                        this.attr[i] = new VertexAttribute<Vertex2>(startOffset, attr, stride, indexType, pad);
                        break;
                    case rwGCNVertexAttribute.rwGCNVA_CLR0:
                        this.attr[i] = new VertexAttribute<Color>(startOffset, attr, stride, indexType, pad);
                        break;
                    default:
                        throw new NotSupportedException($"Unknown declaration type: {attr} at {binaryReader.BaseStream.Position}");
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
                    strips = new List<TriangleStrip>()
                });
            }

            displayList = list.ToArray();

            long headerEndPosition = binaryReader.BaseStream.Position;

            for (int i = 0; i < displayList.Length; i++)
            {
                binaryReader.BaseStream.Position = headerEndPosition + displayList[i].startOffset;

                while (!(binaryReader.BaseStream.Position == headerEndPosition + displayList[i].startOffset + displayList[i].size))
                {
                    GXPrimitive prim = (GXPrimitive)binaryReader.ReadByte();

                    if (prim == 0) continue;
                    else if (prim != GXPrimitive.GX_TRIANGLESTRIP) throw new Exception();

                    short numIndices = Switch(binaryReader.ReadInt16());
                    List<ushort[]> indices = new List<ushort[]>();

                    for (int j = 0; j < numIndices; j++)
                    {
                        List<ushort> vertexAttribute = new List<ushort>();

                        if (fixFlag) binaryReader.BaseStream.Position += 1;

                        for (int k = 0; k < numAttrArrays; k++)
                        {
                            switch (attr[k].indexType)
                            {
                                case rwGCNAttrType.INDEX8:
                                    vertexAttribute.Add(binaryReader.ReadByte());
                                    break;
                                case rwGCNAttrType.INDEX16:
                                    vertexAttribute.Add(Switch(binaryReader.ReadUInt16()));
                                    break;
                                default:
                                    throw new Exception("Unknown index type");
                            }
                        }
                        indices.Add(vertexAttribute.ToArray());
                    }

                    displayList[i].strips.Add(new TriangleStrip()
                    { 
                        primitive = prim,
                        numIndices = numIndices, 
                        indices = indices
                    });
                }
            }

            for (int d = 0; d < attr.Count(); d++)
            {
                binaryReader.BaseStream.Position = headerEndPosition + attr[d].startOffset;

                byte[] data = d + 1 < attr.Length ?
                    binaryReader.ReadBytes(attr[d + 1].startOffset - attr[d].startOffset) :
                    binaryReader.ReadBytes(dataLength - attr[d].startOffset);

                switch (attr[d].attr)
                {
                    case rwGCNVertexAttribute.rwGCNVA_POS:
                    case rwGCNVertexAttribute.rwGCNVA_NRM:
                        var ver3 = (VertexAttribute<Vertex3>)attr[d];
                        ver3.entries = new List<Vertex3>();
                        for (int i = 0; i + 11 < data.Count(); i += 12)
                        {
                            Vertex3 v = new Vertex3(
                                BitConverter.ToSingle(new byte[] { data[i + 3], data[i + 2], data[i + 1], data[i] }, 0),
                                BitConverter.ToSingle(new byte[] { data[i + 7], data[i + 6], data[i + 5], data[i + 4] }, 0),
                                BitConverter.ToSingle(new byte[] { data[i + 11], data[i + 10], data[i + 9], data[i + 8] }, 0));
                            ver3.entries.Add(v);
                        }
                        break;
                    case rwGCNVertexAttribute.rwGCNVA_CLR0:
                    case rwGCNVertexAttribute.rwGCNVA_CLR1:
                        var clr = (VertexAttribute<Color>)attr[d];
                        clr.entries = new List<Color>();
                        for (int i = 0; i < data.Count(); i += 0x4)
                        {
                            Color v = new Color(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] });
                            clr.entries.Add(v);
                        }
                        break;
                    case rwGCNVertexAttribute.rwGCNVA_TEX0:
                    case rwGCNVertexAttribute.rwGCNVA_TEX1:
                        var ver2 = (VertexAttribute<Vertex2>)attr[d];
                        ver2.entries = new List<Vertex2>();
                        for (int i = 0; i < data.Count(); i += 0x8)
                        {
                            Vertex2 v = new Vertex2(
                                BitConverter.ToSingle(new byte[] { data[i + 3], data[i + 2], data[i + 1], data[i] }, 0),
                                BitConverter.ToSingle(new byte[] { data[i + 7], data[i + 6], data[i + 5], data[i + 4] }, 0));
                            ver2.entries.Add(v);
                        }
                        break;
                }
            }
        }

        public List<byte> GetBytes()
        {
            List<byte> listData = new List<byte>();

            foreach (TriangleDeclaration td in displayList)
            {
                td.startOffset = listData.Count();
                foreach (TriangleStrip tl in td.strips)
                {
                    listData.Add((byte)tl.primitive);
                    tl.numIndices = (short)tl.indices.Count();
                    listData.AddRange(BitConverter.GetBytes(tl.numIndices).Reverse());

                    for (int i = 0; i < tl.numIndices; i++)
                    {
                        for (int j = 0; j < tl.indices[i].Length; j++)
                        {
                            if (attr[j].indexType == rwGCNAttrType.INDEX8)
                                listData.Add((byte)tl.indices[i][j]);
                            else if (attr[j].indexType == rwGCNAttrType.INDEX16)
                                listData.AddRange(BitConverter.GetBytes((short)tl.indices[i][j]).Reverse());
                        }
                    }
                }

                while (listData.Count() % 0x20 != 0)
                    listData.Add(0);

                td.size = listData.Count() - td.startOffset;
            }

            foreach (var d in attr)
            {
                d.startOffset = listData.Count();
                switch (d.attr)
                {
                    case rwGCNVertexAttribute.rwGCNVA_POS:
                    case rwGCNVertexAttribute.rwGCNVA_NRM:
                        foreach (var v in ((VertexAttribute<Vertex3>)d).entries)
                        {
                            listData.AddRange(BitConverter.GetBytes(v.X).Reverse());
                            listData.AddRange(BitConverter.GetBytes(v.Y).Reverse());
                            listData.AddRange(BitConverter.GetBytes(v.Z).Reverse());
                        }
                        d.stride = 0xC;
                        break;
                    case rwGCNVertexAttribute.rwGCNVA_CLR0:
                    case rwGCNVertexAttribute.rwGCNVA_CLR1:
                        foreach (var c in ((VertexAttribute<Color>)d).entries)
                        {
                            listData.Add(c.R);
                            listData.Add(c.G);
                            listData.Add(c.B);
                            listData.Add(c.A);
                        }
                        d.stride = 0x4;
                        break;
                    case rwGCNVertexAttribute.rwGCNVA_TEX0:
                    case rwGCNVertexAttribute.rwGCNVA_TEX1:
                        foreach (var tc in ((VertexAttribute<Vertex2>)d).entries)
                        {
                            listData.AddRange(BitConverter.GetBytes(tc.X).Reverse());
                            listData.AddRange(BitConverter.GetBytes(tc.Y).Reverse());
                        }
                        d.stride = 0x8;
                        break;
                }

                while (listData.Count() % 0x20 != 0)
                    listData.Add(0);
            }

            int dataLenght = listData.Count();
            int headerLenght = 12 + 8 * attr.Length + 8 * displayList.Length;

            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(headerLenght));
            list.AddRange(BitConverter.GetBytes(dataLenght));

            list.AddRange(BitConverter.GetBytes(token).Reverse());
            list.AddRange(BitConverter.GetBytes(serialNumber).Reverse());
            list.AddRange(BitConverter.GetBytes(flags).Reverse());
            list.AddRange(BitConverter.GetBytes(attr.Length).Reverse());

            for (int i = 0; i < attr.Length; i++)
            {
                list.AddRange(BitConverter.GetBytes(attr[i].startOffset).Reverse());
                list.Add((byte)attr[i].attr);
                list.Add(attr[i].stride);
                list.Add((byte)attr[i].indexType);
                list.Add(attr[i].pad);
            }

            for (int i = 0; i < displayList.Length; i++)
            {
                list.AddRange(BitConverter.GetBytes(displayList[i].startOffset).Reverse());
                list.AddRange(BitConverter.GetBytes(displayList[i].size).Reverse());
            }

            list.AddRange(listData);
            return list;
        }

        public List<Vertex3> GetLinearVertices()
        {
            return attr.OfType<VertexAttribute<Vertex3>>().Where(a => a.attr == rwGCNVertexAttribute.rwGCNVA_POS).FirstOrDefault()?.entries;
        }

        public List<Vertex3> GetLinearNormals()
        {
            return attr.OfType<VertexAttribute<Vertex3>>().Where(a => a.attr == rwGCNVertexAttribute.rwGCNVA_NRM).FirstOrDefault()?.entries;
        }

        public List<Vertex2> GetLinearTexCoords()
        {
            return attr.OfType<VertexAttribute<Vertex2>>().Where(a => a.attr == rwGCNVertexAttribute.rwGCNVA_TEX0).FirstOrDefault()?.entries;
        }

        public List<Color> GetLinearVertexColors()
        {
            return attr.OfType<VertexAttribute<Color>>().Where(a => a.attr == rwGCNVertexAttribute.rwGCNVA_CLR0).FirstOrDefault()?.entries;
        }
    }
}
