using System;

namespace Base.IO.Vfs
{
    public interface IVfs : IDisposable
    {
        bool CaseSensitive { get; }
        IVfsDirectory Root { get; } 
    }
}
