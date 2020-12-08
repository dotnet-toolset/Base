namespace Base.Lang
{
    public interface IImmutableBuilder<out T>
        where T : class
    {
        T Build();
    }
}
