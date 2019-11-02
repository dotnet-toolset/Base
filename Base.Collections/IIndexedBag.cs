using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Collections
{
    /// <summary>
    /// Collection of items that maintains unique, uniformly incrementing index for each item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIndexedBag<T>
    {
        int Count { get; }
        int Put(T item);
        int Get(T item);
        IList<T> ToList();
        IList<T> ToList(int start);
        T[] ToArray();
        T[] ToArray(int start);
    }
}
