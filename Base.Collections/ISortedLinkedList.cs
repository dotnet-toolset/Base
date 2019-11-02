namespace Base.Collections
{
    public interface ISortedLinkedListNode<out T>
    {
        T Value { get; }
    }

    public interface ISortedLinkedList<T> : IContains<T>
    {
        ISortedLinkedListNode<T> Head { get; }
        ISortedLinkedListNode<T> Find(T value);
        ISortedLinkedListNode<T> Insert(T value);
        ISortedLinkedListNode<T> Delete(T value);
    }
}