namespace RenderWareFile.Textures;

/// <summary>
/// Xbox texture (un)swizzling: the texels are stored in Morton (Z-order) order.
/// </summary>
internal static class XboxSwizzle
{
    /// <summary>Returns the linear index in the swizzled data for the texel (x, y).</summary>
    public static int GetSwizzledIndex(int x, int y, int width, int height)
    {
        int index = 0;
        int outBit = 0;
        int wBits = Log2(width), hBits = Log2(height);
        int common = Math.Min(wBits, hBits);

        for (int i = 0; i < common; i++)
        {
            index |= ((x >> i) & 1) << outBit++;
            index |= ((y >> i) & 1) << outBit++;
        }

        // the remaining bits of the longer dimension are appended linearly
        if (wBits > common)
            index |= (x >> common) << outBit;
        else if (hBits > common)
            index |= (y >> common) << outBit;

        return index;
    }

    /// <summary>Unswizzles byte aligned texels (bytesPerTexel 1, 2 or 4).</summary>
    public static byte[] Unswizzle(byte[] data, int width, int height, int bytesPerTexel)
    {
        if (bytesPerTexel < 1 || bytesPerTexel > 4 || !IsPowerOfTwo(width) || !IsPowerOfTwo(height))
            return null;
        if (data.Length < width * height * bytesPerTexel)
            return null;

        byte[] output = new byte[width * height * bytesPerTexel];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int src = GetSwizzledIndex(x, y, width, height) * bytesPerTexel;
                Buffer.BlockCopy(data, src, output, (y * width + x) * bytesPerTexel, bytesPerTexel);
            }
        return output;
    }

    private static bool IsPowerOfTwo(int v) => v > 0 && (v & (v - 1)) == 0;

    private static int Log2(int v)
    {
        int n = 0;
        while ((1 << (n + 1)) <= v) n++;
        return n;
    }
}
