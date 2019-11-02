namespace Base.Text
{
    public interface IBaseNEncoding
    {
        string Dictionary { get; }
        int MaxBitCount(int charCount);
        int MaxByteCount(int charCount);
        int MaxCharCount(int bitCount);
        string Encode(byte[] bytes, int byteOfs, int bitCount);
        byte[] Decode(string src);
    }
}