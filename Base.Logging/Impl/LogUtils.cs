using System.IO;
using System.Text;

namespace Base.Logging.Impl
{
    public class LogUtils
    {
        public static LogLevel NameToLevel(string name)
        {
            switch (name)
            {
                case "trace":
                    return LogLevel.Trace;
                case "debug":
                    return LogLevel.Debug;
                case "info":
                    return LogLevel.Info;
                case "warn":
                    return LogLevel.Warn;
                case "error":
                    return LogLevel.Error;
                case "fatal":
                    return LogLevel.Fatal;
                default:
                    if (int.TryParse(name, out var l))
                        return (LogLevel) l;
                    return LogLevel.None;
            }
        }

        public static StringBuilder ToHexByteLower(StringBuilder dest, int n)
        {
            var d = (n >> 4) & 0xf;
            dest.Append((char) (d <= 9 ? d + 48 : d + 87));
            d = n & 0xf;
            dest.Append((char) (d <= 9 ? d + 48 : d + 87));
            return dest;
        }


        public static StringBuilder ToHexWordLower(StringBuilder dest, int n)
        {
            var d = (n >> 12) & 0xf;
            dest.Append((char) (d <= 9 ? d + 48 : d + 87));
            d = (n >> 8) & 0xf;
            dest.Append((char) (d <= 9 ? d + 48 : d + 87));
            d = (n >> 4) & 0xf;
            dest.Append((char) (d <= 9 ? d + 48 : d + 87));
            d = n & 0xf;
            dest.Append((char) (d <= 9 ? d + 48 : d + 87));
            return dest;
        }

        public static void DumpBytes(TextWriter writer, byte[] buf, int ofs, int len)
        {
            var lineOfs = 0;
            StringBuilder hexPart = new StringBuilder(), charPart = new StringBuilder(), result = new StringBuilder();
            if (ofs < 0) ofs = 0;
            if (ofs > buf.Length) ofs = buf.Length;
            if (ofs + len < 0) len = 0;
            if (ofs + len > buf.Length) len = buf.Length - ofs;
            var i = 0;
            while (len > 0)
            {
                var b = buf[ofs];
                ToHexByteLower(hexPart, b).Append(' ');
                var c = (char) b;
                if (b < 0x20 || b > 0x7F) c = '.';
                charPart.Append(c);
                lineOfs++;
                ofs++;
                len--;
                if (lineOfs > 0 && (lineOfs % 4) == 0)
                    hexPart.Append(' ');
                if (lineOfs == 16 || len == 0)
                {
                    lineOfs = 0;
                    hexPart.Append(new string(' ', 52 - hexPart.Length)).Append("| ").Append(charPart);
                    if (result.Length > 0)
                    {
                        writer.WriteLine(result);
                        result.Length = 0;
                    }

                    ToHexWordLower(result, i).Append(":  ").Append(hexPart);
                    hexPart.Length = 0;
                    charPart.Length = 0;
                    i += 16;
                }
            }

            if (result.Length > 0)
                writer.WriteLine(result);
        }
    }
}