using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    public static class AStar
    {
        private static readonly object _syncLock = new();

        private static ConcurrentDictionary<Board, byte> _closed;
        private static ManualResetEventSlim _mres;
        private static Board _goalNode;
        private static int _parallelismLevel;
        private static int _threadCount = 0;

        public static Result Run(Board root) => Run(root, Environment.ProcessorCount);

        public static Result Run(Board root, int parallelismLevel)
        {
            Debug.Assert(parallelismLevel > 0);

            var clone = root.Clone();
            clone.RootAutoPlay();

            _closed = new(parallelismLevel, 1000);
            _mres = new(false);

            _threadCount = _parallelismLevel = parallelismLevel;

            var nodes = ParallelHelper.GetNodes(clone, parallelismLevel);
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                ThreadPool.UnsafeQueueUserWorkItem(_ => Search(node), null);
            }

            _mres.Wait();

            return new Result
            {
                GoalNode = _goalNode,
                VisitedNodes = _closed.Count,
                Threads = parallelismLevel,
            };
        }

        internal static void Reset()
        {
            _goalNode = null;

            _closed.Clear();
            _closed = null;
            _mres.Dispose();
            _mres = null;
            GC.Collect();
        }

        private static void Search(Board root)
        {
            var closed = _closed;
            var open = new PriorityQueue<Board>();
            open.Enqueue(root);

            while (open.Count != 0)
            {
                var node = open.Dequeue();

                if (node.IsSolved || _goalNode is not null)
                {
                    Finalize(node);
                    break;
                }

                closed.TryAdd(node, 1);

                foreach (var move in node.GetValidMoves())
                {
                    var next = node.ExecuteMove(move);

                    if (closed.ContainsKey(next))
                    {
                        continue;
                    }

                    next.ComputeCost();

                    if (!QueueWorkItem(next))
                    {
                        var found = open.TryGetValue(next, out var existing);
                        if (found && next.CompareTo(existing) < 0)
                        {
                            open.Replace(existing, next);
                        }
                        else if (!found)
                        {
                            open.Enqueue(next);
                        }
                    }
                }
            }

            if (Interlocked.Decrement(ref _threadCount) == 0)
            {
                _mres.Set();
            }
        }

        private static bool QueueWorkItem(Board root)
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

        private static void Finalize(Board node)
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