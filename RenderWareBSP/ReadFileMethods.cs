using System;
using System.Collections.Generic;
using System.IO;

namespace RenderWareChunk
{
    public static class ReadFileMethods
    {
        public static bool isShadow = false;
        public static bool isCollision = false;

        public static RWSection[] ReadRenderWareFile(string FileName)
        {
            FileStream fileStream = new FileStream(FileName, FileMode.Open);
            return ReadRenderWareFile(fileStream, FileName);
        }

        public static RWSection[] ReadRenderWareFile(byte[] File, string FileName)
        {
            MemoryStream memoryStream = new MemoryStream(File);
            return ReadRenderWareFile(memoryStream, FileName);
        }

        public static RWSection[] ReadRenderWareFile(Stream File, string FileName)
        {
            BinaryReader binaryReader = new BinaryReader(File);
            List<RWSection> renderWareFile = new List<RWSection>();

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                Section currentSection = (Section)binaryReader.ReadInt32();
                if (currentSection == Section.World) renderWareFile.Add(new World_000B().Read(binaryReader));
                else if (currentSection == Section.Clump) renderWareFile.Add(new Clump_0010().Read(binaryReader));
                else if (currentSection == Section.ChunkGroupStart) renderWareFile.Add(new ChunkGroupStart_0029().Read(binaryReader));
                else if (currentSection == Section.ChunkGroupEnd) renderWareFile.Add(new ChunkGroupEnd_002A().Read(binaryReader));
                else throw new Exception(currentSection.ToString());
            }

            return renderWareFile.ToArray();
        }
    }
}
