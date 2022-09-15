using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    public class PointerEnumerator<T> : IEnumerable<Pointer<T>>, IEnumerator<Pointer<T>>
    {
        public PointerEnumerator(Pointer<T> begin, int count) : this(begin, begin + count)
        {
        }
        public PointerEnumerator(Pointer<T> begin, Pointer<T> end)
        {
            _begin = begin;
            _end = end;
            Reset();
        }

        public int Count => (int)(_end - _begin);
        public Pointer<T> Current => _cur;

        public IEnumerator<Pointer<T>> GetEnumerator() => this;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            _cur += 1;
            return _cur != _end;
        }

        public void Reset()
        {
            _cur = _begin - 1;
        }

        object IEnumerator.Current => Current;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Pointer<T> _begin;
        private Pointer<T> _end;
        private Pointer<T> _cur;
    }
}
