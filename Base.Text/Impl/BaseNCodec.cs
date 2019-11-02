using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base.Text;

namespace Base.Text.Impl
{
    public class BaseNCodec : IBaseNEncoding
    {
        private readonly string _dic;
        private readonly int[] _rev;
        private readonly char _pad;
        private readonly double _bitsPerDigit;

        public string Dictionary => _dic;

        private BaseNCodec(string dic)
        {
            _dic = dic;
            _rev = new int[1 << (dic.Any(c => (c & 0xff00) != 0) ? 16 : 8)];
            for (var i = 0; i < _rev.Length; i++)
                _rev[i] = -1;
            for (var i = 0; i < _dic.Length; i++)
                _rev[_dic[i]] = i;
            _bitsPerDigit = Math.Log(dic.Length, 2);
            _pad = dic[0];
        }

        public int MaxBitCount(int charCount)
            => (int) Math.Ceiling(charCount * _bitsPerDigit);

        public int MaxByteCount(int charCount)
            => (int) Math.Ceiling(charCount * _bitsPerDigit / 8);

        public int MaxCharCount(int bitCount)
            => (int) Math.Ceiling(bitCount / _bitsPerDigit);

        public string Encode(byte[] bytes, int byteOfs, int bitCount)
        {
            var byteCount = (bitCount + 7) / 8;
            byte lastByteMask = 0xff;
            var lastBits = bitCount % 8;
            if (lastBits != 0)
                lastByteMask >>= 8 - lastBits;

            var pad = 0;
            while (pad < byteCount)
                if (bytes[byteOfs + pad] == 0)
                    pad++;
                else
                    break;

            var cl = _dic.Length;
            var ret = new StringBuilder(MaxCharCount(bitCount));
            var i = 0;
            for (var si = pad; si < byteCount; si++)
            {
                var b = bytes[si + byteOfs];
                if (si == byteCount - 1) b &= lastByteMask;
                i = (i << 8) + b;
                while (i >= cl)
                {
                    ret.Append(_dic[i % cl]);
                    i /= cl;
                }
            }

            if (i > 0) ret.Append(_dic[i]);
            while (pad-- > 0)
                ret.Append(_pad);
            return ret.ToString();
        }

        public byte[] Decode(string src)
        {
            var sl = src.Length;
            var pad = 0;
            for (var x = sl - 1; x >= 0; x--, pad++)
                if (src[x] != _pad)
                    break;

            var dec = new LinkedList<byte>();
            var i = 0;
            var cl = _dic.Length;
            for (var x = sl - 1 - pad; x >= 0; x--)
            {
                var sc = src[x];
                var r = sc >= _rev.Length ? -1 : _rev[sc];
                if (r < 0) return null;
                i = i * cl + r;
                if (i > 0xff)
                {
                    dec.AddFirst((byte) (i & 0xff));
                    i >>= 8;
                }
            }

            if (i > 0)
                dec.AddFirst((byte) i);
            while (pad-- > 0)
                dec.AddFirst(0);
            return dec.ToArray();
        }

        private static readonly ConcurrentDictionary<string, BaseNCodec> Cache =
            new ConcurrentDictionary<string, BaseNCodec>();

        public static BaseNCodec GetInstance(string dic)
            => Cache.GetOrAdd(dic, d => new BaseNCodec(d));
    }
}