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

            if (parallelismLevel == 1)
            {
                throw new Exception("Cannot produce graph unless no parallelism is specified.");
            }

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

            Events.Add(new Event<Board>(null, root, EventType.Enqueue));

            while ((openCount = open.Count) != 0)
            {
                var node = open.Dequeue();
                Events.Add(new Event<Board>(node, EventType.Dequeue));

                if (node.IsSolved || _goalNode is not null)
                {
                    Finalize(node);
                    Events.Add(new Event<Board>(node, EventType.Goal));
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
                        Events.Add(new Event<Board>(node, next, EventType.CloseExist));
                        continue;
                    }

                    next.ComputeCost();

                    switch (open.TryGetValue(next, out var existing))
                    {
                        case true when next.CompareTo(existing) < 0:
                            open.Replace(existing, next);
                            Events.Add(new Event<Board>(node, next, EventType.ReplaceAdd));
                            Events.Add(new Event<Board>(existing, EventType.ReplaceRemove));
                            break;
                        case false:
                            open.Enqueue(next);
                            Events.Add(new Event<Board>(node, next, EventType.Enqueue));
                            break;
                        default:
                            Events.Add(new Event<Board>(node, next, EventType.OpenExist));
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
                }
            }
        }
    }
}