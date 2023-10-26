using RenderWareFile.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public struct UserData
    {
        public string attribute;
        public List<object> data;
    }

    public class UserDataPLG_011F : RWSection
    {
        public UserDataType userDataType;

        public UserData[] dataList;

        public UserDataPLG_011F Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.UserDataPLG;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

            dataList = new UserData[binaryReader.ReadInt32()];

            for (int i = 0; i < dataList.Length; i++)
            {
                int attribLength = binaryReader.ReadInt32();

                dataList[i].attribute = new string(binaryReader.ReadChars(attribLength - 1));
                binaryReader.BaseStream.Position++;

                UserDataType dataType = (UserDataType)binaryReader.ReadInt32();
                int dataCount = binaryReader.ReadInt32();
                
                dataList[i].data = new List<object>();

                switch (dataType)
                {
                    case UserDataType.Int:
                        for (int j = 0; j < dataCount; j++)
                            dataList[i].data.Add(binaryReader.ReadInt32());
                        break;

                    case UserDataType.Float:
                        for (int j = 0; j < dataCount; j++)
                            dataList[i].data.Add(binaryReader.ReadSingle());
                        break;

                    case UserDataType.String:
                        for (int j = 0; j < dataCount; j++)
                        {
                            int dataAttribLength = binaryReader.ReadInt32();
                            dataList[i].data.Add(binaryReader.ReadChars(dataAttribLength - 1));

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
                int attribLength = d.attribute.Length;

                listBytes.AddRange(BitConverter.GetBytes(attribLength + 1));

                for (int i = 0; i < attribLength; i++)
                {
                    char[] attribute = d.attribute.ToCharArray();
                    
                    foreach (char j in attribute)
                        listBytes.Add((byte)j);
                    listBytes.Add(0);
                }

                int dataType = Convert.ToInt32(d.data.GetType());
                listBytes.AddRange(BitConverter.GetBytes(dataType));

                if ((UserDataType)dataType == UserDataType.Null)
                    for (int i = 0; i < 4; i++)
                        listBytes.Add(0);

                else
                    foreach (var v in d.data)
                    {
                        switch ((UserDataType)dataType)
                        {
                            case UserDataType.Float:
                                listBytes.AddRange(BitConverter.GetBytes(Convert.ToSingle(v)));
                                break;

                            case UserDataType.String:
                                string dataAttrib = Convert.ToString(v);
                                listBytes.AddRange(BitConverter.GetBytes(dataAttrib.Length + 1));

                                char[] dataAttribChars = dataAttrib.ToCharArray();

                                foreach (char i in dataAttribChars)
                                    listBytes.Add((byte)i);

                                listBytes.Add(0);
                                break;

                            default:
                                listBytes.AddRange(BitConverter.GetBytes(Convert.ToInt32(v)));
                                break;
                        }
                    }
            }
        }
    }
}
