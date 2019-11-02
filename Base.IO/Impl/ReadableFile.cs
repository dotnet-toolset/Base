using System.IO;
using Base.Lang;

namespace Base.IO.Impl
{
    public class ReadableFile : ILengthAwareReadable, INamed
    {
        public readonly FileInfo File;
        public long Length => File.Length;
        public string Name => File.FullName;

        public ReadableFile(string file)
        {
            File = new FileInfo(file);
        }

        public ReadableFile(FileInfo file)
        {
            File = file;
        }

        public Stream OpenReader()
        {
            return File.OpenRead();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}