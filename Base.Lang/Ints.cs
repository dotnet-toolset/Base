using System;
using System.Linq.Expressions;

namespace Base.Lang
{
    public class Ints
    {
        public static readonly int[] Sizes = {1, 2, 4, 8};

        public static Type Type(int bytes, bool signed)
        {
            switch (bytes)
            {
                case 1:
                    return signed ? typeof(sbyte) : typeof(byte);
                case 2:
                    return signed ? typeof(short) : typeof(ushort);
                case 4:
                    return signed ? typeof(int) : typeof(uint);
                case 8:
                    return signed ? typeof(long) : typeof(ulong);
                default:
                    throw new CodeBug.Unreachable();
            }
        }

        public static TypeCode TypeCode(int bytes, bool signed)
        {
            switch (bytes)
            {
                case 1:
                    return signed ? System.TypeCode.SByte : System.TypeCode.Byte;
                case 2:
                    return signed ? System.TypeCode.Int16 : System.TypeCode.UInt16;
                case 4:
                    return signed ? System.TypeCode.Int32 : System.TypeCode.UInt32;
                case 8:
                    return signed ? System.TypeCode.Int64 : System.TypeCode.UInt64;
                default:
                    throw new CodeBug.Unreachable();
            }
        }

        public static string ToString(long value, int bytes, bool signed)
        {
            switch (bytes)
            {
                case 1:
                    return signed ? Convert.ToString((sbyte) value) : Convert.ToString((byte) value);
                case 2:
                    return signed ? Convert.ToString((short) value) : Convert.ToString((ushort) value);
                case 4:
                    return signed ? Convert.ToString((int) value) : Convert.ToString((uint) value);
                case 8:
                    return signed ? Convert.ToString(value) : Convert.ToString((ulong) value);
                default:
                    throw new CodeBug.Unreachable();
            }
        }

        public static Expression Const(long value, int bytes, bool signed)
        {
            var t = Type(bytes, signed);
            Expression c;
            if (value.GetType() == t) c = Expression.Constant(value);
            else
                switch (bytes)
                {
                    case 1:
                        c = signed ? Expression.Constant((sbyte) value) : Expression.Constant((byte) value);
                        break;
                    case 2:
                        c = signed ? Expression.Constant((short) value) : Expression.Constant((ushort) value);
                        break;
                    case 4:
                        c = signed ? Expression.Constant((int) value) : Expression.Constant((uint) value);
                        break;
                    case 8:
                        c = signed ? Expression.Constant(value) : Expression.Constant((ulong) value);
                        break;
                    default:
                        throw new CodeBug.Unreachable();
                }
            return c;
        }
    }
}