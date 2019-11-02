using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Base.Collections.Props;

namespace Base.IO.Impl
{
    public class StreamWrapper : Stream, IStreamWrapper, IPropsContainer, IObservableStream
    {
        protected Stream _base;
        protected readonly bool _ownsBase;
        private readonly Subject<StreamEvent> _subject;

        public Stream Base => _base;
        public bool OwnsBase => _ownsBase;

        public StreamWrapper(Stream aBase, bool aOwnsBase)
        {
            _base = aBase;
            _ownsBase = aOwnsBase;
            _subject = new Subject<StreamEvent>();
        }

        protected override void Dispose(bool disposing)
        {
            if (_ownsBase && disposing)
            {
                _base.Dispose();
                Fire(new StreamEvent.Disposed(this));
            }

            base.Dispose(disposing);
        }

        public override bool CanRead => _base.CanRead;
        public override bool CanSeek => _base.CanSeek;
        public override bool CanWrite => _base.CanWrite;

        public override void Flush()
        {
            _base.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _base.FlushAsync(cancellationToken);
        }

        public override int WriteTimeout
        {
            get => _base.WriteTimeout;
            set => _base.WriteTimeout = value;
        }

        public override int ReadTimeout
        {
            get => _base.ReadTimeout;
            set => _base.ReadTimeout = value;
        }

        public override long Length => _base.Length;

        public override long Position
        {
            get => _base.Position;
            set => _base.Position = value;
        }

        protected T Fire<T>(T ev) where T : StreamEvent
        {
            _subject.OnNext(ev);
            return ev;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Fire(new StreamEvent.AfterRead(this, buffer, offset, count)
                {Result = _base.Read(buffer, offset, count)}).Result;
        }

        /* we must override Async reads/writes because _base will most probably have custom implementations of these and we will miss them if we don't override here */

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            return Fire(new StreamEvent.AfterRead(this, buffer, offset, count)
                {Result = await _base.ReadAsync(buffer, offset, count, cancellationToken)}).Result;
        }

        /* We must not override this here, because the default Stream.ReadByte which we inherit 
         * calls Read(byte[] buffer, int offset, int count) that we already override.
         * Besides, if we do it this way, we force our children to override ReadByte and that doesn't make much sense
        public override int ReadByte()
        {
            return _base.ReadByte();
        }*/

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _base.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _base.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Fire(new StreamEvent.BeforeWrite(this, buffer, offset, count));
            _base.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Fire(new StreamEvent.BeforeWrite(this, buffer, offset, count));
            return _base.WriteAsync(buffer, offset, count, cancellationToken);
        }


        /* see motivation in ReadByte
        public override void WriteByte(byte value)
        {
            _base.WriteByte(value);
        }
        */


        public IDisposable Subscribe(IObserver<StreamEvent> observer) => _subject.Subscribe(observer);
        public override string ToString() => _base.ToString();
        private volatile Props _props;

        Props IPropsContainer.GetProps(bool create)
        {
            if (_props != null || !create) return _props;
            lock (this)
                return _props ?? (_props = new Props());
        }

        public static StreamWrapper From(Stream stream, bool owns)
        {
            return stream is StreamWrapper w && w.OwnsBase == owns ? w : new StreamWrapper(stream, owns);
        }
    }
}