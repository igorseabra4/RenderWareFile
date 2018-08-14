using System;
using System.Collections.Generic;
using System.Linq;

namespace RenderWareFile
{
    public abstract class RWSection
    {
        public Section sectionIdentifier;
        public int sectionSize;
        public int renderWareVersion;

        public byte[] GetBytes(int fileVersion)
        {
            List<byte> listBytes = new List<byte>()
            {
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            };

            SetListBytes(fileVersion, ref listBytes);

            sectionSize = listBytes.Count() - 0xC;

            renderWareVersion = fileVersion;

            listBytes[0] = BitConverter.GetBytes((int)sectionIdentifier)[0];
            listBytes[1] = BitConverter.GetBytes((int)sectionIdentifier)[1];
            listBytes[2] = BitConverter.GetBytes((int)sectionIdentifier)[2];
            listBytes[3] = BitConverter.GetBytes((int)sectionIdentifier)[3];
            listBytes[4] = BitConverter.GetBytes(sectionSize)[0];
            listBytes[5] = BitConverter.GetBytes(sectionSize)[1];
            listBytes[6] = BitConverter.GetBytes(sectionSize)[2];
            listBytes[7] = BitConverter.GetBytes(sectionSize)[3];
            listBytes[8] = BitConverter.GetBytes(renderWareVersion)[0];
            listBytes[9] = BitConverter.GetBytes(renderWareVersion)[1];
            listBytes[10] = BitConverter.GetBytes(renderWareVersion)[2];
            listBytes[11] = BitConverter.GetBytes(renderWareVersion)[3];

            return listBytes.ToArray();
        }

        public abstract void SetListBytes(int fileVersion, ref List<byte> listBytes);
    }
}
