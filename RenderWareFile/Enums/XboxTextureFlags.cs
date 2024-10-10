using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace RenderWareFile.Enums
{
    public enum DXTFormat : byte
    {
        None = 0,
        DXT1 = 12,
        DXT2 = 13,
        DXT3 = 14,
        DXT4 = 16,
        DXT5 = 15,
    }
}
