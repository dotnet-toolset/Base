using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Base.IO.Impl
{
    public class TempStream : StreamWrapper, ITempStream
    {
        public const int DefaultThreshold = 65536;

        private int _threshold;
        private TempFile _file;

        public int Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                CheckTreshold(0);
            }
        }

        public TempStream(bool ownsBase = true, int threshold = DefaultThreshold)
            : base(new MemoryStream(), ownsBase)
        {
            _threshold = threshold;
        }

        public TempStream(TempFile file)
            : base(file.OpenWriter(), true)
        {
        }

        // we used to have locking on all I/O, but it is probably unnecessary due to sequential nature of the I/O 
        private void CheckTreshold(int aToBeAdded)
        {
            if (!(_base is MemoryStream memory) || _base.Length + aToBeAdded < _threshold) return;
            _file = new TempFile();
            var file = _file.OpenWriter();
            var position = memory.Position;
            memory.Position = 0;
            memory.CopyTo(file);
            if (file.Position != position) throw new Exception("temp file position mismatch");
            _base = file;
            memory.Dispose();
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckTreshold(count);
            base.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckTreshold(count);
            await base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var actual = base.Read(buffer, offset, count);
            return actual;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            var actual = await base.ReadAsync(buffer, offset, count, cancellationToken);
            return actual;
        }

        public override void SetLength(long value)
        {
            var vDiff = value - _base.Length;
            if (vDiff > 0)
                CheckTreshold(vDiff < int.MaxValue ? (int) vDiff : int.MaxValue);
            base.SetLength(value);
        }

        public void Reset(bool aTruncate = false)
        {
            Position = 0;
            if (aTruncate) SetLength(0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_ownsBase && !disposing)
                try
                {
                    var file = _file;
                    if (file == null) return;
                    _file = null;
                    file.Dispose();
                }
                catch (Exception e)
                {
                    Trace.WriteLine("temp stream cleanup: " + e);
                }
        }

        public bool TryRename(string name)
        {
            var file = _file;
            if (file == null) return false;
            if (file.TryRename(name))
            {
                file.Claim();
                return true;
            }

            return false;
        }


        /// <summary>
        /// This is needed to hold reference to TempStream, so that finalizer doesn't delete our file while we are reading it
        /// </summary>
        private class Reader : StreamWrapper
        {
            private readonly TempStream _parent;

            internal Reader(TempStream parent, Stream s)
                : base(s, true)
            {
                _parent = parent;
            }
        }

        public Stream OpenReader()
        {
            switch (_base)
            {
                case MemoryStream vMemory:
                    return new MemoryStream(vMemory.ToArray());
                case FileStream vFile:
                    return new Reader(this,
                        new FileStream(vFile.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                default:
                    throw new Exception("invalid stream of type " + _base);
            }
        }

        public static TempStream From(Stream s)
        {
            if (s == null) return null;
            var result = new TempStream();
            s.CopyTo(result);
            result.Flush();
            return result;
        }

        public static async Task<TempStream> FromAsync(Stream s, CancellationToken token)
        {
            if (s == null) return null;
            var result = new TempStream();
            await s.CopyToAsync(result, Extensions.DefaultStreamCopyBufferSize, token);
            await result.FlushAsync(token);
            return result;
        }

        public static TempStream From(Stream s, long? size)
        {
            if (s == null) return null;
            var result = (size ?? 0) > DefaultThreshold ? new TempStream(new TempFile()) : new TempStream();
            s.CopyTo(result);
            result.Flush();
            return result;
        }

        public static TempStream From(IReadable readable)
        {
            if (readable == null) return null;
            var result = readable as TempStream;
            if (result == null)
                using (var stream = readable.OpenReader())
                    result = From(stream);
            return result;
        }

        public static async Task<TempStream> FromAsync(IReadable readable, CancellationToken token)
        {
            if (readable == null) return null;
            if (readable is TempStream result) return result;
            if (readable is TempFile file) return new TempStream(file);
            using (var stream = readable.OpenReader())
                result = await FromAsync(stream, token);
            return result;
        }
    }
}