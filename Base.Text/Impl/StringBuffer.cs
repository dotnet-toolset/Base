using System;

namespace Base.Text.Impl
{
    public class StringBuffer : AbstractTextBuffer
    {
        private readonly string _string;
        private int _position;

        public StringBuffer(string aString)
        {
            _string = aString;
            _position = -1;
        }

        #region ITextBuffer Members

        public override int Position => _position;

        public override int Current
        {
            get
            {
                if (_position < 0) throw new InvalidOperationException();
                if (_position >= _string.Length) return Eof;
                return _string[_position];
            }
        }

        public override int Peek()
        {
            var p = _position + 1;
            return p < _string.Length ? _string[p] : Eof;
        }

        public override int PeekAfter(int aOffset)
        {
            var p = _position + aOffset;
            return p < _string.Length ? _string[p] : Eof;
        }

        public override int Next()
        {
            if (_position >= _string.Length) return Eof;
            _position++;
            if (_position >= _string.Length) return Eof;
            var c = _string[_position];
            var n = _position + 1;
            RecordLineTerminator(_position, c, n < _string.Length ? _string[n] : Eof);
            return c;
        }

        public override int Prev()
        {
            if (_position <= 0) return Eof;
            _position--;
            return _string[_position];
        }

        public override bool Matches(string aMatch, bool advanceIfMathes)
        {
            var p = _position + aMatch.Length;
            if (p >= _string.Length) return false;
            var i = aMatch.Length - 1;
            while (i >= 0)
            {
                if (aMatch[i] != _string[p--]) return false;
                i--;
            }
            if (i == -1)
            {
                if (advanceIfMathes)
                    _position += aMatch.Length;
                return true;
            }
            return false;
        }

        #endregion

    }
}
