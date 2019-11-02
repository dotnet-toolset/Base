using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Base.Cil
{
    public static class Extensions
    {
        private static readonly Dictionary<AssemblyName, bool> corlibs = new Dictionary<AssemblyName, bool>();

        public static bool IsCorLib(this Assembly asm)
        {
            var name = asm.GetName();
            bool result;
            lock(corlibs)
            if (!corlibs.TryGetValue(name, out result))
                corlibs.Add(name, result = asm.GetType("System.Object", false, false) != null);
            return result;
        }

        public static void EmitLdarg(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index < 255) il.Emit(OpCodes.Ldarg_S, (byte) index);
                    else il.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        public static CustomAttributeBuilder ToAttributeBuilder(this CustomAttributeData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var constructorArguments = new List<object>();
            foreach (var ctorArg in data.ConstructorArguments)
                constructorArguments.Add(ctorArg.Value);

            var propertyArguments = new List<PropertyInfo>();
            var propertyArgumentValues = new List<object>();
            var fieldArguments = new List<FieldInfo>();
            var fieldArgumentValues = new List<object>();
            foreach (var namedArg in data.NamedArguments)
            {
                var fi = namedArg.MemberInfo as FieldInfo;
                var pi = namedArg.MemberInfo as PropertyInfo;

                if (fi != null)
                {
                    fieldArguments.Add(fi);
                    fieldArgumentValues.Add(namedArg.TypedValue.Value);
                }
                else if (pi != null)
                {
                    propertyArguments.Add(pi);
                    propertyArgumentValues.Add(namedArg.TypedValue.Value);
                }
            }

            return new CustomAttributeBuilder(
                data.Constructor,
                constructorArguments.ToArray(),
                propertyArguments.ToArray(),
                propertyArgumentValues.ToArray(),
                fieldArguments.ToArray(),
                fieldArgumentValues.ToArray());
        }
        
    }
}