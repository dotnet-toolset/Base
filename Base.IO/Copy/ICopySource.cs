namespace Base.IO.Copy
{
    public interface ICopySource
    {
        int ReadTo(ICopyBuffer buffer);
    }
}