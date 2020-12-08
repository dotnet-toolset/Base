using System;

namespace Base.IO.Vfs
{
    public interface IVfsStat
    {
        ulong? Dev { get; }
        VfsMode Mode { get; }
        ulong? Size { get; }
        ulong? Nlink { get; }
        DateTime? Atim { get; }
        DateTime? Mtim { get; }
        DateTime? Ctim { get; }

        uint? Uid { get; }
        uint? Gid { get; }
        ulong? Rdev { get; }
        ulong? Blksize { get; }
        ulong? Ino { get; }
        ulong? Blocks { get; }

    }
}
