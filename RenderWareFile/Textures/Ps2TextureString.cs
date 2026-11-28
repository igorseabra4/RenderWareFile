using RenderWareFile.Sections;

namespace RenderWareFile.Textures;

internal class Ps2TextureString : String_0002
{
    private readonly byte[] originalData;

    public Ps2TextureString() : base() { }
    public Ps2TextureString(string value) : base(value) { }
    public Ps2TextureString(BinaryReader reader) : base(reader)
    {
        long end = reader.BaseStream.Position;
        reader.BaseStream.Position = end - sectionSize;
        originalData = reader.ReadBytes(sectionSize);
        if (originalData.Length != sectionSize)
            throw new EndOfStreamException("Truncated PS2 texture string section.");
    }

    public Ps2TextureString(string value, Ps2TextureString original) : base(value)
    {
        if (original != null && value == original.stringString)
            originalData = original.originalData;
    }

    public override void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes)
    {
        sectionIdentifier = Section.String;

        if (originalData != null)
        {
            listBytes.AddRange(originalData);
            return;
        }
        foreach (char i in stringString)
            listBytes.Add((byte)i);

        listBytes.Add(0);
        while (listBytes.Count % 4 != 0)
            listBytes.Add(0xDD);
    }
}
