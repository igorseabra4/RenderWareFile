using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RenderWareFile.Shared;

namespace RenderWareFile.Sections
{
    public enum DataType : byte
    {
        S32 = 0,
        S16 = 1,
        S8 = 2,
        V2_32 = 4,
        V2_16 = 5,
        V2_8 = 6,
        V3_32 = 8,
        V3_16 = 9,
        V3_8 = 10,
        V4_32 = 12,
        V4_16 = 13,
        V4_8 = 14,
        V4_5 = 15,
    }

    public enum VIFInstruction : byte
    {
        NOP = 0x00,
        STCYCL,
        OFFSET,
        BASE,
        ITOP,
        STMOD,
        MSKPATH3,
        MARK,
        FLUSHE = 0x10,
        FLUSH,
        FLUSHA = 0x13,
        MSCAL,
        MSCNT = 0x17,
        MSCALF = 0x15,
        STMASK = 0x20,
        STROW = 0x30,
        STCOL = 0x31,
        MPG = 0x4A,
        DIRECT = 0x50,
        DIRECTHL = 0x51,
        UNPACK = 0x60
    }

    public class VIFCode
    {
        public ushort Immediate { get; set; }
        public byte Num { get; set; }
        public VIFInstruction VIFInstruct { get; set; }
        public DataType UnpackDataType { get; set; }

        public VIFCode(int value)
        {
            Immediate = (ushort)(value >> 16);
            Num = (byte)((value >> 8) & 0xff);

            if ((value & 0x60) != 0)
            {
                VIFInstruct = VIFInstruction.UNPACK;
                UnpackDataType = (DataType)(value & 0xf);
            }
            else
                VIFInstruct = (VIFInstruction)(value & 0xff);
        }

        public VIFCode(BinaryReader reader) : this(Switch(reader.ReadInt32())) { }

        public VIFCode() { }

        public DataType? GetUnpackDataType()
        {
            if (VIFInstruct == VIFInstruction.UNPACK)
                return UnpackDataType;
            return null;
        }

        public int? GetNumClusters()
        {
            if (VIFInstruct == VIFInstruction.STCYCL)
                return Immediate >> 8;
            return null;
        }

        public override string ToString()
        {
            return VIFInstruct.ToString();
        }

        public void GetBytes(ref List<byte> bytes)
        {
            bytes.AddRange(BitConverter.GetBytes(Immediate).Reverse());
            bytes.Add(Num);

            if (VIFInstruct == VIFInstruction.UNPACK)
                bytes.Add((byte)((byte)VIFInstruction.UNPACK | (byte)UnpackDataType));
            else
                bytes.Add((byte)VIFInstruct);
        }
    }

    public class sceDmaTag
    {
        public ushort QWC { get; set; }
        public byte Mark { get; set; }
        public byte Id { get; set; }
        public int Next { get; set; }
        public VIFCode P { get; set; }
        public VIFCode P2 { get; set; }
        public int Offset => QWC * 0x10;

        public sceDmaTag(BinaryReader reader)
        {
            QWC = reader.ReadUInt16();
            Mark = reader.ReadByte();
            Id = reader.ReadByte();
            Next = reader.ReadInt32();
            P = new VIFCode(reader);
            P2 = new VIFCode(reader);
        }
    }

    public class Cluster
    {
        public VIFCode VIFCode1 { get; set; }
        public VIFCode VIFCode2 { get; set; }
        public VIFCode VIFCode3 { get; set; }
        public VIFCode VIFCode4 { get; set; }

        public Cluster(BinaryReader reader)
        {
            VIFCode1 = new VIFCode(reader);
            VIFCode2 = new VIFCode(reader);
            VIFCode3 = new VIFCode(reader);
            VIFCode4 = new VIFCode(reader);
        }

        public Cluster<T> CreateDataCluster<T>(List<T> entries)
        {
            return new Cluster<T>(this, entries);
        }

        public Cluster(Cluster cluster)
        {
            VIFCode1 = cluster.VIFCode1;
            VIFCode2 = cluster.VIFCode2;
            VIFCode3 = cluster.VIFCode3;
            VIFCode4 = cluster.VIFCode4;
        }

        public Cluster() { }
    }

    public class Cluster<T> : Cluster
    {
        public List<T> entryList;

        public Cluster(Cluster cluster, List<T> entries) : base(cluster)
        {
            entryList = entries;
        }
    }

    public struct Batch
    {
        public List<Cluster> Clusters;
        public Cluster EndCluster;
    }

