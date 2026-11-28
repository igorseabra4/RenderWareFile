using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{

    public class NativeDataStruct_0001 : RWSection
    {
        public NativeDataType nativeDataType { get; set; }
        public NativeDataGC nativeData { get; set; }
        public NativeDataPS2 nativeDataPs2 { get; set; }

        public byte[] nativeDataData { get; set; }

        public NativeDataStruct_0001(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = (RWVersion)binaryReader.ReadInt32();

            if (ReadFileMethods.treatStuffAsByteArray)
            {
                nativeDataData = binaryReader.ReadBytes(sectionSize);
                return;
            }

            long startSectionPosition = binaryReader.BaseStream.Position;

            nativeDataType = (NativeDataType)binaryReader.ReadInt32();
            switch (nativeDataType)
            {
                case NativeDataType.GameCube:
                    try
                    {
                        nativeData = new NativeDataGC(binaryReader, false);
                    }
                    catch
                    {
                        binaryReader.BaseStream.Position = startSectionPosition + 4;
                        nativeData = new NativeDataGC(binaryReader, true);
                    }
                    break;
                case NativeDataType.PS2:
                    nativeDataPs2 = new NativeDataPS2(binaryReader);
                    break;
                default:
                    throw new Exception();
            }

            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;
        }

        public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            if (ReadFileMethods.treatStuffAsByteArray)
            {
                listBytes.AddRange(nativeDataData);
                return;
            }

            listBytes.AddRange(BitConverter.GetBytes((int)nativeDataType));

            switch (nativeDataType)
            {
                case NativeDataType.GameCube:
                    listBytes.AddRange(nativeData.GetBytes());
                    break;
                case NativeDataType.PS2:
                    listBytes.AddRange(nativeDataPs2.GetBytes());
                    break;
                default:
                    throw new Exception();
            }
        }
    }
}
