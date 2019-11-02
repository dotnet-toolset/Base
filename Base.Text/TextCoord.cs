using System.Text;

namespace Base.Text
{
    /// <summary>
    /// Row/Column coordinates in the text stream
    /// </summary>
    public class TextCoord
    {
        public readonly int Row, Column;

        public TextCoord(int aRow, int aColumn)
        {
            Row = aRow;
            Column = aColumn;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(Row).Append(':').Append(Column);
            return result.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is TextCoord o && o.Column == Column && o.Row == Row;
        }

        public override int GetHashCode()
        {
            return (Row << 16) ^ Column;
        }
    }
}
