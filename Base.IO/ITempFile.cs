using System;
using System.IO;

namespace Base.IO
{
    public interface ITempFile : IDisposable, ITextReadable, ITextWritable, IReadable, IWritable
    {
        FileInfo File { get; }
        /// <summary>
        /// Claimed temporary file will not be auto-deleted on dispose.
        /// Claiming means that the file has now become non-temporary and is in use.
        /// </summary>
        bool IsClaimed { get; }

        /// <summary>
        /// Claim ownerhsip of this temporary file. Claimed temp files will not be auto-deleted
        /// </summary>
        void Claim();
    }
}