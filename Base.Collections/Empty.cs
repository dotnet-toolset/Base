using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Base.Lang;

namespace Base.Collections
{
    public static class Empty
    {
        public static T[] Array<T>() => A<T>.Instance;
        public static IReadOnlyList<T> List<T>() => A<T>.Instance;
        public static ILookup<TK, TE> Lookup<TK, TE>() => L<TK, TE>.Instance;
        public static IReadOnlyDictionary<TK, TV> ReadOnlyDictionary<TK, TV>() => D<TK, TV>.Instance;

        public static readonly IDisposable Disposable = new EmptyDisposable();

        class EmptyDisposable : Disposable
        {
        }


        static class A<T>
        {
            public static readonly T[] Instance = new T[0];
        }

        static class L<TKey, TElement>
        {
            public static readonly ILookup<TKey, TElement> Instance =
                Enumerable.Empty<TElement>().ToLookup(x => default(TKey));
        }

        sealed class D<TK, TV> : IReadOnlyDictionary<TK, TV>
        {
            public static readonly IReadOnlyDictionary<TK, TV> Instance = new D<TK, TV>();

            public int Count => 0;

            private D()
            {
            }

            public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
            {
                return Enumerable.Empty<KeyValuePair<TK, TV>>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool ContainsKey(TK key)
            {
                return false;
            }

            public bool TryGetValue(TK key, out TV value)
            {
                value = default(TV);
                return false;
            }

            public TV this[TK key] => throw new KeyNotFoundException();

            public IEnumerable<TK> Keys => Array<TK>();
            public IEnumerable<TV> Values => Array<TV>();
        }

        public static T[] OrEmpty<T>(this T[] a) => a ?? A<T>.Instance;
        public static IReadOnlyList<T> OrEmpty<T>(this IReadOnlyList<T> a) => a ?? A<T>.Instance;


    }
}