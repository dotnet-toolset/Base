namespace Base.Collections.Async
{
    public class TakeResult<T>
    {
        public static readonly TakeResult<T> CompleteInstance = new TakeResult<T>(true);

        public readonly bool End;
        public T Value;

        private TakeResult(bool end)
        {
            End = end;
        }

        public TakeResult(T value)
        {
            Value = value;
        }
    }
}
