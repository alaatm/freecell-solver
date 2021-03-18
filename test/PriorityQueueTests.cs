using System;
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
        public void Remove_removes_element()
        {
            // Arrange
            var pq = new PriorityQueue<Node>();
            var node1 = new Node("1", 3);
            var node2 = new Node("2", 3);
            var node3 = new Node("3", 1);
            var node4 = new Node("4", 1);
            var node5 = new Node("5", 4);
            var node6 = new Node("6", 4);

            pq.Enqueue(node1);
            pq.Enqueue(node2);
            pq.Enqueue(node3);
            pq.Enqueue(node4);
            pq.Enqueue(node5);
            pq.Enqueue(node6);

            // Act
            pq.Remove(node1);
            pq.Remove(node4);
            pq.Remove(node6);

            // Assert
            Assert.Equal(3, pq.Count);
            Assert.Same(node3, pq.Dequeue());
            Assert.Same(node2, pq.Dequeue());
            Assert.Same(node5, pq.Dequeue());
        }

        [Fact]
        public void TryGetValue_returns_false_when_element_not_found1()
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
        public void TryGetValue_returns_false_when_element_not_found2()
        {
            // Arrange
            var pq = new PriorityQueue<Node>();
            var node = new Node("1", 1);
            pq.Enqueue(node);
            pq.Remove(node);

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
