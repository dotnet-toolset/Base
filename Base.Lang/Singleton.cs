using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Lang
{
    public static class Singleton
    {
        private static readonly ConcurrentDictionary<Type, object> Map = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, TaskCompletionSource<object>> WhenMap = new ConcurrentDictionary<Type, TaskCompletionSource<object>>();

        public static T Get<T>() where T : class
        {
            lock (Map)
            {
                if (!Map.TryGetValue(typeof(T), out var vObject) || vObject == null) return default;
                if (vObject is T imm) return imm;
                if (!(vObject is Lazy<T> lazy)) throw new CodeBug.Unreachable();
                imm = lazy.Value;
                Map.TryUpdate(typeof(T), imm, vObject);
                return imm;
            }
        }
        public static Lazy<T> LazyGet<T>() where T : class
        {
            return new Lazy<T>(Get<T>, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static async Task<T> When<T>() where T : class
        {
            return (T)await WhenMap.GetOrAdd(typeof(T), t =>
            {
                var tcs = new TaskCompletionSource<object>();
                lock (Map)
                    if (Map.TryGetValue(typeof(T), out var vObject))
                        tcs.SetResult(vObject);
                return tcs;
            }).Task;
        }

        private static void Put(Type aType, object a)
        {
            lock (Map)
                do
                {
                    Map.AddOrUpdate(aType, a, (type, o) =>
                    {
                        if (!ReferenceEquals(o, a))
                            throw new ArgumentException($"Singleton object of type [{type}] already exists: {o}");
                        return o;
                    });
                    if (WhenMap.TryGetValue(aType, out var tcs))
                        tcs.TrySetResult(a);
                    aType = aType.IsInterface ? aType.BaseType : null;
                } while (aType != null);
        }


        public static T Put<T>(T a) where T : class
        {
            if (a == null) return null;
            var vInterfaceType = typeof(T);
            var vObjectType = a.GetType();
            Put(vInterfaceType, a);
            if (vInterfaceType != vObjectType) Put(vObjectType, a);
            return a;
        }

        public static void Put<T>(Func<T> a) where T : class
        {
            if (a == null) throw new ArgumentNullException();
            var vInterfaceType = typeof(T);
            var vObjectType = a.GetType().GenericTypeArguments[0];
            var lazy = new Lazy<T>(a, LazyThreadSafetyMode.ExecutionAndPublication);
            Put(vInterfaceType, lazy);
            if (vInterfaceType != vObjectType) Put(vObjectType, lazy);
        }

        public static void Put(Type[] aTypes, object a)
        {
            if (a == null) return;
            var vObjectType = a.GetType();
            foreach (var vType in aTypes)
                if (vType != vObjectType)
                    Put(vType, a);
            Put(vObjectType, a);
        }

        public static void Remove<T>(T a) where T : class
        {
            if (a == null) return;
            var vInterfaceType = typeof(T);
            var vObjectType = a.GetType();
            var vType = vInterfaceType;
            lock (Map)
            {
                TaskCompletionSource<object> tcs;
                do
                {
                    Map.TryRemove(vType, out _);
                    if (WhenMap.TryRemove(vType, out tcs)) tcs.TrySetCanceled();
                    vType = vType.IsInterface ? vType.BaseType : null;
                } while (vType != null);
                if (vInterfaceType != vObjectType)
                {
                    Map.TryRemove(vObjectType, out _);
                    if (WhenMap.TryRemove(vObjectType, out tcs)) tcs.TrySetCanceled();
                }
            }
        }
    }
}
