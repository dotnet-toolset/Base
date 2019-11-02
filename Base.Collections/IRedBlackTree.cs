namespace Base.Collections
{
    public interface IRedBlackTree<T> : IContains<T>
    {
        int Count { get; }

        void Add(T data);
        bool Remove(T data);
        void Clear();
    }
}
