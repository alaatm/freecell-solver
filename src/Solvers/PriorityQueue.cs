using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FreeCellSolver.Solvers
{
    // Adapted from .net 5 implementation
    public sealed class PriorityQueue<T> where T : IComparable<T>, IEquatable<T>
    {
        private const int Arity = 4;
        private const int Log2Arity = 2;
        private const int MinCapacity = 4;

        private readonly HashSet<T> _hash;

        private T[] _nodes;
        private int _size;

        public int Count => _size;

        public PriorityQueue()
        {
            _nodes = new T[MinCapacity];
            _hash = new(MinCapacity);
        }

        public PriorityQueue(int initialCapacity)
        {
            Debug.Assert(initialCapacity > 0);

            _nodes = new T[Math.Max(initialCapacity, MinCapacity)];
            _hash = new(Math.Max(initialCapacity, MinCapacity));
        }

        public void Enqueue(T element)
        {
            Debug.Assert(!_hash.Contains(element));

            var currentSize = _size++;

            if (_nodes.Length == currentSize)
            {
                Array.Resize(ref _nodes, currentSize * 2);
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

        public void Replace(T existing, T replacement)
        {
            // Replace is exclusively used to replace one element with another equal (state) but better (cost) element
            Debug.Assert(_hash.Contains(existing));
            Debug.Assert(existing.GetHashCode() == replacement.GetHashCode() && existing.Equals(replacement) && !ReferenceEquals(existing, replacement));
            Debug.Assert(existing.CompareTo(replacement) > 0);

            _hash.Remove(existing);
            _hash.Add(replacement);
            var index = FindIndex(existing);

            if (index != 0 && replacement.CompareTo(_nodes[GetParentIndex(index)]) < 0)
            {
                MoveUp(replacement, index);
            }
            else
            {
                MoveDown(replacement, index);
            }
        }

        public bool TryGetValue(T equalValue, out T actualValue)
            => _hash.TryGetValue(equalValue, out actualValue);

        public void Clear()
        {
            if (_size > 0)
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    Array.Clear(_nodes, 0, _size);
                }

                _hash.Clear();
                _size = 0;
            }
        }

        private void RemoveRootNode()
        {
            var lastNodeIndex = --_size;
            if (lastNodeIndex > 0)
            {
                var lastNode = _nodes[lastNodeIndex];
                MoveDown(lastNode, 0);
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _nodes[lastNodeIndex] = default;
            }
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

        private int FindIndex(T element)
        {
            var index = 0;
            var hc = element.GetHashCode();
            ref var ptr = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_nodes), index);

            while (hc != ptr.GetHashCode() || !element.Equals(ptr))
            {
                ptr = ref Unsafe.Add(ref ptr, 1);
                index++;
            }

            Debug.Assert(index < _size);
            return index;
        }
    }
}