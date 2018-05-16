using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareFile.Sections
{
    public enum NativeDataType
    {
        OpenGL = 2,
        PS2 = 4,
        XBOX = 5,
        GameCube = 6
    }

    public class NativeDataStruct_0001 : RWSection
    {
        public NativeDataType nativeDataType;
        public NativeDataGC nativeData;
        
        public NativeDataStruct_0001 Read(BinaryReader binaryReader)
        {
            sectionIdentifier = Section.Struct;
            sectionSize = binaryReader.ReadInt32();
            renderWareVersion = binaryReader.ReadInt32();

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
                default:
                    throw new Exception();
            }
            
            binaryReader.BaseStream.Position = startSectionPosition + sectionSize;

            return this;
        }

        public override void SetListBytes(int fileVersion, ref List<byte> listBytes)
        {
            sectionIdentifier = Section.Struct;

            listBytes.AddRange(BitConverter.GetBytes((int)nativeDataType));

            switch (nativeDataType)
            {
                case NativeDataType.GameCube:
                    listBytes.AddRange(nativeData.GetBytes());
                    break;
                default:
                    throw new Exception();
            }            
        }
    }
}
