using System.IO;

namespace Base.IO.Vfs
{
    public interface IVfsFile : IVfsEntry, IReadable, IWritable
    {
        long Size { get; }
        Stream Open(VfsAccess access, VfsMode mode);
    }
}
