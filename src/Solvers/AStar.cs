using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FreeCellSolver.Solvers
{
    public class AStar
    {
        private readonly Board _board;
        private readonly int _maxDepth;

        public Board SolvedBoard { get; private set; }
        public int TotalVisitedNodes { get; private set; }
        public int SolvedFromId { get; private set; }

        public AStar(Board board, int maxDepth)
            => (_board, _maxDepth) = (board, maxDepth);

        public static AStar Run(Board board, int maxDepth)
        {
            var astar = new AStar(board, maxDepth);
            Console.WriteLine($"Solver: A*");

            astar.Search(board, 0);
            return astar;
        }

        public static async Task<AStar> RunParallelAsync(Board board, int maxDepth)
        {
            var astar = new AStar(board, maxDepth);
            var states = ParallelHelper.GetStates(board, Environment.ProcessorCount);
            Console.WriteLine($"Solver: A* - using {states.Count} cores");

            var tasks = states.Select((b, i) => Task.Run(() => astar.Search(b, i)));

            await Task.WhenAll(tasks);
            return astar;
        }

        private void Search(Board root, int stateId)
        {
            var open = new StateScoreTree();
            var closed = new HashSet<int>();

            root.ComputeScore();
            open.Add(root);

            while (open.Count != 0)
            {
                var board = open.Remove();

                if (board.IsSolved || SolvedBoard != null)
                {
                    Finalize(board, closed.Count, stateId);
                    break;
                }

                closed.Add(board.GetHashCode());

                if (board.Moves.Count > _maxDepth)
                {
                    continue;
                }

                var (moves, _) = board.GetValidMoves(false);

                foreach (var move in moves)
                {
                    var next = board.Clone();
                    next.ExecuteMove(move, false, false);

                    if (closed.Contains(next.GetHashCode()))
                    {
                        continue;
                    }

                    next.ComputeScore();

                    var exist = open.GetValue(next);
                    if (exist == null || next.Score < exist.Score)
                    {
                        if (exist != null)
                        {
                            open.Remove(exist);
                        }

                        open.Add(next);
                    }
                }
            }

            var x = "";
        }

        private void Finalize(Board board, int visitedCount, int stateId)
        {
            lock (this)
            {
                if (SolvedBoard == null)
                {
                    SolvedBoard = board;
                    TotalVisitedNodes = visitedCount;
                    SolvedFromId = stateId;
                }
            }
        }
    }
}