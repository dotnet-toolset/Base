using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Collections.Impl
{
    public class IndexableStack<T> : IIndexableStack<T>
    {
        private readonly LinkedList<T> _list;

        public bool IsEmpty => _list.Last == null;
        public int Count => _list.Count;

        public IndexableStack()
        {
            _list = new LinkedList<T>();
        }

        public IndexableStack(IEnumerable<T> aEnumerable)
        {
            _list = new LinkedList<T>(aEnumerable);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public void Push(T aItem)
        {
            _list.AddLast(aItem);
        }

        public T Pop()
        {
            var vLast = _list.Last;
            if (vLast == null) throw new IndexOutOfRangeException();
            _list.Remove(vLast);
            return vLast.Value;
        }

        public T Peek(bool aReturnDefault = false)
        {
            var vLast = _list.Last;
            if (vLast == null)
                if (aReturnDefault) return default;
                else throw new IndexOutOfRangeException();
            return vLast.Value;
        }

        public T Peek(int aIndex, bool aReturnDefault = false)
        {
            var vLast = _list.Last;
            do
            {
                if (vLast == null)
                    if (aReturnDefault) return default;
                    else throw new IndexOutOfRangeException();
                if (aIndex-- <= 0) break;
                vLast = vLast.Previous;
            } while (true);
            return vLast.Value;
        }


        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
}
