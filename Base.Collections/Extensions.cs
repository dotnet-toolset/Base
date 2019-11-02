using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base.Collections.Impl;

namespace Base.Collections
{
    public static class Extensions
    {
        #region Tree Nodes

        public static bool IsRoot<T>(this INode<T> node) where T : class, INode<T> =>
            node == null ? throw new ArgumentNullException(nameof(node)) : node.ParentNode == null;

        public static bool IsLeaf<T>(this INode<T> node) where T : class, INode<T> =>
            node == null ? throw new ArgumentNullException(nameof(node)) : node.FirstChild == null;

        public static bool IsDescendantOf<T>(this INode<T> node, INode<T> potentialParent) where T : class, INode<T>
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (potentialParent == null) return false;
            var next = node.ParentNode;
            while (true)
            {
                if (next == null) return false;
                if (next == potentialParent) return true;
                next = next.ParentNode;
            }
        }

        private class CommonAncestorInfo<T> where T : class, INode<T>
        {
            private readonly INode<T> _childA, _childB;

            internal CommonAncestorInfo(INode<T> childA, INode<T> childB)
            {
                _childA = childA;
                _childB = childB;
            }

            internal int Compare()
            {
                if (_childA == _childB) return 0;
                if (_childA.NextSibling == null)
                    return 1;
                if (_childB.NextSibling == null)
                    return -1;
                // Otherwise we need to see which node occurs first.  Crawl backwards from child2 looking for child1.
                for (var child = _childA.PreviousSibling; child != null; child = child.PreviousSibling)
                    if (child == _childB)
                        return 1;
                return -1;
            }
        }

        private static CommonAncestorInfo<T> FindCommonAncestor<T>(INode<T> a, INode<T> b) where T : class, INode<T>
        {
            if (a == null || b == null) return null;
            if (a.ParentNode == b.ParentNode)
                return new CommonAncestorInfo<T>(a, b);
            var chainA = new LinkedList<INode<T>>();
            var chainB = new LinkedList<INode<T>>();
            for (var current = a; current != null; current = current.ParentNode)
                chainA.AddFirst(current);
            for (var current = b; current != null; current = current.ParentNode)
                chainB.AddFirst(current);
            var rootA = chainA.First;
            var rootB = chainB.First;
            if (rootA.Value != rootB.Value) return null;
            LinkedListNode<INode<T>> childA, childB;
            for (childA = rootA, childB = rootB;
                childA != null && childB != null && childA.Value == childB.Value;
                childA = childA.Next, childB = childB.Next) ;
            if (childA == null || childB == null) return null;
            return new CommonAncestorInfo<T>(childA.Value, childB.Value);
        }

        public static bool IsPreceding<T>(this INode<T> a, INode<T> b) where T : class, INode<T> =>
            FindCommonAncestor(a, b)?.Compare() < 0;

        public static int CompareTreeOrder<T>(this INode<T> a, INode<T> b) where T : class, INode<T>
        {
            if (a == b) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            if (a.IsDescendantOf(b)) return -1;
            if (b.IsDescendantOf(a)) return 1;
            if (a.IsPreceding(b)) return -1;
            return 1;
        }

        public static int Index<T>(this INode<T> node) where T : class, INode<T>
        {
            if (node == null) return -1;
            var result = 0;
            while ((node = node.PreviousSibling) != null)
                result++;
            return result;
        }

        /// <summary>
        /// Adds node to the tree after the last child
        /// this used to bear <c>Append</c> name, renamed because of the clash with <c>IEnumerable<>.Append()</c>
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="node"></param>
        /// <typeparam name="T"></typeparam>
        public static void Add<T>(this INode<T> tree, T node) where T : class, INode<T> => tree.Insert(node);

        public static void RemoveAll<T>(this INode<T> tree) where T : class, INode<T>
        {
            T node;
            while ((node = tree.FirstChild) != null)
                tree.Remove(node);
        }

        public static INode<T> Root<T>(this INode<T> node) where T : class, INode<T>
        {
            if (node == null) return null;
            do
            {
                var parent = node.ParentNode;
                if (parent == null) return node;
                node = parent;
            } while (true);
        }

        public static T TraverseNext<T>(this INode<T> node, INode<T> root = null, bool recursive = true)
            where T : class, INode<T>
        {
            if (node == null || (root != null && root == node)) return null;
            if (!recursive) return node.NextSibling;
            var result = node.FirstChild ?? node.NextSibling;
            var parent = node;
            while (result == null)
            {
                parent = parent.ParentNode;
                if (parent == null || parent == root) break;
                result = parent.NextSibling;
            }

            return result;
        }

