using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace FreeCellSolver.Solvers
{
    public class AStar
    {
        private static ConcurrentDictionary<int, byte> _closed;

        private readonly Board _board;
        private readonly int _maxDepth;

        public Board SolvedBoard { get; private set; }
        public int SolvedFromId { get; private set; }
        public int VisitedNodes => _closed.Count;

        public AStar(Board board, int maxDepth)
            => (_board, _maxDepth) = (board, maxDepth);

        public static AStar Run(Board board)
        {
            Console.WriteLine($"Solver: A*");

            const int maxDepth = 200;

            var clone = board.Clone();
            clone.AutoPlay();

            // Should obviously use a local HashSet<int> here but we don't care much about this
            // non parallel version, its only here for debugging.
            _closed = new ConcurrentDictionary<int, byte>(1, 1000);

            var astar = new AStar(clone, maxDepth);
            astar.Search(board, 0);
            return astar;
        }

        public static async Task<AStar> RunParallelAsync(Board board)
        {
            const int maxDepth = 200;

            var clone = board.Clone();
            clone.AutoPlay();

            var states = ParallelHelper.GetStates(clone, Environment.ProcessorCount);
            Console.WriteLine($"Solver: A* - using {states.Count} cores");

            _closed = new ConcurrentDictionary<int, byte>(states.Count, 1000);
            var astar = new AStar(board, maxDepth);

            var tasks = states.Select((b, i) => Task.Run(() => astar.Search(b, i)));
            await Task.WhenAll(tasks);
            return astar;
        }

        private void Search(Board root, int stateId)
        {
            var open = new StateCostTree();

            open.Add(root);

            while (open.Count != 0)
            {
                var board = open.RemoveMin();

                if (board.IsSolved || SolvedBoard != null)
                {
                    Finalize(board, stateId);
                    break;
                }

                _closed.AddOrUpdate(board.GetHashCode(), (byte)1, (k, v) => (byte)1);

                if (board.MoveCount > _maxDepth)
                {
                    continue;
                }

                foreach (var move in board.GetValidMoves(out _))
                {
                    var next = board.Clone();
                    next.ExecuteMove(move, board);

                    if (_closed.ContainsKey(next.GetHashCode()))
                    {
                        continue;
                    }

                    next.ComputeCost(false);

                    var existing = open.GetValue(next);
                    if (existing == null || next.Cost < existing.Cost)
                    {
                        if (existing != null)
                        {
                            open.Remove(existing);
                        }

                        open.Add(next);
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