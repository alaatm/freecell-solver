using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    public class AStar
    {
        private static ConcurrentDictionary<int, byte> _closed;

        public Board SolvedBoard { get; private set; }
        public int SolvedFromId { get; private set; }
        public int VisitedNodes => _closed.Count;

        public static AStar Run(Board board, bool best)
        {
            Console.WriteLine($"Solver: A*");

            var clone = board.Clone();
            clone.AutoPlay();

            // Should obviously use a local HashSet<int> here but we don't care much about this
            // non parallel version, its only here for debugging.
            _closed = new ConcurrentDictionary<int, byte>(1, 1000);

            var astar = new AStar();
            if (best)
            {
                astar.SearchBest(clone, 0);
            }
            else
            {
                astar.SearchFast(clone, 0);
            }
            return astar;
        }

        public static async Task<AStar> RunParallelAsync(Board board, bool best)
        {
            var clone = board.Clone();
            clone.AutoPlay();

            var states = ParallelHelper.GetStates(clone, Environment.ProcessorCount);
            Console.WriteLine($"Solver: A* - using {states.Count} cores");

            _closed = new ConcurrentDictionary<int, byte>(states.Count, 1000);
            var astar = new AStar();

            var tasks = states.Select((b, i) => Task.Run(() =>
            {
                if (best)
                {
                    astar.SearchBest(b, i);
                }
                else
                {
                    astar.SearchFast(b, i);
                }
            }));
            await Task.WhenAll(tasks);
            return astar;
        }

        private void SearchFast(Board root, int stateId)
        {
            var open = new PriorityQueue();
            open.Enqueue(root);

            while (open.Count != 0)
            {
                var board = open.Dequeue();

                if (board.IsSolved || SolvedBoard != null)
                {
                    Finalize(board, stateId);
                    break;
                }

                _closed.TryAdd(board.GetHashCode(), 1);

                foreach (var move in board.GetValidMoves())
                {
                    var next = board.Clone();
                    next.ExecuteMove(move, board);

                    if (_closed.ContainsKey(next.GetHashCode()))
                    {
                        continue;
                    }

                    next.ComputeCost(false);

                    if (!open.TryGetValue(next, out _))
                    {
                        open.Enqueue(next);
                    }
                }
            }
        }

        private void SearchBest(Board root, int stateId)
        {
            var open = new DynamicPriorityQueue();
            open.Enqueue(root);

            while (open.Count != 0)
            {
                var board = open.Dequeue();

                if (board.IsSolved || SolvedBoard != null)
                {
                    Finalize(board, stateId);
                    break;
                }

                _closed.TryAdd(board.GetHashCode(), 1);

                foreach (var move in board.GetValidMoves())
                {
                    var next = board.Clone();
                    next.ExecuteMove(move, board);

                    if (_closed.ContainsKey(next.GetHashCode()))
                    {
                        continue;
                    }

                    next.ComputeCost(true);

                    var found = open.TryGetValue(next, out var existing);
                    if (found && next.Cost < existing.Cost)
                    {
                        open.Remove(existing);
                        open.Enqueue(next);
                    }
                    else if (!found)
                    {
                        open.Enqueue(next);
                    }
                }
            }
        }

        private void Finalize(Board board, int stateId)
        {
            lock (this)
            {
                if (SolvedBoard == null)
                {
                    SolvedBoard = board;
                    SolvedFromId = stateId;
                }
            }
        }
    }
}