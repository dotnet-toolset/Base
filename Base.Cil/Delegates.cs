using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Base.Cil.Attributes;
using Base.Collections;
using Base.Collections.Impl;
using Base.Lang;

namespace Base.Cil
{

    public static class Delegates
    {

        public delegate object GetterDelegate(object instance);
        public delegate void SetterDelegate(object instance, object value);

        public struct Accessors
        {
            public readonly GetterDelegate Getter;
            public readonly SetterDelegate Setter;

            internal Accessors(GetterDelegate getter, SetterDelegate setter)
            {
                Getter = getter;
                Setter = setter;
            }
        }

        public static readonly ICache<MemberInfo, Accessors> _accessors = new StrongCache<MemberInfo, Accessors>();

        public static Accessors For(Type type, string name)
        {
            foreach (var m in type.GetMember(name, BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public))
            {
                var result = _accessors.Get(m, info =>
                {
                    var p = info as PropertyInfo;
                    if (p != null) return For(p);
                    var f = info as FieldInfo;
                    if (f != null) return For(f);
                    return default;
                });
                if (result.Getter != null || result.Setter != null) return result;
            }
            return default;
        }

        public static Accessors For(PropertyInfo aPropertyInfo)
        {
            var obj = Expression.Parameter(typeof(object), "o");
            GetterDelegate aGetter;
            SetterDelegate aSetter;
            var vGetMethod = aPropertyInfo.GetGetMethod() ?? aPropertyInfo.GetGetMethod(true);
            if (vGetMethod != null)
            {
	            var instance = vGetMethod.IsStatic ? null : Expression.Convert(obj, vGetMethod.DeclaringType);
				aGetter = Expression.Lambda<GetterDelegate>(
                        Expression.Convert(
                            Expression.Call(instance, vGetMethod),
                            typeof(object)), obj).Compile();
            }
            else aGetter = null;
            var vSetMethod = aPropertyInfo.GetSetMethod() ?? aPropertyInfo.GetSetMethod(true);
            if (vSetMethod != null)
            {
	            var instance = vSetMethod.IsStatic ? null : Expression.Convert(obj, vSetMethod.DeclaringType);
                var value = Expression.Parameter(typeof(object));
                aSetter = Expression.Lambda<SetterDelegate>(
                        Expression.Call(instance, vSetMethod,
                                        Expression.Convert(value, vSetMethod.GetParameters()[0].ParameterType)), obj,
                        value).Compile();
            }
            else aSetter = null;
            return new Accessors(aGetter, aSetter);
        }

        public static Accessors For(FieldInfo field)
        {
            var dt = field.DeclaringType;
            var ft = field.FieldType;
            var typeName = dt.Name;
            var methodName = typeName + ".get_" + field.Name;
            var method = new DynamicMethod(methodName, typeof(object), new[] { typeof(object) }, dt, true);
            var gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            if (dt.IsValueType)
                gen.Emit(OpCodes.Unbox, dt);
            gen.Emit(OpCodes.Ldfld, field);
            if (ft.IsValueType) gen.Emit(OpCodes.Box, ft);
            gen.Emit(OpCodes.Ret);
            var aGetter = (GetterDelegate)method.CreateDelegate(typeof(GetterDelegate));

            methodName = typeName + ".set_" + field.Name;
            method = new DynamicMethod(methodName, null, new[] { typeof(object), typeof(object) }, dt, true);
            gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            if (dt.IsValueType)
                gen.Emit(OpCodes.Unbox, dt);
            gen.Emit(OpCodes.Ldarg_1);
            if (ft.IsValueType) gen.Emit(OpCodes.Unbox_Any, ft);
            gen.Emit(OpCodes.Stfld, field);
            gen.Emit(OpCodes.Ret);
            var aSetter = (SetterDelegate)method.CreateDelegate(typeof(SetterDelegate));
            return new Accessors(aGetter, aSetter);
        }

        public static Accessors For<T>(Expression<Func<T>> expression)
        {
            var member = ((MemberExpression)expression.Body).Member;
            var field = member as FieldInfo;
            if (field != null) return For(field);
            var property = member as PropertyInfo;
            if (field != null) return For(property);
            throw new CodeBug("invalid expression");
        }

        public static void For<S, T>(FieldInfo field, out Func<S, T> getter, out Action<S, T> setter)
        {
            var instExp = Expression.Parameter(typeof(S));
            var fieldExp = Expression.Field(instExp, field);
            getter = Expression.Lambda<Func<S, T>>(fieldExp, instExp).Compile();
            if (!field.IsInitOnly)
            {
                var valueExp = Expression.Parameter(typeof(T));
                setter = Expression.Lambda<Action<S, T>>(Expression.Assign(fieldExp, valueExp), instExp, valueExp).Compile();
            }
            else setter = null;
        }

        public static TD Constructor<TD>(Type t) where TD:Delegate
        {
            var pars=typeof(TD).GetMethod("Invoke").GetParameters();
            var argTypes = pars.Select(p => p.ParameterType).ToArray();
            var ctor=t.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, argTypes, null);
            var dynamic = new DynamicMethod(string.Empty, t, argTypes, t);
            var  il = dynamic.GetILGenerator();
            for (var i = 0; i <  argTypes.Length; i++)
                Extensions.EmitLdarg(il, i);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);
            return (TD)dynamic.CreateDelegate(typeof(TD));
        }

        public static TD Constructor<T, TD>() where TD : Delegate
        {
            return Constructor<TD>(typeof(T));
        }

        private delegate object Creator();
        private static readonly Type[] ConstructorArgs = new Type[0];
        private static readonly ConcurrentDictionary<Type, Delegate> Creators = new ConcurrentDictionary<Type, Delegate>();
        private static Creator GetCreator(Type type)
        {
            return (Creator)Creators.GetOrAdd(type, t =>
            {
                var ica = type.GetAttribute<InstanceCreatorAttribute>();
                if (ica != null)
                    return GetCreator(ica.Class);
                var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, ConstructorArgs, null);
                if (ctor == null) return null;
                var dynamic = new DynamicMethod(string.Empty, type, ConstructorArgs, type, true);
                var il = dynamic.GetILGenerator();
                if (t.IsValueType)
                {
                    var loc = il.DeclareLocal(t);
                    il.Emit(OpCodes.Ldloca, loc);
                    il.Emit(OpCodes.Initobj);
                    il.Emit(OpCodes.Ldloc, loc);
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    il.Emit(OpCodes.Newobj, ctor);
                    il.Emit(OpCodes.Ret);
                }
                return dynamic.CreateDelegate(typeof(Creator));
            });
        }

        public static object CreateInstance(Type type)
        {
            var creator = GetCreator(type);
            return creator != null ? creator() : FormatterServices.GetUninitializedObject(type);
        }

        public static T CreateInstance<T>()
        {
            var creator = GetCreator(typeof(T));
            return creator != null ? (T)creator() : (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

    }
}
