using System;

namespace Base.Text
{
    public class TextLines
    {
        /// <summary>
        /// Array holding offsets of the starts of the lines in the text. Array must be sorted
        /// </summary>
        public readonly int[] Lines;

        public TextLines(int[] lines)
        {
            Lines = lines;
        }

        public TextCoord GetCoord(int offset)
        {
            var index = Array.BinarySearch(Lines, offset);
            if (index >= 0) return new TextCoord(index + 1, 1);
            index = ~index;
            if (index == Lines.Length) return new TextCoord(index + 1, 1);
            return new TextCoord(index + 1, offset - Lines[index] + 1);
        }

        public int GetOffset(int row, int column)
        {
            if (row < 0 || row >= Lines.Length) return -1;
            return Lines[row] + column;
        }
    }
}