    public class NativeDataPS2
    {
        public bool NoPointers;
        public sceDmaTag DMATag;
        public List<Batch> Batches = new List<Batch>();

        public byte[] UnkBytes;

        public NativeDataPS2(BinaryReader reader)
        {
            int Length = reader.ReadInt32();
            NoPointers = Convert.ToBoolean(reader.ReadInt32());
            long startNativeData = reader.BaseStream.Position;

            if (NoPointers == false)
            {
                throw new NotImplementedException("Pointer DMA Tags not implemented yet");
            }
            else
            {
                DMATag = new sceDmaTag(reader);
                while (reader.BaseStream.Position < (startNativeData + DMATag.Offset))
                {
                    Cluster startCluster = new Cluster(reader);
                    reader.BaseStream.Position -= 16;

                    Batch batch = new Batch()
                    {
                        Clusters = new List<Cluster>()
                    };

                    for (int i = 0; i < startCluster.VIFCode3.GetNumClusters(); i++)
                    {
                        VIFCode vifcode1 = new VIFCode(reader);
                        VIFCode vifcode2 = new VIFCode(reader);
                        VIFCode vifcode3 = new VIFCode(reader);
                        VIFCode vifcode4 = new VIFCode(reader);

                        DataType? dataType = vifcode4.GetUnpackDataType();
                        if (dataType == null)
                            throw new Exception("Unknown unpack data type");

                        int dataSize = GetDataTypeSize((DataType)dataType) * vifcode4.Num;
                        long startData = reader.BaseStream.Position;

                        Cluster cluster = new Cluster()
                        {
                            VIFCode1 = vifcode1,
                            VIFCode2 = vifcode2,
                            VIFCode3 = vifcode3,
                            VIFCode4 = vifcode4,
                        };

                        cluster = ReadGeometry(reader, cluster);

                        reader.BaseStream.Position = (startData + dataSize);

                        reader.BaseStream.Position += (16 - (dataSize % 16)) % 16;
                        batch.Clusters.Add(cluster);
                    }
                    batch.EndCluster = new Cluster(reader);
                    Batches.Add(batch);
                }
            }
            UnkBytes = reader.ReadBytes((int)((startNativeData + Length) - reader.BaseStream.Position));
        }

        private Cluster ReadGeometry(BinaryReader reader, Cluster cluster)
        {
            VIFCode vif = cluster.VIFCode4;

            switch (vif.GetUnpackDataType())
            {
                case DataType.V3_32:
                    return cluster.CreateDataCluster(Enumerable.Range(0, vif.Num).Select(_ => new Vertex3(reader)).ToList());

                case DataType.V2_32:
                    return cluster.CreateDataCluster(Enumerable.Range(0, vif.Num).Select(_ => new Vertex2(reader)).ToList());

                case DataType.V4_8:
                    return cluster.CreateDataCluster(Enumerable.Range(0, vif.Num).Select(_ => new Color(reader.ReadInt32())).ToList());

                case DataType.V3_8:
                    return cluster.CreateDataCluster(Enumerable.Range(0, vif.Num)
                        .Select(_ => new Vertex3(reader.ReadSByte() / 128f, reader.ReadSByte() / 128f, reader.ReadSByte() / 128f)).ToList());

                case DataType.V4_32:
                    return cluster.CreateDataCluster(Enumerable.Range(0, vif.Num).Select(_ => new Vertex4(reader)).ToList());

                default:
                    throw new Exception("Unsupported data type");
            }
        }

        public List<Vertex3> GetLinearVerticesList()
        {
            return Batches.SelectMany(batch => batch.Clusters.Where(c => c is Cluster<Vertex3> clustVert && clustVert.VIFCode4.GetUnpackDataType() == DataType.V3_32).Cast<Cluster<Vertex3>>()).SelectMany(cv => cv.entryList).ToList();
        }

        public List<Vertex2> GetLinearTexCoordsList()
        {
            return Batches.SelectMany(batch => batch.Clusters.Where(c => c is Cluster<Vertex2>).Cast<Cluster<Vertex2>>()).SelectMany(cv => cv.entryList).ToList();
        }

        public List<Color> GetLinearColorList()
        {
            return Batches.SelectMany(batch => batch.Clusters.Where(c => c is Cluster<Color> clustVert).Cast<Cluster<Color>>()).SelectMany(cv => cv.entryList).ToList();
        }

