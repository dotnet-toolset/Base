using Base.Rng.Impl;
using System.Security.Cryptography;

namespace Base.Rng
{
    public static class RngFactory
    {
        /// <summary>
        /// Create pseudo-random number generator using the specified hash algorithm and the seed
        /// </summary>
        /// <param name="hasher">hash algorithm to use</param>
        /// <param name="seed">seed</param>
        /// <returns>pseudo-random number generator</returns>
        public static IRng Create(HashAlgorithm hasher, string seed)
            => new SeededPrng(hasher, seed);

        /// <summary>
        /// Create pseudo-random number generator using the SHA256 as the hash algorithm
        /// </summary>
        /// <param name="seed">seed</param>
        /// <returns>pseudo-random number generator</returns>
        public static IRng CreateSeeded(string seed)
            => Create(SHA256.Create(), seed);

    }
}
