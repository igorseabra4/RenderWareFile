using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RenderWareFile
{
    public struct RWVersion : IComparable<RWVersion>, IEquatable<RWVersion>
    {
        public byte Renderware { get; set; }
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Revision { get; set; }
        public ushort LibraryBuild { get; set; }
        public int PackedVersion
        {
            get => Renderware << 16 | (Major & 0xF) << 12 | (Minor & 0xF) << 8 | Revision;
            set
            {
                Renderware = (byte)(value >> 16);
                Major = (byte)(value >> 12 & 0xF);
                Minor = (byte)(value >> 8 & 0xF);
                Revision = (byte)(value & 0xFF);
            }
        }

        public RWVersion(byte renderware, byte major, byte minor, byte revision, ushort build = 0xFFFF)
        {
            Renderware = renderware;
            Major = major;
            Minor = minor;
            Revision = revision;
            LibraryBuild = build;
        }

        public RWVersion(byte renderware, byte major) : this(renderware, major, 0, 0) { }

        public RWVersion(int ver)
        {
            int unpackedVer = (ver & 0xFFFF000) != 0 ? (ver >> 14 & 0x3FF00) + 0x30000 | (ver >> 16 & 0x3F) : ver << 8;
            Renderware = (byte)(unpackedVer >> 16);
            Major = (byte)(unpackedVer >> 12 & 0xF);
            Minor = (byte)(unpackedVer >> 8 & 0xF);
            Revision = (byte)(unpackedVer & 0xFF);
            LibraryBuild = (ushort)(ver & 0xFFFF);
        }

        public RWVersion(BinaryReader reader) : this(reader.ReadInt32()) { }

        public static RWVersion Undefined => new RWVersion(0, 0, 0, 0);
        public static RWVersion MinVersion => new RWVersion(3, 1, 0, 0);
        public static RWVersion MaxVersion => new RWVersion(3, 7, 0, 2);
        public static bool IsValidRange(RWVersion ver) => ver >= MinVersion && ver <= MaxVersion;
        public bool InRange(RWVersion v1, RWVersion v2) => PackedVersion >= v1.PackedVersion && PackedVersion <= v2.PackedVersion;

        public int Write()
        {
            int ver = PackedVersion;
            if (ver <= 0x31000)
                return ver >> 8;
            return ((ver - 0x30000) & 0x3FF00) << 14 | (ver & 0x3F) << 16 | LibraryBuild;
        }

        public static bool operator >(RWVersion v1, RWVersion v2) => v1.PackedVersion > v2.PackedVersion;
        public static bool operator <(RWVersion v1, RWVersion v2) => v1.PackedVersion < v2.PackedVersion;
        public static bool operator >=(RWVersion v1, RWVersion v2) => v1.PackedVersion >= v2.PackedVersion;
        public static bool operator <=(RWVersion v1, RWVersion v2) => v1.PackedVersion <= v2.PackedVersion;

        public static bool operator ==(RWVersion v1, RWVersion v2) => v1.PackedVersion == v2.PackedVersion;
        public static bool operator !=(RWVersion v1, RWVersion v2) => v1.PackedVersion != v2.PackedVersion;

        public int CompareTo(RWVersion other) => PackedVersion.CompareTo(other.PackedVersion);

        public bool Equals(RWVersion other) => PackedVersion == other.PackedVersion;

        public override bool Equals(object obj) => obj is RWVersion other && Equals(other);
        public override int GetHashCode() => PackedVersion.GetHashCode();
        public override string ToString() => $"{Renderware}.{Major}.{Minor}.{Revision}";

        public static implicit operator RWVersion((byte renderware, byte major, byte minor, byte revision, ushort build) verTuple)
            => new RWVersion(verTuple.renderware, verTuple.major, verTuple.minor, verTuple.revision, verTuple.build);

        public static implicit operator RWVersion((byte renderware, byte major, byte minor, byte revision) verTuple)
            => new RWVersion(verTuple.renderware, verTuple.major, verTuple.minor, verTuple.revision);

        public static implicit operator RWVersion((byte renderware, byte major) verTuple)
            => new RWVersion(verTuple.renderware, verTuple.major);

        public static implicit operator RWVersion(int ver) => new RWVersion(ver);
        public static implicit operator int(RWVersion v) => v.Write();
    }
}
