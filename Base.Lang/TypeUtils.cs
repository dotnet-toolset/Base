using System;
using System.Reflection;

namespace Base.Lang
{
    public static class TypeUtils
    {
        public static bool AreEquivalent(Type t1, Type t2) => t1 == t2 || t1.IsEquivalentTo(t2);

        public static bool AreReferenceAssignable(Type dest, Type src) =>
            AreEquivalent(dest, src) || !dest.IsValueType && !src.IsValueType && dest.IsAssignableFrom(src);

        public static bool IsConvertible(Type type)
        {
            type = type.GetNonNullableType();
            if (!type.IsEnum)
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                    default:
                        return false;
                }
            return true;
        }

        internal static bool HasReferenceConversion(Type source, Type dest)
        {
            if (source == typeof(void) || dest == typeof(void))
                return false;
            var sn = source.GetNonNullableType();
            var dn = dest.GetNonNullableType();
            return sn.IsAssignableFrom(dn) ||
                   dn.IsAssignableFrom(sn) || source.IsInterface || dest.IsInterface ||
                   IsLegalExplicitVariantDelegateConversion(source, dest) || source == typeof(object) ||
                   dest == typeof(object);
        }

        public static bool IsLegalExplicitVariantDelegateConversion(Type source, Type dest)
        {
            if (!source.IsDelegate() || !dest.IsDelegate() || !source.IsGenericType || !dest.IsGenericType)
                return false;
            var genericTypeDefinition = source.GetGenericTypeDefinition();
            if (dest.GetGenericTypeDefinition() != genericTypeDefinition)
                return false;
            var genericArguments = genericTypeDefinition.GetGenericArguments();
            var genericArguments2 = source.GetGenericArguments();
            var genericArguments3 = dest.GetGenericArguments();
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var type = genericArguments2[i];
                var type2 = genericArguments3[i];
                if (!AreEquivalent(type, type2))
                {
                    var t = genericArguments[i];
                    if (t.IsInvariant())
                    {
                        return false;
                    }

                    if (t.IsCovariant())
                    {
                        if (!HasReferenceConversion(type, type2))
                        {
                            return false;
                        }
                    }
                    else if (t.IsContravariant() && (type.IsValueType || type2.IsValueType))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}