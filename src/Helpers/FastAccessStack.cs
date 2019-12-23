using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FreeCellSolver
{
    /// <summary>
    /// This implementation lacks many error checks but good enough for our purpose.
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

        public T Peek()
        {
            var size = _size - 1;
            T[] array = _array;

            if ((uint)size >= (uint)array.Length)
            {
                ThrowForEmptyStack();
            }

            return array[size];
        }

        public void Push(T item)
        {
            var size = _size;
            T[] array = _array;

            if ((uint)size < (uint)array.Length)
            {
                array[size] = item;
                _size = size + 1;
            }
        }

        public T Pop()
        {
            var size = _size - 1;
            T[] array = _array;

            if ((uint)size >= (uint)array.Length)
            {
                ThrowForEmptyStack();
            }

            _size = size;
            T item = array[size];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                array[size] = default!;
            }
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

        private void ThrowForEmptyStack()
        {
            Debug.Assert(_size == 0);
            throw new InvalidOperationException("The stack is empty.");
        }
    }
}