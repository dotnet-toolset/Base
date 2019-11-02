namespace Base.Rng
{
    /// <summary>
    /// Pseudo-random number generator
    /// </summary>
    public interface IRng
    {
        /// <summary>
        /// Fills the buffer with pseudo-random bytes
        /// </summary>
        /// <param name="buffer">buffer to fill</param>
        void NextBytes(byte[] buffer);

        /// <summary>
        /// Generates pseudo-random byte
        /// </summary>
        /// <returns>pseudo-random byte</returns>
        byte NextByte();
    }
}
