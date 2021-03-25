using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FreeCellSolver.Solvers;
using Xunit;

namespace FreeCellSolver.Test
{
    public class PriorityQueueTests
    {
        [Fact]
        public void EnqueueDequeue_respects_priority_order()
        {
            // Arrange
            var rnd = new Random();
            var pq = new PriorityQueue<Node>(500);

            // Act
            for (var i = 0; i < 500; i++)
            {
                pq.Enqueue(new Node(i.ToString(), rnd.Next(500)));
            }

            // Assert
            var min = int.MinValue;
            for (var i = 0; i < 500; i++)
            {
                var priority = pq.Dequeue().Priority;
                Assert.True(priority >= min);
                min = priority;
            }
        }

        [Fact]
        public void Removing_last_element_doesnt_keep_reference_to_it()
        {
            var pq = new PriorityQueue<Node>();
            var wr = Populate(pq);
            Assert.True(SpinWait.SpinUntil(() =>
            {
                GC.Collect();
                return !wr.TryGetTarget(out _);
            }, 2000));
            GC.KeepAlive(pq);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static WeakReference<object> Populate(PriorityQueue<Node> pq)
            {
                var node = new Node("", 0);
                pq.Enqueue(node);
                pq.Dequeue();
                return new WeakReference<object>(node);
            }
        }

        [Fact]
        public void Replace_replaces_element()
        {
            // Arrange
            var pq = new PriorityQueue<Node>();
            var node1 = new Node("1", 3);
            var node2 = new Node("2", 3);
            var node3 = new Node("3", 1);
            var node4 = new Node("4", 1);
            var node5 = new Node("5", 4);
            var node6 = new Node("6", 4);

            var replacement1 = new Node("r1", 0);
            var replacement2 = new Node("r1", 2);
            var replacement3 = new Node("r1", 5);

            pq.Enqueue(node1);
            pq.Enqueue(node2);
            pq.Enqueue(node3);
            pq.Enqueue(node4);
            pq.Enqueue(node5);
            pq.Enqueue(node6);

            // Act
            pq.Replace(node1, replacement1);
            pq.Replace(node4, replacement2);
            pq.Replace(node6, replacement3);

            // Assert
            Assert.Equal(6, pq.Count);
            Assert.Same(replacement1, pq.Dequeue());
            Assert.Same(node3, pq.Dequeue());
            Assert.Same(replacement2, pq.Dequeue());
            Assert.Same(node2, pq.Dequeue());
            Assert.Same(node5, pq.Dequeue());
            Assert.Same(replacement3, pq.Dequeue());
        }

        [Fact]
        public void Replace_maintains_heap_properties1()
        {
            // Arrange
            var size = 2500;
            var replaceSize = 350;
            var rnd = new Random();

            var hash = new HashSet<int>(replaceSize);
            var list = new List<Node>(size);
            var pq = new PriorityQueue<Node>(size);

            for (var i = 0; i < size; i++)
            {
                var node = new Node(i.ToString(), rnd.Next());
                list.Add(node);
                pq.Enqueue(node);
            }

            // Act - replace random replaceSize elements
            for (var i = 0; i < replaceSize; i++)
            {
                var index = rnd.Next(0, size);
                while (hash.Contains(index))
                {
                    index = rnd.Next(0, size);
                }

                hash.Add(index);
                pq.Replace(list[index], new Node($"r{i}", rnd.Next()));
            }

            // Assert
            var min = int.MinValue;
            for (var i = 0; i < size; i++)
            {
                var priority = pq.Dequeue().Priority;
                Assert.True(priority >= min);
                min = priority;
            }
        }

        [Fact]
        public void Replace_maintains_heap_properties2()
        {
            // Arrange
            var size = 2498;
            var rnd = new Random();

            var pq = new PriorityQueue<Node>(size);

            var minNode = new Node("min", int.MinValue);
            var maxNode1 = new Node("max1", int.MaxValue);
            var maxNode2 = new Node("max2", int.MaxValue - 1);
            var maxNode3 = new Node("max3", int.MaxValue - 2);

            pq.Enqueue(minNode);
            pq.Enqueue(maxNode1);
            pq.Enqueue(maxNode3);
            for (var i = 0; i < size; i++)
            {
                var node = new Node(i.ToString(), rnd.Next(0, int.MaxValue - 5));
                pq.Enqueue(node);
            }
            pq.Enqueue(maxNode2);

            // Act
            pq.Replace(maxNode2, new Node("r1", rnd.Next(0, int.MaxValue - 5)));
            pq.Replace(minNode, new Node("r2", rnd.Next(0, int.MaxValue - 5)));
            pq.Replace(maxNode1, new Node("r3", rnd.Next(0, int.MaxValue - 5)));
            pq.Replace(maxNode3, new Node("r4", rnd.Next(0, int.MaxValue - 5)));

            // Assert
            var min = int.MinValue;
            for (var i = 0; i < size; i++)
            {
                var priority = pq.Dequeue().Priority;
                Assert.True(priority >= min);
                min = priority;
            }
        }

        [Fact]
        public void Replace_keeps_track_of_hashes()
        {
            // Arrange
            var oldVal = new Node("old", 0);
            var newVal = new Node("new", 0);
            var pq = new PriorityQueue<Node>();
            pq.Enqueue(oldVal);
            pq.Replace(oldVal, newVal);

            // Act
            var oldFound = pq.TryGetValue(oldVal, out var actualOld);
            var newFound = pq.TryGetValue(newVal, out var actualNew);

            // Assert
            Assert.False(oldFound);
            Assert.Null(actualOld);

            Assert.True(newFound);
            Assert.Same(newVal, actualNew);
        }

        [Fact]
        public void TryGetValue_returns_false_when_element_not_found()
        {
            // Arrange
            var pq = new PriorityQueue<Node>();
            var node = new Node("1", 1);
            pq.Enqueue(node);
            pq.Dequeue();

            // Act
            var found = pq.TryGetValue(node, out var ele);

            // Assert
            Assert.False(found);
            Assert.Null(ele);
        }

        [Fact]
        public void TryGetValue_returns_true_when_element_found()
        {
            // Arrange
            var pq = new PriorityQueue<Node>();
            var node = new Node("1", 1);
            pq.Enqueue(node);

            // Act
            var found = pq.TryGetValue(node, out var ele);

            // Assert
            Assert.True(found);
            Assert.Same(node, ele);
        }

        [Fact]
        public void Clear_clears_queue()
        {
            // Arrange
            var pq = new PriorityQueue<Node>();
            pq.Enqueue(new Node("1", 1));
            Assert.Equal(1, pq.Count);

            // Act
            pq.Clear();

            // Assert
            Assert.Equal(0, pq.Count);
        }

        [DebuggerDisplay("{Priority}", Name = "{Name}")]
        class Node : IComparable<Node>, IEquatable<Node>
        {
            public string Name { get; set; }
            public int Priority { get; set; }

            public Node(string name, int priority) => (Name, Priority) = (name, priority);

            public int CompareTo(Node other) => Priority.CompareTo(other.Priority);
            public bool Equals(Node other) => ReferenceEquals(this, other);
        }
    }
}
