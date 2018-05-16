using System;
using System.Collections.Generic;
using System.IO;
using RenderWareFile.Sections;

namespace RenderWareFile
{
    public static class ReadFileMethods
    {
        public static bool isShadow = false;
        public static bool isCollision = false;

        public static RWSection[] ReadRenderWareFile(string fileName)
        {
            return ReadRenderWareFile(new FileStream(fileName, FileMode.Open));
        }

        public static RWSection[] ReadRenderWareFile(byte[] file)
        {
            return ReadRenderWareFile(new MemoryStream(file));
        }

        public static RWSection[] ReadRenderWareFile(Stream File)
        {
            BinaryReader binaryReader = new BinaryReader(File);
            List<RWSection> renderWareFile = new List<RWSection>();

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                Section currentSection = (Section)binaryReader.ReadInt32();
                if (currentSection == Section.World) renderWareFile.Add(new World_000B().Read(binaryReader));
                else if (currentSection == Section.Clump) renderWareFile.Add(new Clump_0010().Read(binaryReader));
                else renderWareFile.Add(new GenericSection().Read(binaryReader, currentSection));
            }

            binaryReader.Close();

            return renderWareFile.ToArray();
        }

        public static byte[] ExportRenderWareFile(RWSection[] RWFile, int version)
        {
            List<byte> list = new List<byte>();
            foreach (RWSection i in RWFile)
                list.AddRange(i.GetBytes(version));

            return list.ToArray();
        }
    }
}
