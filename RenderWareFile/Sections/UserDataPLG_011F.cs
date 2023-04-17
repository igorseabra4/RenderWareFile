using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public struct UserData
    {
        public string attribute;
        public int dataType;
        public List<object> data;
    }
    public class UserDataPLG_011F : RWSection
    {
        public enum DataTypes
        {
            Null,
            Int,
            Float,
            String
        }


        public UserData[] dataList;

        public UserDataPLG_011F Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.UserDataPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            var numData = binaryReader.ReadInt32();

            dataList = new UserData[numData];

            for (int i = 0; i < numData; i++)
            {
                var attributeLength = binaryReader.ReadInt32() - 1;

                dataList[i].attribute = new string(binaryReader.ReadChars(attributeLength));
                binaryReader.BaseStream.Position++;

                dataList[i].dataType = binaryReader.ReadInt32();

                var dataCount = binaryReader.ReadInt32();

                dataList[i].data = new List<object>();

                switch ((DataTypes)dataList[i].dataType)
                {
                    case DataTypes.Int:
                        for (int j = 0; j < dataCount; j++)
                            dataList[i].data.Add(binaryReader.ReadInt32());
                        break;

                    case DataTypes.Float:
                        for (int j = 0; j < dataCount; j++)
                            dataList[i].data.Add(binaryReader.ReadSingle());
                        break;

                    case DataTypes.String:
                        for (int j = 0; j < dataCount; j++)
                        {
                            var dataAttribLength = binaryReader.ReadInt32() - 1;
                            dataList[i].data.Add(binaryReader.ReadChars(dataAttribLength));

                            binaryReader.BaseStream.Position++;
                        }
                        break;

                    default:
                        throw new Exception();
                }
            }

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.UserDataPLG;

            listBytes.AddRange(BitConverter.GetBytes(dataList.Length + 1));

            foreach (var d in dataList)
            {
                listBytes.AddRange(BitConverter.GetBytes(d.attribute.Length + 1));
                for (int i = 0; i <= d.attribute.Length; i++)
                {
                    if (i >= d.attribute.Length)
                        listBytes.Add(0);
                    else
                        listBytes.Add((byte)d.attribute[i]);
                }

                listBytes.AddRange(BitConverter.GetBytes(d.dataType));
                listBytes.AddRange(BitConverter.GetBytes(d.data.Count));

                foreach (var v in d.data)
                {
                    switch ((DataTypes)d.dataType)
                    {
                        case DataTypes.Int:
                            listBytes.AddRange(BitConverter.GetBytes(Convert.ToInt32(v)));
                            break;

                        case DataTypes.Float:
                            listBytes.AddRange(BitConverter.GetBytes(Convert.ToSingle(v)));
                            break;

                        case DataTypes.String:
                            var dataString = Convert.ToString(v);
                            char[] dataChars = dataString.ToCharArray();

                            listBytes.AddRange(BitConverter.GetBytes(dataChars.Length + 1));

                            foreach (char i in dataChars)
                                listBytes.Add((byte)i);
                            listBytes.Add(0);
                            break;

                        default:
                            for (int i = 0; i < 4; i++)
                                listBytes.Add(0);
                            break;
                    }
                }
            }
            return;
        }
    }
}
