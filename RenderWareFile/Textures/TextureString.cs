namespace RenderWareFile.Textures;

/// <summary>Helpers for the fixed 32-byte name fields used by the PC, Xbox and GameCube texture headers.</summary>
internal static class TextureString
{
    public const int Length = 32;

    public static string Decode(byte[] raw)
    {
        int length = Array.IndexOf(raw, (byte)0);
        if (length < 0) length = raw.Length;
        return System.Text.Encoding.ASCII.GetString(raw, 0, length);
    }

    /// <summary>Returns the original raw bytes if the text is unchanged (preserves padding), otherwise builds a new field.</summary>
    public static byte[] Encode(string text, byte[] originalRaw, byte padding)
    {
        if (originalRaw != null && Decode(originalRaw) == text)
            return originalRaw;

        byte[] result = new byte[Length];
        for (int i = 1; i < Length; i++)
            result[i] = padding;
        for (int i = 0; i < text.Length && i < Length - 1; i++)
            result[i] = (byte)text[i];
        return result;
    }
}
