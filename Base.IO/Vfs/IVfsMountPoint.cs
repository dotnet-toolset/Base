using System;
using Base.Lang;

namespace Base.IO.Vfs
{
    public interface IVfsMountPoint : INamed, IDisposable
    {
        IVfsMountain Mountain { get; }
        PathSegments Path { get; }
        IVfs Vfs { get; }
    }
}
