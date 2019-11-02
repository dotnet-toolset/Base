using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Base.IO
{
    public static class Extensions
    {
        public const int DefaultStreamCopyBufferSize = 65536;

        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false, false);

        public static T GetBase<T>(this Stream stream) where T : Stream
        {
            do
            {
                if (stream is T result) return result;
                if (!(stream is IStreamWrapper wrapper)) return null;
                stream = wrapper.Base;
            } while (true);
        }
        public static Stream GetBase(this Stream stream) 
        {
            do
            {
                if (!(stream is IStreamWrapper wrapper)) return null;
                stream = wrapper.Base;
            } while (true);
        }


        public static int CopyTo(this TextReader source, Stream dest)
        {
            if (source == null || dest == null) return -1;
            var buf = new char[4096];
            var charsWritten = 0;
            using (var w = new StreamWriter(dest, Utf8NoBom, buf.Length, true))
            {
                do
                {
                    var actual = source.Read(buf, 0, buf.Length);
                    if (actual == 0) break;
                    w.Write(buf, 0, actual);
                    charsWritten += actual;
                } while (true);
                w.Flush();
            }
            return charsWritten;
        }

        public static async Task<int> CopyToAsync(this TextReader source, Stream dest, CancellationToken ct)
        {
            if (source == null || dest == null) return -1;
            var buf = new char[4096];
            var charsWritten = 0;
            using (var w = new StreamWriter(dest, Utf8NoBom, buf.Length, true))
            {
                do
                {
                    var actual = await source.ReadAsync(buf, 0, buf.Length);
                    if (actual == 0) break;
                    await w.WriteAsync(buf, 0, actual);
                    charsWritten += actual;
                    ct.ThrowIfCancellationRequested();
                } while (true);
                w.Flush();
            }
            return charsWritten;
        }

        public static bool? DataAvailable(this Stream stream)
        {
            var ns = stream.GetBase<NetworkStream>();
            return ns?.DataAvailable;
        }

        public static void Write(this Stream stream, string s)
        {
            if (stream == null || string.IsNullOrEmpty(s)) return;
            using (var w = new StreamWriter(stream, Utf8NoBom, Math.Max(s.Length, 1024), true))
            {
                w.Write(s);
                w.Flush();
            }
        }

        public static async Task WriteAsync(this Stream stream, string s, CancellationToken ct)
        {
            if (stream == null || string.IsNullOrEmpty(s)) return;
            using (var w = new StreamWriter(stream, Utf8NoBom, Math.Max(s.Length, 1024), true))
            {
                await w.WriteAsync(s);
                await w.FlushAsync();
            }
        }

        public static void Write(this Stream stream, byte[] buf)
        {
            if (buf == null || buf.Length == 0) return;
            stream.Write(buf, 0, buf.Length);
        }

        public static Task WriteAsync(this Stream stream, byte[] buf, CancellationToken ct)
        {
            if (buf == null || buf.Length == 0) return Task.FromResult(true);
            return stream.WriteAsync(buf, 0, buf.Length, ct);
        }


        public static void Write(this FileInfo file, string s)
        {
            if (file == null || string.IsNullOrEmpty(s)) return;
            using (var stream = file.Open(FileMode.Create)) // don't use file.OpenWrite, it does not truncate!
                stream.Write(s);
        }

        public static async Task WriteAsync(this FileInfo file, string s, CancellationToken ct)
        {
            if (file == null || string.IsNullOrEmpty(s)) return;
            using (var stream = file.Open(FileMode.Create)) // don't use file.OpenWrite, it does not truncate!
                await stream.WriteAsync(s, ct);
        }

        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                return mem.ToArray();
            }
        }

        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken ct)
        {
            using (var mem = new MemoryStream())
            {
                await stream.CopyToAsync(mem, DefaultStreamCopyBufferSize, ct);
                return mem.ToArray();
            }
        }

        public static byte[] ReadAllBytes(this IReadable readable)
        {
            if (readable is ILengthAwareReadable knowsSize)
            {
                var result = new byte[knowsSize.Length];
                using (var s = readable.OpenReader())
                    s.Read(result, 0, result.Length);
                return result;
            }
            using (var mem = new MemoryStream())
            using (var s = readable.OpenReader())
            {
                s.CopyTo(mem);
                return mem.ToArray();
            }
        }

        public static async Task<byte[]> ReadAllBytesAsync(this IReadable readable, CancellationToken ct)
        {
            if (readable is ILengthAwareReadable knowsSize)
            {
                var result = new byte[knowsSize.Length];
                using (var s = readable.OpenReader())
                    await s.ReadAsync(result, 0, result.Length, ct);
                return result;
            }
            using (var mem = new MemoryStream())
            using (var s = readable.OpenReader())
            {
                await s.CopyToAsync(mem, DefaultStreamCopyBufferSize, ct);
                return mem.ToArray();
            }
        }

        public static void CopyTo(this IReadable readable, Stream dest)
        {
            using (var s = readable.OpenReader())
            {
                s.CopyTo(dest);
                dest.Flush();
            }
        }

        public static async Task CopyToAsync(this IReadable readable, Stream dest, CancellationToken ct)
        {
            using (var s = readable.OpenReader())
            {
                await s.CopyToAsync(dest, DefaultStreamCopyBufferSize, ct);
                await dest.FlushAsync(ct);
            }
        }

        public static void CopyTo(this IStream source, IStream destination, int bufferSize=DefaultStreamCopyBufferSize)
        {
            var array = new byte[bufferSize];
            int count;
            while ((count = source.Read(array, 0, array.Length)) != 0)
                destination.Write(array, 0, count);
        }       
        
        public static void CopyTo(this Stream source, IStream destination, int bufferSize=DefaultStreamCopyBufferSize)
        {
            var array = new byte[bufferSize];
            int count;
            while ((count = source.Read(array, 0, array.Length)) != 0)
                destination.Write(array, 0, count);
        }       
    }
}