using System;
using System.IO;
using System.Threading;

namespace Base.IO.Impl
{
    /// <summary>
    /// Readable that allows its stream to be opened only once
    /// </summary>
    public class ForwardReadableStream : IReadable
    {
        private readonly Stream _stream;
        private int _counter;

        public ForwardReadableStream(Stream stream)
        {
            _stream = stream;
        }

        public Stream OpenReader()
        {
            if (Interlocked.Increment(ref _counter) > 1)
                throw new InvalidOperationException("cannot open forward readable stream more than once");
            return _stream;
        }

    }
}