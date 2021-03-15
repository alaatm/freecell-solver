using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace FreeCellSolver.Solvers
{
    public class PriorityQueue<T> where T : IComparable<T>, IEquatable<T>
    {
        private const int Arity = 4;
        private const int Log2Arity = 2;

        private readonly HashSet<T> _hash;

        private T[] _nodes;
        private int _size;

        public int Count => _size;

        public PriorityQueue()
        {
            _nodes = Array.Empty<T>();
            _hash = new();
        }

        public PriorityQueue(int initialCapacity)
        {
            Debug.Assert(initialCapacity > 0);

            _nodes = new T[initialCapacity];
            _hash = new(initialCapacity);
        }

        public void Enqueue(T element)
        {
            Debug.Assert(!_hash.Contains(element));

            var currentSize = _size++;

            if (_nodes.Length == currentSize)
            {
                Grow(currentSize + 1);
            }

            _hash.Add(element);
            MoveUp(element, currentSize);
        }

        public T Dequeue()
        {
            Debug.Assert(_size > 0);

            var element = _nodes[0];
            _hash.Remove(element);
            RemoveRootNode();
            return element;
        }

        public void Remove(T element)
        {
            Debug.Assert(_hash.Contains(element));

            _hash.Remove(element);
            var index = _nodes.AsSpan().IndexOf(element);

            _size--;
            if (index < _size)
            {
                Unsafe.CopyBlock(
                    ref Unsafe.As<T, byte>(ref _nodes[index]),
                    ref Unsafe.As<T, byte>(ref _nodes[index + 1]),
                    (uint)(Unsafe.SizeOf<T>() * (_size - index)));
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _nodes[_size] = default;
            }
        }

        public bool TryGetValue(T equalValue, out T actualValue) => _hash.TryGetValue(equalValue, out actualValue);

        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_nodes, 0, _size);
            }
            _size = 0;
        }

        private void Grow(int minCapacity)
        {
            Debug.Assert(_nodes.Length < minCapacity);

            const int MaxArrayLength = 0X7FEFFFFF;
            const int GrowFactor = 2;
            const int MinimumGrow = 4;

            var newCapacity = GrowFactor * _nodes.Length;

            if ((uint)newCapacity > MaxArrayLength)
            {
                newCapacity = MaxArrayLength;
            }

            newCapacity = Math.Max(newCapacity, _nodes.Length + MinimumGrow);

            if (newCapacity < minCapacity)
            {
                newCapacity = minCapacity;
            }

            Array.Resize(ref _nodes, newCapacity);
        }

        private void RemoveRootNode()
        {
            var lastNodeIndex = _size - 1;
            var lastNode = _nodes[lastNodeIndex];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _nodes[lastNodeIndex] = default;
            }

            _size--;

            MoveDown(lastNode, 0);
        }

        private static int GetParentIndex(int index) => (index - 1) >> Log2Arity;

        private static int GetFirstChildIndex(int index) => (index << Log2Arity) + 1;

        private void MoveUp(T node, int nodeIndex)
        {
            var nodes = _nodes;

            while (nodeIndex > 0)
            {
                var parentIndex = GetParentIndex(nodeIndex);
                var parent = nodes[parentIndex];

                if (node.CompareTo(parent) < 0)
                {
                    nodes[nodeIndex] = parent;
                    nodeIndex = parentIndex;
                }
                else
                {
                    break;
                }
            }

            nodes[nodeIndex] = node;
        }

        private void MoveDown(T node, int nodeIndex)
        {
            var nodes = _nodes;
            var size = _size;

            int i;
            while ((i = GetFirstChildIndex(nodeIndex)) < size)
            {
                var minChild = nodes[i];
                var minChildIndex = i;

                var childIndexUpperBound = Math.Min(i + Arity, size);
                while (++i < childIndexUpperBound)
                {
                    var nextChild = nodes[i];
                    if (nextChild.CompareTo(minChild) < 0)
                    {
                        minChild = nextChild;
                        minChildIndex = i;
                    }
                }

                if (node.CompareTo(minChild) <= 0)
                {
                    break;
                }

                nodes[nodeIndex] = minChild;
                nodeIndex = minChildIndex;
            }

            nodes[nodeIndex] = node;
        }
    }
}