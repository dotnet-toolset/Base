using System.Collections.Generic;

namespace Base.IO.Vfs
{
    public interface IVfsDirectory : IVfsEntry, IEnumerable<IVfsEntry>
    {
        IEnumerable<IVfsEntry> Find(VfsSearchParams args);
        void Create();

        IVfsEntry Find(PathSegments path);
        IVfsFile GetFile(PathSegments path);
        IVfsDirectory GetDirectory(PathSegments path);
    }
}