        public List<Vertex3> GetLinearNormalsList()
        {
            return Batches.SelectMany(batch => batch.Clusters.Where(c => c is Cluster<Vertex3> clustVert && clustVert.VIFCode4.GetUnpackDataType() == DataType.V3_8).Cast<Cluster<Vertex3>>()).SelectMany(cv => cv.entryList).ToList();
        }

        public List<Vertex4> GetLinearVerticesFlagList()
        {
            return Batches.SelectMany(batch => batch.Clusters.Where(c => c is Cluster<Vertex4> clustVert).Cast<Cluster<Vertex4>>()).SelectMany(cv => cv.entryList).ToList();
        }

        private static int GetDataTypeSize(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.S8:
                    return 1;
                case DataType.V2_8:
                case DataType.S16:
                    return 2;
                case DataType.V3_8:
                    return 3;
                case DataType.V2_16:
                case DataType.V4_8:
                case DataType.S32:
                    return 4;
                case DataType.V3_16:
                    return 6;
                case DataType.V2_32:
                case DataType.V4_16:
                    return 8;
                case DataType.V3_32:
                    return 12;
                case DataType.V4_32:
                    return 16;
                default:
                    return 0;
            }
        }

        public List<byte> GetBytes()
        {
            List<byte> listData = new List<byte>();

            listData.AddRange(BitConverter.GetBytes(Convert.ToInt32(NoPointers)));
            listData.AddRange(BitConverter.GetBytes(DMATag.QWC));
            listData.Add(DMATag.Mark);
            listData.Add(DMATag.Id);
            listData.AddRange(BitConverter.GetBytes(DMATag.Next));
            DMATag.P.GetBytes(ref listData);
            DMATag.P2.GetBytes(ref listData);

            foreach (Batch b in Batches)
            {
                foreach (var cluster in b.Clusters)
                {
                    cluster.VIFCode1.GetBytes(ref listData);
                    cluster.VIFCode2.GetBytes(ref listData);
                    cluster.VIFCode3.GetBytes(ref listData);
                    cluster.VIFCode4.GetBytes(ref listData);

                    int startData = listData.Count;

                    if (cluster is Cluster<Vertex3> clusterVert && cluster.VIFCode4.GetUnpackDataType() == DataType.V3_32)
                        foreach (Vertex3 vec in clusterVert.entryList)
                        {
                            listData.AddRange(BitConverter.GetBytes(vec.X));
                            listData.AddRange(BitConverter.GetBytes(vec.Y));
                            listData.AddRange(BitConverter.GetBytes(vec.Z));
                        }
                    else if (cluster is Cluster<Vertex2> clusterTex)
                        foreach (Vertex2 vec in clusterTex.entryList)
                        {
                            listData.AddRange(BitConverter.GetBytes(vec.X));
                            listData.AddRange(BitConverter.GetBytes(vec.Y));
                        }
                    else if (cluster is Cluster<Vertex3> clusterNorm && cluster.VIFCode4.GetUnpackDataType() == DataType.V3_8)
                        foreach (Vertex3 vec in clusterNorm.entryList)
                        {
                            listData.Add((byte)(vec.X * 128));
                            listData.Add((byte)(vec.Y * 128));
                            listData.Add((byte)(vec.Z * 128));
                        }
                    else if (cluster is Cluster<Color> clusterColor)
                        foreach (Color c in clusterColor.entryList)
                        {
                            listData.Add(c.R);
                            listData.Add(c.G);
                            listData.Add(c.B);
                            listData.Add(c.A);
                        }
                    else if (cluster is Cluster<Vertex4> ClusterVertFlag)
                        foreach (Vertex4 vec in ClusterVertFlag.entryList)
                        {
                            listData.AddRange(BitConverter.GetBytes(vec.X));
                            listData.AddRange(BitConverter.GetBytes(vec.Y));
                            listData.AddRange(BitConverter.GetBytes(vec.Z));
                            listData.AddRange(BitConverter.GetBytes(vec.W));
                        }
                    listData.AddRange(new byte[(16 - ((listData.Count-startData)%16)) % 16]);
                }
                b.EndCluster.VIFCode1.GetBytes(ref listData);
                b.EndCluster.VIFCode2.GetBytes(ref listData);
                b.EndCluster.VIFCode3.GetBytes(ref listData);
                b.EndCluster.VIFCode4.GetBytes(ref listData);
            }

            listData.AddRange(UnkBytes);
            listData.InsertRange(0, BitConverter.GetBytes(listData.Count-4));
            return listData;
        }
    }
}
