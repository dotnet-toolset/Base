using System;

namespace Base.IO.Vfs
{
    [Flags]
    public enum VfsAccess
    {
        ReadOnly = 0,
        WriteOnly = 1,
        ReadWrite = 2,
        Create = 64,
        Exclusive = 128,
        Noctty = 256,
        Trunc = 512,
        Append = 1024,
        Nonblock = 2048,
        Direct = 16384,
        Directory = 65536,
        Nofollow = 131072,
        Sync = 1052672,
    }
}
