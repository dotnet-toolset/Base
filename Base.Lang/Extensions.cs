using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Base.Lang.Attributes;

namespace Base.Lang
{
    public static class Extensions
    {
        #region Custom Attributes

        #region Type

        public static T GetAttribute<T>(this Type aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T) Attribute.GetCustomAttribute(aType, typeof(T));
        }

        public static T GetAttribute<T>(this Type aType, bool inherit) where T : Attribute
        {
            if (aType == null) return null;
            return (T) Attribute.GetCustomAttribute(aType, typeof(T), inherit);
        }

        public static T[] GetAttributes<T>(this Type aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T[]) Attribute.GetCustomAttributes(aType, typeof(T));
        }

        public static bool HasAttribute<T>(this Type aType) where T : Attribute
        {
            return aType != null && Attribute.IsDefined(aType, typeof(T));
        }

        #endregion

        #region Assembly

        public static T GetAttribute<T>(this Assembly aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T) Attribute.GetCustomAttribute(aType, typeof(T));
        }

        public static T[] GetAttributes<T>(this Assembly aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T[]) Attribute.GetCustomAttributes(aType, typeof(T));
        }

        public static bool HasAttribute<T>(this Assembly aType) where T : Attribute
        {
            return aType != null && Attribute.IsDefined(aType, typeof(T));
        }

        #endregion

        #region MemberInfo

        public static T GetAttribute<T>(this MemberInfo aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T) Attribute.GetCustomAttribute(aType, typeof(T));
        }

        public static T[] GetAttributes<T>(this MemberInfo aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T[]) Attribute.GetCustomAttributes(aType, typeof(T));
        }

        public static bool HasAttribute<T>(this MemberInfo aType) where T : Attribute
        {
            return aType != null && Attribute.IsDefined(aType, typeof(T));
        }

        #endregion

        #region Module

        public static T GetAttribute<T>(this Module aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T) Attribute.GetCustomAttribute(aType, typeof(T));
        }

        public static T[] GetAttributes<T>(this Module aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T[]) Attribute.GetCustomAttributes(aType, typeof(T));
        }

        public static bool HasAttribute<T>(this Module aType) where T : Attribute
        {
            return aType != null && Attribute.IsDefined(aType, typeof(T));
        }

        #endregion

        #region ParameterInfo

        public static T GetAttribute<T>(this ParameterInfo aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T) Attribute.GetCustomAttribute(aType, typeof(T));
        }

        public static T[] GetAttributes<T>(this ParameterInfo aType) where T : Attribute
        {
            if (aType == null) return null;
            return (T[]) Attribute.GetCustomAttributes(aType, typeof(T));
        }

        public static bool HasAttribute<T>(this ParameterInfo aType) where T : Attribute
        {
            return aType != null && Attribute.IsDefined(aType, typeof(T));
        }

        #endregion

        #endregion

        #region Text & Strings

        public static string Join(this IEnumerable<string> a, string aDelimiter, Func<string, string> aStringifier = null)
        {
            if (a == null) return null;
            var vResult = new StringBuilder();
            foreach (var v in a)
            {
                if (vResult.Length > 0)
                    vResult.Append(aDelimiter);
                var s = aStringifier == null ? v : aStringifier(v);
                vResult.Append(s);
            }

            return vResult.ToString();
        }
        public static string[] SplitLines(this string s, bool trim = true, bool skipEmpty = true)
        {
            var vResult = new List<string>();
            string line;
            if (!String.IsNullOrEmpty(s))
                using (var reader = new StringReader(s))
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (trim) line = line.Trim();
                        if (skipEmpty && line.Length == 0) continue;
                        vResult.Add(line);
                    }

            return vResult.ToArray();
        }

        public static void ForEachLine(this string s, Action<string> a, bool trim = true, bool skipEmpty = true)
        {
            string line;
            if (!String.IsNullOrEmpty(s) && a != null)
                using (var reader = new StringReader(s))
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (trim) line = line.Trim();
                        if (skipEmpty && line.Length == 0) continue;
                        a(line);
                    }
        }

        public static IEnumerable<string> ReadLines(this string s, bool trim = true, bool skipEmpty = true)
        {
            string line;
            if (!String.IsNullOrEmpty(s))
                using (var reader = new StringReader(s))
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (trim) line = line.Trim();
                        if (skipEmpty && line.Length == 0) continue;
                        yield return line;
                    }
        }

        public static IEnumerable<string> ReadLines(this TextReader reader, bool trim = true, bool skipEmpty = true)
        {
            if (reader == null) yield break;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (trim) line = line.Trim();
                if (skipEmpty && line.Length == 0) continue;
                yield return line;
            }
        }

        #endregion

        #region Tasks

        /// <summary>
        /// Avoid UnhandledTaskException for tasks that may not be observed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T IgnoreExceptions<T>(this T t) where T : Task
        {
            t.ContinueWith(tt => { tt.Exception?.Flatten(); }, TaskContinuationOptions.OnlyOnFaulted |
                                                               TaskContinuationOptions.ExecuteSynchronously);
            return t;
        }

        /// <summary>
        /// This is a noop to avoid warnings about tasks not being awaited
        /// </summary>
        /// <param name="task"></param>
        public static void RunConcurrently(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (task.Status == TaskStatus.Created)
                task.Start();
            task.IgnoreExceptions();
        }

        /// <summary>
        /// Queues work to the task factory and returns a <see cref="Task"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task Run(this TaskFactory @this, Action action)
        {
            return @this.StartNew(action, @this.CancellationToken,
                @this.CreationOptions | TaskCreationOptions.DenyChildAttach, @this.Scheduler ?? TaskScheduler.Default);
        }

        public static Task Run(this TaskFactory @this, Action action, CancellationToken token)
        {
            return @this.StartNew(action, token, @this.CreationOptions | TaskCreationOptions.DenyChildAttach,
                @this.Scheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        /// Queues work to the task factory and returns a <see cref="Task{TResult}"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action)
        {
            return @this.StartNew(action, @this.CancellationToken,
                @this.CreationOptions | TaskCreationOptions.DenyChildAttach, @this.Scheduler ?? TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action, CancellationToken token)
        {
            return @this.StartNew(action, token, @this.CreationOptions | TaskCreationOptions.DenyChildAttach,
                @this.Scheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        /// Queues work to the task factory and returns a proxy <see cref="Task"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task Run(this TaskFactory @this, Func<Task> action)
        {
            return
                @this.StartNew(action, @this.CancellationToken,
                    @this.CreationOptions | TaskCreationOptions.DenyChildAttach,
                    @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }

        public static Task Run(this TaskFactory @this, Func<Task> action, CancellationToken token)
        {
            return
                @this.StartNew(action, token, @this.CreationOptions | TaskCreationOptions.DenyChildAttach,
                    @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }

        /// <summary>
        /// Queues work to the task factory and returns a proxy <see cref="Task{TResult}"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action)
        {
            return
                @this.StartNew(action, @this.CancellationToken,
                    @this.CreationOptions | TaskCreationOptions.DenyChildAttach,
                    @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }

        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action,
            CancellationToken token)
        {
            return
                @this.StartNew(action, token, @this.CreationOptions | TaskCreationOptions.DenyChildAttach,
                    @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }

        public static async Task Then(
            this Task antecedent, Action continuation)
        {
            await antecedent;
            continuation();
        }

        public static async Task<TNewResult> Then<TNewResult>(
            this Task antecedent, Func<TNewResult> continuation)
        {
            await antecedent;
            return continuation();
        }

        public static async Task Then<TResult>(
            this Task<TResult> antecedent, Action<TResult> continuation)
        {
            continuation(await antecedent);
        }

        public static async Task<TNewResult> Then<TResult, TNewResult>(
            this Task<TResult> antecedent, Func<TResult, TNewResult> continuation)
        {
            return continuation(await antecedent);
        }

        public static async Task Then(
            this Task task, Func<Task> continuation)
        {
            await task;
            await continuation();
        }

        public static async Task<TNewResult> Then<TNewResult>(
            this Task task, Func<Task<TNewResult>> continuation)
        {
            await task;
            return await continuation();
        }

        public static async Task Then<TResult>(
            this Task<TResult> task, Func<TResult, Task> continuation)
        {
            await continuation(await task);
        }

        public static async Task<TNewResult> Then<TResult, TNewResult>(
            this Task<TResult> task, Func<TResult, Task<TNewResult>> continuation)
        {
            return await continuation(await task);
        }

        // This and the one below potentially introduces a problem of unobserved exceptions on tcs.Task
        // It may so happen that noone cares to await on tcs.Task, so exceptions will propagate up to the Unobserved event
        // We may want to consider to call IgnoreException on tcs.Task right away here???
        public static Task<T> Then<T>(this Task<T> task, TaskCompletionSource<T> tcs)
        {
            task.ContinueWith(t =>
            {
                if (t.IsCanceled) tcs.TrySetCanceled();
                else if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                else tcs.TrySetResult(t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
            tcs.Task.IgnoreExceptions();
            return tcs.Task;
        }

        #endregion

        public static int CompareTo<T>(this T? a, T? b)
            where T : struct, IComparable<T>
        {
            if (a.HasValue)
                if (b.HasValue)
                    return a.Value.CompareTo(b.Value);
                else
                    return 1;
            if (b.HasValue)
                return -1;
            return 0;
        }

        public static string GetTitle(this Type aType)
        {
            if (aType == null) return null;
            var vNa = aType.GetCustomAttribute<NameAttribute>();
            var vName = aType.Name;
            if (vNa != null)
            {
                if (vNa.Title != null) return vNa.Title;
                if (vNa.Name != null) vName = vNa.Name;
            }

            var vDa = aType.GetCustomAttribute<DescriptionAttribute>();
            if (vDa?.Description != null) return vDa.Description;
            return vName;
        }

        public static string GetName(this Type aType)
        {
            if (aType == null) return null;
            return aType.GetCustomAttribute<NameAttribute>()?.Name ?? aType.Name;
        }

        public static bool IsNullableType(this Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static Type GetNonNullableType(this Type type) =>
            type.IsNullableType() ? type.GetGenericArguments()[0] : type;

        public static Type GetNullableType(this Type type) => type.IsValueType && !type.IsNullableType()
            ? typeof(Nullable<>).MakeGenericType(type)
            : type;

        /// <summary>
        /// Split string on the first occurence of separator,
        /// for example, SplitFirst("/a/b/c/d", '/') returns ["", "a/b/c/d"]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] SplitFirst(this string str, char separator)
        {
            if (!String.IsNullOrEmpty(str))
            {
                var i = str.IndexOf(separator);
                if (i >= 0)
                    return new[] {str.Substring(0, i), str.Substring(i + 1)};
            }

            return new[] {str};
        }

        public static bool SplitFirst(this string str, char separator, out string first, out string second)
        {
            if (!String.IsNullOrEmpty(str))
            {
                var i = str.IndexOf(separator);
                if (i >= 0)
                {
                    first = str.Substring(0, i);
                    second = str.Substring(i + 1);
                    return true;
                }
            }

            first = null;
            second = null;
            return false;
        }

        public static bool SplitFirst(this string str, char[] separator, out string first, out string second)
        {
            if (!String.IsNullOrEmpty(str))
            {
                var i = str.IndexOfAny(separator);
                if (i >= 0)
                {
                    first = str.Substring(0, i);
                    second = str.Substring(i + 1);
                    return true;
                }
            }

            first = null;
            second = null;
            return false;
        }

        /// <summary>
        /// Split string on the last occurence of separator,
        /// for example, SplitLast("/a/b/c/d", '/') returns ["/a/b/c", "d"]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] SplitLast(this string str, char separator)
        {
            if (!String.IsNullOrEmpty(str))
            {
                var i = str.LastIndexOf(separator);
                if (i >= 0)
                    return new[] {str.Substring(0, i), str.Substring(i + 1)};
            }

            return new[] {str};
        }

        public static bool SplitLast(this string str, char separator, out string first, out string second)
        {
            if (!String.IsNullOrEmpty(str))
            {
                var i = str.LastIndexOf(separator);
                if (i >= 0)
                {
                    first = str.Substring(0, i);
                    second = str.Substring(i + 1);
                    return true;
                }
            }

            first = null;
            second = null;
            return false;
        }

        public static T CheckNotNull<T>(this T obj, string name)
        {
            if (obj == null) throw new ArgumentNullException(name);
            return obj;
        }
    }
}