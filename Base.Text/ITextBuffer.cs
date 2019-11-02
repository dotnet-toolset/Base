using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Text
{
    public interface ITextBuffer : IDisposable
    {
        /// <summary>
        /// Get current 0-based offset in the text buffer, this is the position of Current character
        /// Returns -1 if Next() has never been called on this buffer
        /// </summary>
        int Position { get; }
        /// <summary>
        /// Get character in the buffer at current position.
        /// </summary>
        int Current { get; }
        /// <summary>
        /// Get character immediately following current one.
        /// Does not advance current position.
        /// Returns Eof if the current characher is the last one.
        /// </summary>
        /// <returns></returns>
        int Peek();
        /// <summary>
        /// Get character immediately following aOffset characters after the current one.
        /// Does not advance current position.
        /// PeekAfter(0) is equivalent to Current
        /// PeekAfter(1) is equivalent to Peek()
        /// Returns Eof if Position+aOffset is beyond the end of the buffer
        /// PeekAfter() operates only within the current buffer (not on the whole stream). 
        /// So it is not recommended to use large aOffset values. 
        /// If aOffset is such that the buffer will need to be updated with new portion of data, and that will lead to current
        /// position pointing below the start of the buffer, PeekAfter will simply return Eof without making any changes to the buffer
        /// </summary>
        /// <param name="aOffset"></param>
        /// <returns></returns>
        int PeekAfter(int aOffset);
        Task<int> PeekAfterAsync(int aOffset);
        /// <summary>
        /// Get string of count characters immediately following current one.
        /// Does not advance current position.
        /// Returns empty string if the current characher is the last one.
        /// </summary>
        /// <returns></returns>
        string Peek(int offset, int count);
        Task<string> PeekAsync(int offset, int count);
        /// <summary>
        /// Attempts to advance current position by 1 and returns character at that position.
        /// Returns Eof if there are no more characters. In this case current position is not advanced.
        /// </summary>
        /// <returns></returns>
        int Next();
        Task<int> NextAsync();
        /// <summary>
        /// Attempts to advance current position by aCount and returns character at that position.
        /// Returns Eof if there are no more characters. In this case current position is not advanced.
        /// </summary>
        /// <param name="aCount"></param>
        /// <returns></returns>
        int Skip(int aCount = 1);
        Task<int> SkipAsync(int aCount = 1);
        /// <summary>
        /// Attempts to step back in the buffer and decrease current position by 1.
        /// Returns Eof if current position is at the beginning of the buffer
        /// </summary>
        /// <returns></returns>
        int Prev();
        /// <summary>
        /// Attempts to match characters after the current one to the aMatch string.
        /// Returns true if match was successful, false otherwise.
        /// </summary>
        /// <param name="aMatch"></param>
        /// <returns></returns>
        bool Matches(string aMatch, bool advanceIfMathes);
        Task<bool> MatchesAsync(string aMatch, bool advanceIfMathes);

        TextLines GetTextLines();
        TextCoord GetCoord(int aOffset);
        int GetOffset(int aRow, int aColum);
    }
}
