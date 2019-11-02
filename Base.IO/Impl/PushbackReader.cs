using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Base.IO.Impl
{
    /// <summary>
    /// Stream wrapper that allows to rewind reads for non-seakable streams.
    /// Call PushBack add bytes to the top of the read buffer (next Read/ReadAsync will read from the top of the buffer instead of calling the base stream Read)
    /// Use Mark() to remember subsequent reads, dispose the result of Mark() to rewind the stream to the position before Mark was called.
    /// Nested Mark()'s are supported, but make sure to dispose in the same order.
    /// </summary>
    public class PushbackReader : StreamWrapper
    {
        private readonly LinkedList<ArraySegment<byte>> _queue;
        private readonly LinkedList<Marker> _markers;

        public PushbackReader(Stream aBase, bool aOwnsBase)
            : base(aBase, aOwnsBase)
        {
            _queue = new LinkedList<ArraySegment<byte>>();
            _markers = new LinkedList<Marker>();
        }

        private int TryReadFromQueue(byte[] buffer, ref int offset, ref int count)
        {
            var result = 0;
            var item = _queue.First;
            while (item != null)
            {
                var segment = item.Value;
                if (segment.Count > count)
                {
                    var copied = count;
                    Buffer.BlockCopy(segment.Array, segment.Offset, buffer, offset, copied);
                    count -= copied;
                    result += copied;
                    offset += copied;
                    item.Value = new ArraySegment<byte>(segment.Array, segment.Offset + copied, segment.Count - copied);
                    break;
                }
                else
                {
                    var copied = segment.Count;
                    Buffer.BlockCopy(segment.Array, segment.Offset, buffer, offset, copied);
                    count -= copied;
                    result += copied;
                    offset += copied;
                    item = item.Next;
                    _queue.RemoveFirst();
                }
            }
            return result;
        }

        private void MindMarkers(byte[] buffer, int offset, int count)
        {
            if (count <= 0) return;
            lock (_markers)
                if (_markers.First != null)
                {
                    var buf = new byte[count];
                    Buffer.BlockCopy(buffer, offset, buf, 0,
                        count); //  who knows, maybe received modifies the data in the buffer, better make a copy
                    foreach (var marker in _markers)
                        marker.Queue.AddLast(new ArraySegment<byte>(buf));
                }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var c = count;
            var o = offset;
            var result = TryReadFromQueue(buffer, ref offset, ref count);
            if (result != c)
            {
                Debug.Assert(result < c);
                // we need to check if data is available on the blocking streams such as Network stream.
                // otherwise, we may block here even if data is available to be sent to the reader, which is bad for example for HTTP MixedReader
                if (result == 0 || (Base.DataAvailable() ?? false)) 
                    result += base.Read(buffer, offset, count);
            }
            MindMarkers(buffer, o, result);
            return result;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var c = count;
            var o = offset;
            var result = TryReadFromQueue(buffer, ref offset, ref count);
            if (result != c)
            {
                Debug.Assert(result < c);
                // we need to check if data is available on the blocking streams such as Network stream.
                // otherwise, we may block here even if data is available to be sent to the reader, which is bad for example for HTTP MixedReader
                if (result == 0 || (Base.DataAvailable() ?? false))
                    result += await base.ReadAsync(buffer, offset, count, cancellationToken);
            }
            MindMarkers(buffer, o, result);
            return result;
        }

        public void PushBack(ArraySegment<byte> segment)
        {
            _queue.AddLast(segment);
        }

        public Marker Mark(bool resetOnDispose = true)
        {
            return new Marker(this, resetOnDispose);
        }

        public class Marker : IDisposable // we don't use Disposable here because we don't want to actually dispose the object if the exception was thrown from within Dispose
        {
            private bool _disposed;
            internal readonly LinkedList<ArraySegment<byte>> Queue;
            public readonly PushbackReader Reader;
            public readonly bool ResetOnDispose;

            public Marker(PushbackReader reader, bool resetOnDispose)
            {
                Reader = reader;
                Queue = new LinkedList<ArraySegment<byte>>();
                lock (reader._markers)
                    reader._markers.AddLast(this);
                ResetOnDispose = resetOnDispose;
            }

            private void DoReset()
            {
                var last = Queue.First;
                while (last != null)
                {
                    Reader.PushBack(last.Value);
                    last = last.Next;
                }
            }

            public void Reset()
            {
                Debug.Assert(!_disposed, "marker already disposed");
                Dispose();
                DoReset();
            }

            public void Dispose()
            {
                if (_disposed) return;
                lock (Reader._markers)
                {
                    Debug.Assert(Reader._markers.Last?.Value == this, "pushback markers must be disposed in the reverse order");
                    Reader._markers.RemoveLast();
                }
                _disposed = true;
                if (ResetOnDispose) DoReset();
            }
        }
    }
}
