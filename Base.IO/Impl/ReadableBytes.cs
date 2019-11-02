using System.IO;

namespace Base.IO.Impl
{
    public class ReadableBytes : ILengthAwareReadable
    {
        public readonly byte[] Bytes;
        public long Length => Bytes.Length;

        public ReadableBytes(byte[] bytes)
        {
            Bytes = bytes;
        }

        public Stream OpenReader()
        {
            return new MemoryStream(Bytes);
        }
    }
}