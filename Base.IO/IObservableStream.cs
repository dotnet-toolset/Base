using System;

namespace Base.IO
{
    public interface IObservableStream : IStream, IObservable<StreamEvent>
    {
    }
}