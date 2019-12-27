using System;
using System.Linq;
using System.Collections.Generic;

namespace FreeCellSolver
{
    /// <summary>
    /// This implementation has no error checks but good enough for our purpose.
    /// </summary>
    public class FastAccessStack
    {
        private static int _capacity = 19;

        private int[] _array = new int[_capacity];
        private int _size;

        public int Size => _size;

        public int this[int index] => _array[_size - index - 1];

        public int Peek() => _array[_size - 1];

        public void Push(int item)
        {
            _array[_size] = item;
            _size++;
        }

        public int Pop()
        {
            _size--;
            int item = _array[_size];

            return item;
        }

        public bool SequenceEqual(FastAccessStack other)
        {
            if (_size != other._size)
            {
                return false;
            }

            for (var i = 0; i < _size; i++)
            {
                if (_array[i] != other._array[i])
                {
                    return false;
                }
            }

            return true;
        }

        public FastAccessStack Clone()
        {
            const int INT_SIZE = 4;

            var clone = new FastAccessStack();
            Buffer.BlockCopy(_array, 0, clone._array, 0, _capacity * INT_SIZE);
            clone._size = _size;
            return clone;
        }

        internal IEnumerable<int> All() => _array.Take(_size);
    }
}