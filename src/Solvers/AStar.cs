using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using FreeCellSolver.Game;
using FreeCellSolver.Solvers.Visualizers;

namespace FreeCellSolver.Solvers
{
    public sealed class AStar
    {
        private static readonly object _syncLock = new();

        private ConcurrentDictionary<Board, byte> _closed;
        private ManualResetEventSlim _mres;
        private Board _goalNode;
        private int _parallelismLevel;
        private int _threadCount;

        public static readonly List<Event<Board>> Events = new();

        public static Result Run(Board root) => Run(root, Environment.ProcessorCount);

        public static Result Run(Board root, int parallelismLevel)
        {
            Debug.Assert(parallelismLevel > 0);
            return new AStar().RunCore(root, parallelismLevel);
        }

        private Result RunCore(Board root, int parallelismLevel)
        {
            var clone = root.Clone();
            clone.RootAutoPlay();

            _threadCount = 1;
            _parallelismLevel = parallelismLevel;
            _closed = new(parallelismLevel, 1000);

            _mres = new(false);
            ThreadPool.UnsafeQueueUserWorkItem(_ => Search(clone), null);
            _mres.Wait();

            return new Result
            {
                GoalNode = _goalNode,
                VisitedNodes = _closed.Count,
                Threads = parallelismLevel,
            };
        }

        private void Search(Board root)
        {
            var closed = _closed;
            var open = new PriorityQueue<Board>();
            open.Enqueue(root);
            int openCount;

            lock (_syncLock) { Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, null, root, EventType.Enqueue)); }

            while ((openCount = open.Count) != 0)
            {
                var node = open.Dequeue();
                lock (_syncLock) { Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, node, EventType.Dequeue)); }

                if (node.IsSolved || _goalNode is not null)
                {
                    Finalize(node);
                    break;
                }

                // Check that we have at least 1 node besides the dequeued one in open set
                // to prevent infinite loop when there is no solution or on initial root search
                if (openCount > 1 && QueueWorkItem(node))
                {
                    continue;
                }

                closed.TryAdd(node, 1);

                foreach (var move in node.GetValidMoves())
                {
                    var next = node.ExecuteMove(move);

                    if (closed.ContainsKey(next))
                    {
                        lock (_syncLock) { Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, node, next, EventType.CloseExist)); }
                        continue;
                    }

                    next.ComputeCost();

                    switch (open.TryGetValue(next, out var existing))
                    {
                        case true when next.CompareTo(existing) < 0:
                            open.Replace(existing, next);
                            lock (_syncLock)
                            {
                                Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, node, next, EventType.ReplaceAdd));
                                Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, existing, EventType.ReplaceRemove));
                            }
                            break;
                        case false:
                            open.Enqueue(next);
                            lock (_syncLock) { Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, node, next, EventType.Enqueue)); }
                            break;
                        default:
                            lock (_syncLock) { Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, node, next, EventType.OpenExist)); }
                            break;
                    }
                }
            }

            if (Interlocked.Decrement(ref _threadCount) == 0)
            {
                _mres.Set();
            }
        }

        private bool QueueWorkItem(Board root)
        {
            int newThreadCount;
            int currentThreadCount;
            do
            {
                currentThreadCount = _threadCount;
                newThreadCount = currentThreadCount + 1;

                if (currentThreadCount >= _parallelismLevel)
                {
                    return false;
                }
            } while (Interlocked.CompareExchange(ref _threadCount, newThreadCount, currentThreadCount) != currentThreadCount);
            ThreadPool.UnsafeQueueUserWorkItem(_ => Search(root), null);
            return true;
        }

        private void Finalize(Board node)
        {
            lock (_syncLock)
            {
                if (_goalNode is null || (node.IsSolved && node.MoveCount < _goalNode.MoveCount))
                {
                    _goalNode = node;
                    Events.Add(new Event<Board>(Thread.CurrentThread.ManagedThreadId, node, EventType.Goal));
                }
            }
        }
    }
}