        public static T TraversePrevious<T>(this INode<T> node, INode<T> root = null, bool recursive = true)
            where T : class, INode<T>
        {
            if (node == null || (root != null && root == node)) return null;
            var result = node.PreviousSibling;
            if (!recursive) return result;
            if (result == null)
            {
                result = node.ParentNode;
                if (result == root) result = null;
            }
            else
                while (result.LastChild != null)
                    result = result.LastChild;

            return result;
        }

        public static IEnumerable<T> Parents<T>(this INode<T> node) where T : class, INode<T>
        {
            if (node == null) yield break;
            for (var n = node.ParentNode; n != null; n = n.ParentNode)
                yield return n;
        }

        public static IEnumerable<T> Parents<T>(this INode<T> node, Predicate<T> predicate) where T : class, INode<T>
        {
            if (node == null) yield break;
            for (var n = node.ParentNode; n != null; n = n.ParentNode)
                if (predicate == null || predicate(n))
                    yield return n;
        }

        public static IEnumerable<T> MeAndParents<T>(this INode<T> node) where T : class, INode<T>
        {
            if (node == null) yield break;
            for (var n = (T) node; n != null; n = n.ParentNode)
                yield return n;
        }

        public static IEnumerable<T> MeAndParents<T>(this INode<T> node, Predicate<T> predicate)
            where T : class, INode<T>
        {
            if (node == null) yield break;
            for (var n = (T) node; n != null; n = n.ParentNode)
                if (predicate == null || predicate(n))
                    yield return n;
        }

        public static IEnumerable<T> EnumerateChildren<T>(this INode<T> node, Predicate<T> predicate = null)
            where T : class, INode<T>
        {
            if (node != null)
            {
                var n = node.FirstChild;
                while (n != null)
                {
                    if (predicate == null || predicate(n))
                        yield return n;
                    n = n.NextSibling;
                }
            }
        }

        public static IEnumerable<TC> EnumerateChildren<T, TC>(this INode<T> node, Predicate<T> predicate = null)
            where T : class, INode<T>
            where TC : class, T
        {
            if (node != null)
            {
                var n = node.FirstChild;
                while (n != null)
                {
                    if (n is TC c && (predicate == null || predicate(n)))
                        yield return c;
                    n = n.NextSibling;
                }
            }
        }

        public static IEnumerable<T> EnumerateSubtree<T>(this INode<T> node, Predicate<T> predicate = null)
            where T : class, INode<T>
        {
            if (node != null)
            {
                var n = node.FirstChild;
                while (n != null)
                {
                    if (predicate == null || predicate(n))
                        yield return n;
                    n = n.TraverseNext(node);
                }
            }
        }

        public static IEnumerable<TC> EnumerateSubtree<T, TC>(this INode<T> node)
            where T : class, INode<T>
            where TC : class, T
        {
            if (node != null)
            {
                var n = node.FirstChild;
                while (n != null)
                {
                    if (n is TC c)
                        yield return c;
                    n = n.TraverseNext(node);
                }
            }
        }

        public static bool VisitSubtree<T>(this INode<T> node, Func<T, bool> visitor) where T : class, INode<T>
        {
            for (var n = node.FirstChild; n != null; n = n.NextSibling)
            {
                if (visitor(n)) return true;
                VisitSubtree(n, visitor);
            }

            return false;
        }

        public static T FirstChild<T>(this INode<T> node, Predicate<T> predicate) where T : class, INode<T>
        {
            if (node != null)
                for (var n = node.FirstChild; n != null; n = n.NextSibling)
                    if (predicate(n))
                        return n;
            return null;
        }

        public static T LastChild<T>(this INode<T> node, Predicate<T> predicate) where T : class, INode<T>
        {
            if (node != null)
                for (var n = node.LastChild; n != null; n = n.PreviousSibling)
                    if (predicate(n))
                        return n;
            return null;
        }

        public static T NextSibling<T>(this INode<T> node, Predicate<T> predicate) where T : class, INode<T>
        {
            if (node != null)
                for (var n = node.NextSibling; n != null; n = n.NextSibling)
                    if (predicate(n))
                        return n;
            return null;
        }

        public static T PreviousSibling<T>(this INode<T> node, Predicate<T> predicate) where T : class, INode<T>
        {
            if (node != null)
                for (var n = node.PreviousSibling; n != null; n = n.PreviousSibling)
                    if (predicate(n))
                        return n;
            return null;
        }

        public static T NearestAncestor<T>(this INode<T> node, Predicate<T> predicate) where T : class, INode<T>
        {
            if (node != null)
                for (var n = node.ParentNode; n != null; n = n.ParentNode)
                    if (predicate(n))
                        return n;
            return null;
        }

        public static bool IsFirstChild<T>(this INode<T> node, INode<T> child) where T : class, INode<T> =>
            node != null && node.FirstChild == child;

