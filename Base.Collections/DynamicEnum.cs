using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Base.Lang;

namespace Base.Collections
{
    /// <summary>
    /// Dynamic enumeration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DynamicEnum<T> : INamed
        where T : DynamicEnum<T>
    {
        private static readonly ConcurrentDictionary<string, T> Map = new ConcurrentDictionary<string, T>();

        /// <summary>
        /// Name of enum entry
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructs dynamic enumeration entry with the given unique name
        /// </summary>
        /// <param name="name"></param>
        protected DynamicEnum(string name)
        {
            Name = name;
            if (!Map.TryAdd(name, (T)this)) throw new CodeBug("duplicate dynamic enum value: " + name);
        }

        public static T Find(string name)
        {
            return Map.TryGetValue(name, out var v) ? v : null;
        }

        public bool Is(string name)
        {
            return Name == name;
        }

        public bool Is(T item)
        {
            return Equals(item);
        }

        public bool In(params T[] list)
        {
            return list.Any(v => v.Name == Name);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as T;
            return other != null && other.Name == Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        private delegate T Creator(string name);
        private static readonly Creator CreatorDelegate;
        static DynamicEnum()
        {
            var type = typeof(T);
            var constructorArgs = new[] { typeof(string) };
            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, constructorArgs, null);
            if (ctor != null)
            {
                var dynamic = new DynamicMethod(string.Empty, type, constructorArgs, type, true);
                var il = dynamic.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ret);
                CreatorDelegate = (Creator) dynamic.CreateDelegate(typeof(Creator));
            }
        }

        public static T New(string name)
        {
            if (CreatorDelegate == null) throw new CodeBug("no constructor for dynamic enum " + typeof(T));
            return CreatorDelegate(name);
        }

        public static bool operator ==(DynamicEnum<T> a, DynamicEnum<T> b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(DynamicEnum<T> a, DynamicEnum<T> b)
        {
            return !Equals(a, b);
        }
    }
}
