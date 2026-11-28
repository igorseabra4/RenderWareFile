namespace RenderWareFile.Sections
{
    public struct MipMapEntry
    {
        public int dataSize { get; set; }
        public byte[] data { get; set; }

        public MipMapEntry(int dataSize, byte[] data)
        {
            this.dataSize = dataSize;
            this.data = data;
        }
    }
}
