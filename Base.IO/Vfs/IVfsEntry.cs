using Base.Lang;

namespace Base.IO.Vfs
{
    public interface IVfsEntry : INamed
    {
        IVfs Vfs { get; }
        IVfsDirectory Parent { get; }
        VfsEntryType Type { get; }
        PathSegments Path { get; }
        bool Exists { get; }
        IVfsStat Stat { get; }

        void Delete();
    }

    public interface IVfsMountedEntry : IVfsEntry
    {
        IVfsMountPoint MountPoint { get; }
        IVfsEntry Entry { get; }
    }
}
