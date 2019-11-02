using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Base.Rng
{
    public static class Extensions
    {
        /// <summary>
        /// Generate pseudo-random boolean value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>either <code>true</code> or <code>false</code></returns>
        public static bool NextBoolean(this IRng rng) => rng.NextByte() % 2 == 0;

        /// <summary>
        /// Genearate pseudo-random byte array of the specified length
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <param name="length">length of the array to return</param>
        /// <returns>array of pseudo-random bytes</returns>
        public static byte[] NextBytes(this IRng rng, int length)
        {
            var ret = new byte[length];
            rng.NextBytes(ret);
            return ret;
        }

        /// <summary>
        /// Generate pseudo-random unsigned byte value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="byte.MinValue"/>, <see cref="byte.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static byte NextUInt8(this IRng rng) => rng.NextByte();

        /// <summary>
        /// Generate pseudo-random unsigned byte value less than or equal to the given value.
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="byte.MinValue"/>, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static byte NextUInt8(this IRng rng, byte max)
        {
            if (max == byte.MaxValue) return rng.NextUInt8();
            if (max == 0) return 0;
            const uint num_rand = (uint)byte.MaxValue + 1;
            var num_bins = (uint)max + 1;
            uint bin_size = num_rand / num_bins, defect = num_rand % num_bins;
            byte x;
            do
            {
                x = rng.NextByte();
            } while (num_rand - defect <= x);

            return (byte)(x / bin_size);
        }

        /// <summary>
        /// Generate pseudo-random unsigned byte within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static byte NextUInt8(this IRng rng, byte min, byte max)
        {
            if (max <= min) return min;
            return (byte)(min + rng.NextUInt8((byte)(max - min)));
        }

        /// <summary>
        /// Generate pseudo-random signed byte value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="sbyte.MinValue"/>, <see cref="sbyte.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static sbyte NextInt8(this IRng rng) => (sbyte)rng.NextByte();

        private const int Ovf8s = sbyte.MaxValue + 1;

        /// <summary>
        /// Generate pseudo-random nsigned byte within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static sbyte NextInt8(this IRng rng, sbyte min, sbyte max)
        {
            if (max <= min) return min;
            return (sbyte)(rng.NextUInt8((byte)(min + Ovf8s), (byte)(max + Ovf8s)) - Ovf8s);
        }

        /// <summary>
        /// Generate pseudo-random unsigned short value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="ushort.MinValue"/>, <see cref="ushort.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static ushort NextUInt16(this IRng rng) => BitConverter.ToUInt16(rng.NextBytes(2), 0);

        /// <summary>
        /// Generate pseudo-random unsigned short value less than or equal to the given value.
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="ushort.MinValue"/>, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static ushort NextUInt16(this IRng rng, ushort max)
        {
            if (max == ushort.MaxValue) return rng.NextUInt16();
            if (max == 0) return 0;
            const uint num_rand = (uint)ushort.MaxValue + 1;
            var num_bins = (uint)max + 1;
            uint bin_size = num_rand / num_bins, defect = num_rand % num_bins;
            ushort x;
            do
            {
                x = rng.NextUInt16();
            } while (num_rand - defect <= x);

            return (ushort)(x / bin_size);
        }

        /// <summary>
        /// Generate pseudo-random unsigned short within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static ushort NextUInt16(this IRng rng, ushort min, ushort max)
        {
            if (max <= min) return min;
            return (ushort)(min + rng.NextUInt16((ushort)(max - min)));
        }

        /// <summary>
        /// Generate pseudo-random signed short value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="short.MinValue"/>, <see cref="short.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static short NextInt16(this IRng rng) => BitConverter.ToInt16(rng.NextBytes(2), 0);

        private const int Ovf16s = short.MaxValue + 1;

        /// <summary>
        /// Generate pseudo-random signed short within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static short NextInt16(this IRng rng, short min, short max)
        {
            if (max <= min) return min;
            return (short)(rng.NextUInt16((ushort)(min + Ovf16s), (ushort)(max + Ovf16s)) - Ovf16s);
        }

        /// <summary>
        /// Generate pseudo-random unsigned int value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="uint.MinValue"/>, <see cref="uint.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static uint NextUInt32(this IRng rng) => BitConverter.ToUInt32(rng.NextBytes(4), 0);

        /// <summary>
        /// Generate pseudo-random unsigned int value less than or equal to the given value.
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="uint.MinValue"/>, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static uint NextUInt32(this IRng rng, uint max)
        {
            if (max == uint.MaxValue) return rng.NextUInt32();
            if (max == 0) return 0;
            const ulong num_rand = (ulong)uint.MaxValue + 1;
            var num_bins = (ulong)max + 1;
            ulong bin_size = num_rand / num_bins, defect = num_rand % num_bins;
            uint x;
            do
            {
                x = rng.NextUInt32();
            } while (num_rand - defect <= x);

            return (uint)(x / bin_size);
        }

        /// <summary>
        /// Generate pseudo-random unsigned int within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static uint NextUInt32(this IRng rng, uint min, uint max)
        {
            if (max <= min) return min;
            return min + rng.NextUInt32(max - min);
        }


        /// <summary>
        /// Generate pseudo-random signed int value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="int.MinValue"/>, <see cref="int.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static int NextInt32(this IRng rng) => BitConverter.ToInt32(rng.NextBytes(4), 0);

        private const uint Ovf32s = (uint)int.MaxValue + 1;

        /// <summary>
        /// Generate pseudo-random signed int within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static int NextInt32(this IRng rng, int min, int max)
        {
            if (max <= min) return min;
            return (int)(rng.NextUInt32((uint)(min + Ovf32s), (uint)(max + Ovf32s)) - Ovf32s);
        }

        /// <summary>
        /// Generate pseudo-random unsigned long value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="ulong.MinValue"/>, <see cref="ulong.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static ulong NextUInt64(this IRng rng) => BitConverter.ToUInt64(rng.NextBytes(8), 0);

        /// <summary>
        /// Generate pseudo-random unsigned long value less than or equal to the given value.
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="ulong.MinValue"/>, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static ulong NextUInt64(this IRng rng, ulong max)
        {
            if (max == ulong.MaxValue) return rng.NextUInt64();
            const ulong num_rand = ulong.MaxValue;
            var num_bins = max + 1;
            ulong bin_size = num_rand / num_bins, defect = num_rand % num_bins;
            ulong x;
            do
            {
                x = rng.NextUInt64() - 1;
            } while (num_rand - defect <= x);

            return x / bin_size;
        }

        /// <summary>
        /// Generate pseudo-random unsigned long within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static ulong NextUInt64(this IRng rng, ulong min, ulong max)
        {
            if (max <= min) return min;
            return min + rng.NextUInt64(max - min);
        }

        /// <summary>
        /// Generate pseudo-random signed long value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [<see cref="long.MinValue"/>, <see cref="long.MaxValue"/>] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static long NextInt64(this IRng rng) => BitConverter.ToInt64(rng.NextBytes(8), 0);

        /// <summary>
        /// Generate pseudo-random signed long within the given range (inclusive)
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [min, max] range</returns>
        /// <remarks>Note that both upper and lower boundaries of the range are inclusive</remarks>
        public static long NextInt64(this IRng rng, long min, long max)
        {
            if (max <= min) return min;
            return (long)(rng.NextUInt64((ulong)(min + long.MaxValue + 1), (ulong)(max + long.MaxValue + 1)) -
                           long.MaxValue - 1);
        }


        /// <summary>
        /// Generate pseudo-random double value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [0, 1) range</returns>
        /// <remarks>Note that lower boundary of the range is inclusive, but upper boundary is NOT inclusive</remarks>
        public static double NextDouble(this IRng rng)
        {
            return rng.NextUInt32() / ((double)uint.MaxValue + 1);
        }

        /// <summary>
        /// Generate pseudo-random float value
        /// </summary>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>value from the [0, 1) range</returns>
        /// <remarks>Note that lower boundary of the range is inclusive, but upper boundary is NOT inclusive</remarks>
        public static float NextFloat(this IRng rng)
        {
            return rng.NextUInt32() / ((float)uint.MaxValue + 1);
        }

        /// <summary>
        /// Sort list items in the random order
        /// </summary>
        /// <typeparam name="T">type of items in the list</typeparam>
        /// <param name="list">items in this list will be shuffled</param>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>the same list provided in the <c>list</c> parameter</returns>
        public static IList<T> Shuffle<T>(this IList<T> list, IRng rng)
        {
            for (var i = list.Count - 1; i > 1; i--)
            {
                var k = rng.NextInt32(0, i);
                var tmp = list[k];
                list[k] = list[i];
                list[i] = tmp;
            }

            return list;
        }

        /// <summary>
        /// Select random item from the given collection
        /// </summary>
        /// <typeparam name="T">type of items in the collection</typeparam>
        /// <param name="coll">collection of items</param>
        /// <param name="rng">pseudo-random number generator</param>
        /// <returns>random element or <c>default(T)</c> if the collection is <see langword="null"/> or empty</returns>
        public static T RandomElementOrDefault<T>(this IReadOnlyCollection<T> coll, IRng rng)
        {
            if (coll == null) return default;
            var c = coll.Count;
            if (c == 0) return default;
            if (c == 1) return coll.First();
            var i = rng.NextInt32(0, c - 1);
            if (i == 0) return coll.First();
            return coll.Skip(i).First();
        }
    }
}