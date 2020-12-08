using System.Collections.Generic;

namespace Base.IO.Vfs
{
    public interface IVfsMountain:IVfs
    {
        IReadOnlyList<IVfsMountPoint> MountPoints { get; }
        IVfsMountPoint Mount(string path, IVfs vfs);
    }
}
