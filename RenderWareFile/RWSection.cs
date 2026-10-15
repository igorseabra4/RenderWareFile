using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RenderWareFile
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class RWSection
    {
        public Section sectionIdentifier { get; internal set; }
        public int sectionSize { get; internal set; }
        public RWVersion renderWareVersion { get; internal set; }

        public byte[] GetBytes(int fileVersion)
        {
            List<byte> listBytes = new List<byte>();

            SetListBytes(fileVersion, ref listBytes);

            sectionSize = listBytes.Count();

            renderWareVersion = fileVersion;

            listBytes.InsertRange(0, BitConverter.GetBytes(renderWareVersion.Write()));
            listBytes.InsertRange(0, BitConverter.GetBytes(sectionSize));
            listBytes.InsertRange(0, BitConverter.GetBytes((int)sectionIdentifier));

            return listBytes.ToArray();
        }

        public abstract void SetListBytes(RWVersion fileVersion, ref List<byte> listBytes);
    }
}
