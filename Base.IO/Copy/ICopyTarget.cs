namespace Base.IO.Copy
{
    public interface ICopyTarget
    {
        int WriteFrom(ICopyBuffer buffer);
    }
}