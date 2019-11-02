using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Base.Text.Impl
{
    public delegate bool TextLineTerminatorTester(int c, int n);

    public abstract class AbstractTextBuffer : ITextBuffer
    {
        public const int Eof = -1;

        private int iLastSeenPosition = -1;
        private readonly List<int> iLines = new List<int>();

        protected readonly TextLineTerminatorTester IsLineTerminator;

        protected AbstractTextBuffer()
        {
            IsLineTerminator = DefaultIsLineTerminator;
        }

        protected AbstractTextBuffer(TextLineTerminatorTester aTester)
        {
            IsLineTerminator = aTester;
        }

        #region ITextBuffer Members

        public abstract int Position { get; }
        public abstract int Current { get; }
        public abstract int Peek();
        public abstract int PeekAfter(int aOffset);

        public virtual Task<int> PeekAfterAsync(int aOffset)
        {
            return Task.FromResult(PeekAfter(aOffset));
        }

        public string Peek(int offset, int count)
        {
            var result = new StringBuilder(count);
            var i = offset;
            while (count-- > 0)
            {
                var c = PeekAfter(i++);
                if (c == Eof) break;
                result.Append((char)c);
            }
            return result.ToString();
        }
        public async Task<string> PeekAsync(int offset, int count)
        {
            var result = new StringBuilder(count);
            var i = offset;
            while (count-- > 0)
            {
                var c = await PeekAfterAsync(i++);
                if (c == Eof) break;
                result.Append((char)c);
            }
            return result.ToString();
        }

        public abstract int Next();

        public virtual Task<int> NextAsync()
        {
            return Task.FromResult(Next());
        }

        public int Skip(int aCount = 1)
        {
            var result = Eof;
            while (aCount-- > 0)
            {
                result = Next();
                if (result == Eof) break;
            }
            return result;
        }

        public async Task<int> SkipAsync(int aCount = 1)
        {
            var result = Eof;
            while (aCount-- > 0)
            {
                result = await NextAsync();
                if (result == Eof) break;
            }
            return result;
        }


        public abstract int Prev();
        public abstract bool Matches(string aMatch, bool advanceIfMathes);

        public virtual Task<bool> MatchesAsync(string aMatch, bool advanceIfMathes)
        {
            return Task.FromResult(Matches(aMatch, advanceIfMathes));
        }

        public TextLines GetTextLines()
        {
            return new TextLines(iLines.ToArray());
        }

        public TextCoord GetCoord(int aOffset)
        {
            var vIndex = iLines.BinarySearch(aOffset);
            if (vIndex >= 0) return new TextCoord(vIndex + 1, 1);
            vIndex = ~vIndex;
            if (vIndex == iLines.Count) return new TextCoord(vIndex + 1, 1);
            return new TextCoord(vIndex + 1, aOffset - iLines[vIndex] + 1);
        }

        public int GetOffset(int aRow, int aColum)
        {
            if (aRow < 0 || aRow >= iLines.Count) return -1;
            return iLines[aRow] + aColum;
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
        }

        #endregion

        protected void RecordLineTerminator(int aPosition, int c, int n)
        {
            if (aPosition <= iLastSeenPosition) return;
            iLastSeenPosition = aPosition;
            if (!IsLineTerminator(c, n)) return;
            iLines.Add(aPosition);
        }

        private static bool DefaultIsLineTerminator(int c, int n)
        {
            switch (c)
            {
                case '\u000A': return true;
                case '\u000D': return n != '\u000A';
            }
            return false;
        }
    }
}
