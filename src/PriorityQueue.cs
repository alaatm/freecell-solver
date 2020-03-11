using System;

namespace FreeCellSolver
{
    // Modified version of the .net Queue<T> class
    public class PriorityQueue<T>
    {
        private T[] _array;
        private int _head;       // The index from which to dequeue if the queue isn't empty.
        private int _tail;       // The index at which to enqueue if the queue isn't full.
        private int _size;       // Number of elements.

        private const int MinimumGrow = 4;
        private const int GrowFactor = 200;  // double each time

        // Creates a queue with room for capacity objects. The default initial
        // capacity and grow factor are used.
        public PriorityQueue() => _array = Array.Empty<T>();

        // Creates a queue with room for capacity objects. The default grow factor
        // is used.
        public PriorityQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "SR.ArgumentOutOfRange_NeedNonNegNum");
            }
            _array = new T[capacity];
        }

        public int Count => _size;

        // Adds item to the tail of the queue.
        public void Enqueue(T item)
        {
            if (_size == _array.Length)
            {
                var newcapacity = (int)((long)_array.Length * (long)GrowFactor / 100);
                if (newcapacity < _array.Length + MinimumGrow)
                {
                    newcapacity = _array.Length + MinimumGrow;
                }
                SetCapacity(newcapacity);
            }

            _array[_tail] = item;
            MoveNext(ref _tail);
            _size++;
        }

        // Removes the object at the head of the queue and returns it. If the queue
        // is empty, this method throws an
        // InvalidOperationException.
        public T Dequeue()
        {
            var head = _head;
            T[] array = _array;

            if (_size == 0)
            {
                ThrowForEmptyQueue();
            }

            T removed = array[head];

            MoveNext(ref _head);
            _size--;
            return removed;
        }

        // Removes the element at the given index. The size of the list is
        // decreased by one.
        public bool Remove(T item)
        {
            var index = Array.IndexOf(_array, item, 0, _size);
            if (index == 0)
            {
                Dequeue();
                return true;
            }
            else if (index >= 0)
            {
                if (index == _size - 1)
                {
                    MovePrev(ref _tail);
                }
                RemoveAt(index);
                return true;
            }

            return false;
        }

        // Removes the element at the given index. The size of the list is
        // decreased by one.
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
            {
                ThrowIndexOutOfRangeException();
            }
            _size--;
            if (index < _size)
            {
                Array.Copy(_array, index + 1, _array, index, _size - index);
            }
        }

        // PRIVATE Grows or shrinks the buffer to hold capacity objects. Capacity
        // must be >= _size.
        private void SetCapacity(int capacity)
        {
            T[] newarray = new T[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, newarray, 0, _size);
                }
                else
                {
                    Array.Copy(_array, _head, newarray, 0, _array.Length - _head);
                    Array.Copy(_array, 0, newarray, _array.Length - _head, _tail);
                }
            }

            _array = newarray;
            _head = 0;
            _tail = (_size == capacity) ? 0 : _size;
        }

        // Increments the index wrapping it if necessary.
        private void MoveNext(ref int index)
        {
            // It is tempting to use the remainder operator here but it is actually much slower
            // than a simple comparison and a rarely taken branch.
            // JIT produces better code than with ternary operator ?:
            var tmp = index + 1;
            if (tmp == _array.Length)
            {
                tmp = 0;
            }
            index = tmp;
        }

        // Increments the index wrapping it if necessary.
        private void MovePrev(ref int index)
        {
            // It is tempting to use the remainder operator here but it is actually much slower
            // than a simple comparison and a rarely taken branch.
            // JIT produces better code than with ternary operator ?:
            var tmp = index - 1;
            if (tmp == _array.Length)
            {
                tmp = 0;
            }
            index = tmp;
        }

        private static void ThrowForEmptyQueue() => throw new InvalidOperationException("SR.InvalidOperation_EmptyQueue");
        private static void ThrowIndexOutOfRangeException() => throw new IndexOutOfRangeException();
    }
}