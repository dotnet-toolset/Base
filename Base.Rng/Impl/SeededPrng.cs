using System;
using System.Security.Cryptography;
using System.Text;

namespace Base.Rng.Impl
{
    public class SeededPrng : IRng
    {
        static readonly byte[] Primes = { 7, 11, 23, 37, 43, 59, 71 };

        private readonly HashAlgorithm _hasher;
        private readonly int _hashSize;
        int _mixIndex;
        byte[] _state;
        int _stateFilled;

        internal SeededPrng(HashAlgorithm hasher, string seed)
        {
            _hasher = hasher;
            if ((hasher.HashSize & 7) != 0) throw new NotSupportedException("hash size must be a multiple of 8 bits");
            var seedHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(seed ?? Guid.NewGuid().ToString()));
            for (var i = 0; i < seedHash.Length; i++)
            {
                seedHash[i] *= Primes[i % Primes.Length];
                seedHash = hasher.ComputeHash(seedHash);
            }
            _stateFilled = _hashSize = hasher.HashSize >> 3;
            _state = seedHash;
        }

        void NextState()
        {
            for (var i = 0; i < _hashSize; i++)
                _state[i] ^= Primes[_mixIndex = (_mixIndex + 1) % Primes.Length];
            _state = _hasher.ComputeHash(_state);
            _stateFilled = _hashSize;
        }

        public void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            var length = buffer.Length;
            var offset = 0;
            lock (this)
                while (length > 0)
                {
                    if (length >= _stateFilled)
                    {
                        Buffer.BlockCopy(_state, _hashSize - _stateFilled, buffer, offset, _stateFilled);
                        offset += _stateFilled;
                        length -= _stateFilled;
                        _stateFilled = 0;
                    }
                    else
                    {
                        Buffer.BlockCopy(_state, _hashSize - _stateFilled, buffer, offset, length);
                        _stateFilled -= length;
                        length = 0;
                    }

                    if (_stateFilled == 0)
                        NextState();
                }
        }

        public byte NextByte()
        {
            lock (this)
            {
                var ret = _state[_hashSize - _stateFilled];
                _stateFilled--;
                if (_stateFilled == 0)
                    NextState();
                return ret;
            }
        }


    }
}