        public static bool IsFirstChild<T>(this INode<T> node) where T : class, INode<T>
        {
            var parent = node?.ParentNode;
            return parent != null && parent.FirstChild == node;
        }

        public static bool IsLastChild<T>(this INode<T> node, INode<T> child) where T : class, INode<T> =>
            node != null && node.LastChild == child;

        public static bool IsLastChild<T>(this INode<T> node) where T : class, INode<T>
        {
            var parent = node?.ParentNode;
            return parent != null && parent.LastChild == node;
        }

        public static T FirstDescendant<T>(this INode<T> node, Predicate<T> predicate) where T : class, INode<T> =>
            node.EnumerateSubtree().FirstOrDefault(p => predicate(p));

        public static bool HasChildNodes<T>(this INode<T> node) where T : class, INode<T> => node?.FirstChild != null;

        public static bool HasGrandChildNodes<T>(this INode<T> node) where T : class, INode<T> =>
            node != null && node.EnumerateChildren().Any(n => n.FirstChild != null);

        #endregion

        #region Linked Objects

        public static IEnumerable<T> Before<T>(this ILinked<T> node) where T : class, ILinked<T>
        {
            var prev = node.Previous;
            while (prev != null)
            {
                yield return prev;
                prev = prev.Previous;
            }
        }

        public static IEnumerable<T> After<T>(this ILinked<T> node) where T : class, ILinked<T>
        {
            var next = node.Next;
            while (next != null)
            {
                yield return next;
                next = next.Next;
            }
        }

        public static T First<T>(this ILinked<T> node) where T : class, ILinked<T>
        {
            while (true)
            {
                var prev = node.Previous;
                if (prev == null) return (T) node;
                node = prev;
            }
        }

        public static T Last<T>(this ILinked<T> node) where T : class, ILinked<T>
        {
            while (true)
            {
                var next = node.Next;
                if (next == null) return (T) node;
                node = next;
            }
        }

        #endregion

        #region LinkedList

        public static LinkedListNode<TSource> LastOrDefault<TSource>(this LinkedList<TSource> source,
            Func<TSource, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            var node = source.Last;
            while (node != null)
            {
                if (predicate(node.Value))
                    return node;
                node = node.Previous;
            }

            return null;
        }

        public static LinkedListNode<TSource> FirstOrDefault<TSource>(this LinkedList<TSource> source,
            Func<TSource, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            var node = source.First;
            while (node != null)
            {
                if (predicate(node.Value))
                    return node;
                node = node.Next;
            }

            return null;
        }

        public static LinkedListNode<TSource> LastOrDefault<TSource>(this LinkedList<TSource> source,
            Func<TSource, bool> predicate, out int indexFromEnd)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            var node = source.Last;
            indexFromEnd = 0;
            while (node != null)
            {
                if (predicate(node.Value))
                    return node;
                indexFromEnd++;
                node = node.Previous;
            }

