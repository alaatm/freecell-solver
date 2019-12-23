namespace FreeCellSolver
{
    /// <summary>
    /// This implementation has no error checks but good enough for our purpose.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FastAccessStack<T>
    {
        private int _initialCapacity = 0;

        private T[] _array;
        private int _size;

        public int Size => _size;

        public T this[int index] => _array[_size - index - 1];

        public FastAccessStack(int capacity)
        {
            _initialCapacity = capacity;
            _array = new T[capacity];
        }

        public T Peek() => _array[_size - 1];

        public void Push(T item)
        {
            _array[_size] = item;
            _size++;
        }

        public T Pop()
        {
            _size--;
            T item = _array[_size];
            _array[_size] = default;

            return item;
        }

        public bool SequenceEqual(FastAccessStack<T> other)
        {
            if (_size != other._size)
            {
                return false;
            }

            for (var i = 0; i < _size; i++)
            {
                if (!_array[i].Equals(other._array[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public FastAccessStack<T> Clone()
        {
            var clone = new FastAccessStack<T>(_initialCapacity);
            _array.CopyTo(clone._array, 0);
            clone._size = _size;
            return clone;
        }
    }
}