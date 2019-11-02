using System;
using System.IO;
using System.Threading.Tasks;

namespace Base.Text.Impl
{
    public class TextBuffer : AbstractTextBuffer
    {
        private readonly TextReader iReader;
        private char[] iBuffer;
        private int iBufferCount, iBufferPosition, iPosition;

        public override int Position => iPosition;

        public override int Current
        {
            get
            {
                if (iBufferPosition < 0) throw new InvalidOperationException();
                if (iBufferPosition >= iBufferCount) return Eof;
                return iBuffer[iBufferPosition];
            }
        }

        public override int Peek()
        {
            var p = iBufferPosition + 1;
            return p < iBufferCount ? iBuffer[p] : Eof;
        }

        public override int PeekAfter(int offset)
        {
            CheckBuffer(offset);
            var p = iBufferPosition + offset;
            return p < iBufferCount ? iBuffer[p] : Eof;
        }

        public override async Task<int> PeekAfterAsync(int offset)
        {
            await CheckBufferAsync(offset);
            var p = iBufferPosition + offset;
            return p < iBufferCount ? iBuffer[p] : Eof;
        }

        public bool IsValid => iBufferPosition >= 0 && iBufferPosition < iBufferCount;

        private TextBuffer(TextReader aReader, int size = 8192)
        {
            if ((size & 1) != 0) throw new ArgumentException(nameof(size),"TextReader buffer cannot be odd");
            iReader = aReader;
            iBuffer = new char[size];
            iBufferPosition = -1;
            iPosition = -1;
        }

        private void Initialize()
        {
            iBufferCount = iReader.ReadBlock(iBuffer, 0, iBuffer.Length);
        }

        private async Task InitializeAsync()
        {
            iBufferCount = await iReader.ReadBlockAsync(iBuffer, 0, iBuffer.Length);
        }

        private void CheckBuffer(int n)
        {
            if (iBufferPosition + n < iBufferCount || iBufferCount < iBuffer.Length) return;
            var vToRead = iBufferCount / 2;
            if (n > vToRead) return; // Prevent the case when Peek or Match will leed to loosing current buffer position
            Array.Copy(iBuffer, vToRead, iBuffer, 0, vToRead);
            iBufferCount -= vToRead;
            iBufferPosition -= vToRead;
            iBufferCount += iReader.ReadBlock(iBuffer, vToRead, vToRead);
        }

        private async Task CheckBufferAsync(int n)
        {
            if (iBufferPosition + n < iBufferCount || iBufferCount < iBuffer.Length) return;
            var vToRead = iBufferCount / 2;
            if (n > vToRead) return; // Prevent the case when Peek or Match will leed to loosing current buffer position
            Array.Copy(iBuffer, vToRead, iBuffer, 0, vToRead);
            iBufferCount -= vToRead;
            iBufferPosition -= vToRead;
            iBufferCount += await iReader.ReadBlockAsync(iBuffer, vToRead, vToRead);
        }

        public override int Next()
        {
            if (iBufferPosition > iBufferCount) return Eof;
            iBufferPosition++;
            CheckBuffer(1);
            if (!IsValid)
            {
                iBufferPosition -= 1;
                return Eof;
            }
            iPosition++;
            var c = iBuffer[iBufferPosition];
            var p = iBufferPosition + 1;
            RecordLineTerminator(iPosition, c, p < iBufferCount ? iBuffer[p] : Eof);
            return c;
        }

        public override async Task<int> NextAsync()
        {
            if (iBufferPosition > iBufferCount) return Eof;
            iBufferPosition++;
            await CheckBufferAsync(1);
            if (!IsValid)
            {
                iBufferPosition -= 1;
                return Eof;
            }
            iPosition++;
            var c = iBuffer[iBufferPosition];
            var p = iBufferPosition + 1;
            RecordLineTerminator(iPosition, c, p < iBufferCount ? iBuffer[p] : Eof);
            return c;
        }

        public override int Prev()
        {
            if (iBufferPosition < 0) return Eof;
            iBufferPosition--;
            if (!IsValid)
            {
                iBufferPosition += 1;
                return Eof;
            }
            iPosition--;
            return iBuffer[iBufferPosition];
        }

        public override bool Matches(string aMatch, bool advanceIfMathes)
        {
            CheckBuffer(aMatch.Length);
            var p = iBufferPosition + aMatch.Length;
            if (p >= iBufferCount) return false;
            var i = aMatch.Length - 1;
            while (i >= 0)
            {
                if (aMatch[i] != iBuffer[p--]) return false;
                i--;
            }
            if (i == -1)
            {
                if (advanceIfMathes)
                {
                    iBufferPosition += aMatch.Length;
                    iPosition += aMatch.Length;
                }
                return true;
            }
            return false;
        }

        public override async Task<bool> MatchesAsync(string aMatch, bool advanceIfMathes)
        {
            await CheckBufferAsync(aMatch.Length);
            var p = iBufferPosition + aMatch.Length;
            if (p >= iBufferCount) return false;
            var i = aMatch.Length - 1;
            while (i >= 0)
            {
                if (aMatch[i] != iBuffer[p--]) return false;
                i--;
            }
            if (i == -1)
            {
                if (advanceIfMathes)
                {
                    iBufferPosition += aMatch.Length;
                    iPosition += aMatch.Length;
                }
                return true;
            }
            return false;
        }

        #region IDisposable Members

        public override void Dispose()
        {
            iReader.Dispose();
            iBufferCount = -1;
            iBufferPosition = -1;
            iPosition = -1;
            iBuffer = null;
        }

        #endregion

        public static ITextBuffer Create(TextReader reader, int bufsize = 8192)
        {
            if (reader is StringReader sr)
                return new StringBuffer(sr.ReadToEnd());
            var result = new TextBuffer(reader, bufsize);
            result.Initialize();
            return result;
        }

        public static ITextBuffer Create(TextReader reader)
        {
            if (reader is StringReader sr)
                return new StringBuffer(sr.ReadToEnd());
            var result = new TextBuffer(reader);
            result.Initialize();
            return result;
        }

        public static async Task<ITextBuffer> CreateAsync(TextReader reader)
        {
            if (reader is StringReader sr)
                return new StringBuffer(sr.ReadToEnd());
            var result = new TextBuffer(reader);
            await result.InitializeAsync();
            return result;
        }

        public static async Task<ITextBuffer> CreateAsync(TextReader reader, int bufsize = 8192)
        {
            if (reader is StringReader sr)
                return new StringBuffer(sr.ReadToEnd());
            var result = new TextBuffer(reader, bufsize);
            await result.InitializeAsync();
            return result;
        }
    }
}
