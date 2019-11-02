using System;
using System.IO;

namespace Base.IO
{
    public interface IReadable
    {
        Stream OpenReader();
    }

    public interface ILengthAwareReadable : IReadable
    {
        long Length { get; }
    }

}