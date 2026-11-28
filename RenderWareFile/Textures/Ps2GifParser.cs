using System.Buffers.Binary;

namespace RenderWareFile.Textures;

/// <summary>One image upload (BITBLTBUF/TRXPOS/TRXREG/TRXDIR + IMAGE data) found in a PS2 GIF packet chain.</summary>
public class Ps2GifUpload
{
    public ulong BitBltBuf;
    public bool HasBitBltBuf;
    public ulong TrxPos;
    public int Width;
    public int Height;
    public byte[] Data;
    internal ReadOnlyMemory<byte> DataView;

    /// <summary>Destination pixel storage mode (DPSM) of the upload.</summary>
    public int DestinationFormat => (int)(BitBltBuf >> 56 & 0x3F);
    /// <summary>Destination buffer width in pixels (DBW * 64).</summary>
    public int DestinationBufferWidth => (int)(BitBltBuf >> 48 & 0x3F) * 64;
    public int DestinationBasePointer => (int)(BitBltBuf >> 32 & 0x3FFF);
}

/// <summary>
/// Parses chains of PS2 GIF packets (PACKED A+D register writes followed by IMAGE data) as stored
/// when a RenderWare PS2 raster has texel headers.
/// </summary>
public static class Ps2GifParser
{
    private const int RegBitBltBuf = 0x50, RegTrxPos = 0x51, RegTrxReg = 0x52, RegTrxDir = 0x53;

    public static List<Ps2GifUpload> Parse(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var uploads = ParseView(data);
        foreach (var upload in uploads)
        {
            upload.Data = upload.DataView.ToArray();
            upload.DataView = default;
        }
        return uploads;
    }

    internal static List<Ps2GifUpload> ParseView(ReadOnlyMemory<byte> data)
    {
        var uploads = new List<Ps2GifUpload>();
        Ps2GifUpload pending = null;
        int pos = 0;

        while (pos + 16 <= data.Length)
        {
            ulong low = BinaryPrimitives.ReadUInt64LittleEndian(data.Span.Slice(pos));
            ulong registers = BinaryPrimitives.ReadUInt64LittleEndian(data.Span.Slice(pos + 8));
            pos += 16;

            int nloop = (int)(low & 0x7FFF);
            int flg = (int)(low >> 58 & 3);
            int nreg = (int)(low >> 60 & 0xF);
            if (nreg == 0) nreg = 16;

            if (flg == 0)
            {
                // PACKED: nloop * nreg quadwords. Only A+D (register 0xE) is used for uploads.
                for (int i = 0; i < nloop * nreg; i++)
                {
                    if (pos + 16 > data.Length) throw new InvalidDataException("Truncated PS2 GIF register packet.");
                    ulong value = BinaryPrimitives.ReadUInt64LittleEndian(data.Span.Slice(pos));
                    int address = data.Span[pos + 8];
                    pos += 16;

                    if (((registers >> ((i % nreg) * 4)) & 0xF) != 0xE)
                        continue;

                    if (address == RegBitBltBuf)
                        pending = new Ps2GifUpload { BitBltBuf = value, HasBitBltBuf = true };
                    else if (address == RegTrxPos)
                    {
                        if (pending == null) pending = new Ps2GifUpload();
                        pending.TrxPos = value;
                    }
                    else if (address == RegTrxReg)
                    {
                        if (pending == null) pending = new Ps2GifUpload();
                        pending.Width = (int)(value & 0xFFF);
                        pending.Height = (int)(value >> 32 & 0xFFF);
                    }
                }
            }
            else if (flg == 2)
            {
                int size = nloop * 16;
                if (pos + size > data.Length) throw new InvalidDataException("Truncated PS2 GIF image packet.");
                if (pending != null)
                {
                    pending.DataView = data.Slice(pos, size);
                    uploads.Add(pending);
                    pending = null;
                }
                pos += size;
            }
            else
            {
                throw new NotSupportedException("Unsupported PS2 GIF packet format.");
            }
        }

        if (pos != data.Length) throw new InvalidDataException("Truncated PS2 GIF tag.");

        return uploads;
    }
}
