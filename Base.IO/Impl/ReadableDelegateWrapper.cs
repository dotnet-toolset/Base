using System.IO;

namespace Base.IO.Impl
{
    public delegate Stream ReadableDelegate();

    public class ReadableDelegateWrapper : IReadable
    {
        public readonly ReadableDelegate Delegate;

        public ReadableDelegateWrapper(ReadableDelegate d)
        {
            Delegate = d;
        }

        public Stream OpenReader()
        {
            return Delegate();
        }
    }
}