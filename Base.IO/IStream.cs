using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Base.IO
{
    public interface IStream
    {
        bool CanRead { get; }
        bool CanSeek { get; }
        bool CanWrite { get; }

        long Length { get; } 
        long Position { get; set; }
        
        int Read(byte[] buffer, int offset, int count);
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        void Write(byte[] buffer, int offset, int count);
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        void Flush();
        Task FlushAsync(CancellationToken cancellationToken);

        long Seek(long offset, SeekOrigin origin);
        void SetLength(long value);
    }
}