            return null;
        }

        public static LinkedListNode<TSource> FirstOrDefault<TSource>(this LinkedList<TSource> source,
            Func<TSource, bool> predicate, out int indexFromStart)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            var node = source.First;
            indexFromStart = 0;
            while (node != null)
            {
                if (predicate(node.Value))
                    return node;
                indexFromStart++;
                node = node.Next;
            }

            return null;
        }

        public static void AddFirst<T>(this LinkedList<T> list, IEnumerable<T> range)
        {
            if (list != null)
                foreach (var vValue in range)
                    list.AddFirst(vValue);
        }

        public static void AddLast<T>(this LinkedList<T> list, IEnumerable<T> range)
        {
            if (list != null)
                foreach (var vValue in range)
                    list.AddLast(vValue);
        }

        public static T PopFirst<T>(this LinkedList<T> list)
        {
            var item = list.First;
            if (item == null) return default(T);
            list.Remove(item);
            return item.Value;
        }

        public static T PopLast<T>(this LinkedList<T> list)
        {
            var item = list.Last;
            if (item == null) return default(T);
            list.Remove(item);
            return item.Value;
        }

        public static void RemoveAll<T>(this LinkedList<T> linkedList, Func<T, bool> predicate)
        {
            for (var node = linkedList.First; node != null;)
            {
                var next = node.Next;
                if (predicate(node.Value)) linkedList.Remove(node);
                node = next;
            }
        }

        #endregion

        #region Enumerables

        public static void ForEach<T>(this IEnumerable<T> e, Action<T> a)
        {
            foreach (var i in e)
                a(i);
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T value, IEqualityComparer<T> comparer = null)
        {
            var index = 0;
            if (comparer == null) comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }

            return -1;
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> source)
        {
            using (var it = source.GetEnumerator())
            {
                bool hasRemainingItems;
                var isFirst = true;
                var item = default(T);
                do
                {
                    hasRemainingItems = it.MoveNext();
                    if (!hasRemainingItems) continue;
                    if (!isFirst) yield return item;
                    item = it.Current;
                    isFirst = false;
                } while (hasRemainingItems);
            }
        }

        /// <summary>
        /// Returns any duplicated values from the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="valueSelector">The value selector.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IEnumerable<T> Duplicates<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));
            return source.GroupBy(valueSelector)
                .Where(group => group.Count() > 1)
                .SelectMany(t => t);
        }

        public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> a)
        {
            var set = new HashSet<T>();
            foreach (var i in a)
                if (set.Contains(i))
                    yield return i;
                else
                    set.Add(i);
        }

        public static ISet<T> ToSet<T>(this IEnumerable<T> iter)
        {
            return new HashSet<T>(iter);
        }

        public static ISet<T> ToSet<T>(this IEnumerable<T> iter, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(iter, comparer);
        }

        public static ISet<TK> ToSet<T, TK>(this IEnumerable<T> iter, Func<T, TK> keySelector)
        {
            return new HashSet<TK>(iter.Select(keySelector));
        }

        public static ISet<TK> ToSet<T, TK>(this IEnumerable<T> iter, Func<T, TK> keySelector,
            IEqualityComparer<TK> comparer)
        {
            return new HashSet<TK>(iter.Select(keySelector), comparer);
        }


        /// <summary>
        /// Casts the enumerable to an array if it is already an array.  Otherwise call ToList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static List<T> AsList<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source as List<T> ?? source.ToList();
        }

        /// <summary>
        /// Casts the enumerable to an array if it is already an array.  Otherwise call ToArray
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static T[] AsArray<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source as T[] ?? source.ToArray();
        }

        public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> list)
        {
            return list as IReadOnlyList<T> ?? new List<T>(list).AsReadOnly();
        }

        public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> list)
        {
            return list as IReadOnlyCollection<T> ?? new List<T>(list).AsReadOnly();
        }

        public static IReadOnlySet<T> AsReadOnlySet<T>(this IEnumerable<T> list)
        {
            return list as IReadOnlySet<T> ?? new ReadOnlySet<T>(list);
        }

        public static IReadOnlySet<T> AsReadOnlySet<T>(this IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            return list as IReadOnlySet<T> ?? new ReadOnlySet<T>(list, comparer);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> e) where T : class
        {
            return e.Where(i => i != null);
        }

        public static IEnumerable<T> WhereNotIn<T>(this IEnumerable<T> e, IEnumerable<T> set)
        {
            if (set is ISet<T> realSet) return e.Where(i => !realSet.Contains(i));
            if (set is IContains<T> realContains) return e.Where(i => !realContains.Contains(i));
            return e.Where(i => !set.Contains(i));
        }

        public static IEnumerable<T> WhereIn<T>(this IEnumerable<T> e, IEnumerable<T> set)
        {
            if (set is ISet<T> realSet) return e.Where(i => realSet.Contains(i));
            if (set is IContains<T> realContains) return e.Where(i => realContains.Contains(i));
            return e.Where(set.Contains);
        }

        public static void Clear<T>(this IProducerConsumerCollection<T> collection)
        {
            while (collection.TryTake(out _))
                ;
        }

        #endregion

        #region Lists

        public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            comparer = comparer ?? Comparer<T>.Default;

            var lower = 0;
            var upper = list.Count - 1;

            while (lower <= upper)
            {
                var middle = lower + (upper - lower) / 2;
                var comparisonResult = comparer.Compare(value, list[middle]);
                if (comparisonResult == 0)
                    return middle;
                if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return ~lower;
        }

        public static IEnumerable<T[]> PartitionBy<T>(this IReadOnlyList<T> source, int size)
        {
            for (var i = 0; i < Math.Ceiling(source.Count / (double) size); i++)
                yield return source.Skip(size * i).Take(size).ToArray();
        }

        public static IEnumerable<T[]> PartitionInto<T>(this IReadOnlyList<T> source, int partitions)
        {
            var vCount = (int) Math.Ceiling(source.Count / (double) partitions);
            var i = 0;
            while (i < partitions)
            {
                var vResult = source.Skip(vCount * i).Take(vCount).ToArray();
                if (vResult.Length > 0) yield return vResult;
                else yield break;
                i++;
            }
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> range)
        {
            if (list != null && range != null)
                foreach (var value in range)
                    list.Add(value);
        }

        #endregion

        #region Sets

        public static void AddRange<T>(this ISet<T> set, IEnumerable<T> range)
        {
            if (set != null && range!=null)
                foreach (var value in range)
                    set.Add(value);
        }
        
        #endregion
        public static async Task<T> WithYield<T>(this Task<T> task)
        {
            var result = await task.ConfigureAwait(false);
            await Task.Yield();
            return result;
        }
    }
}