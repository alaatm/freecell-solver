using System;
using System.Linq;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class Solver
    {
        private const int _maxDepth = 200;
        private Func<Board, bool> _solvedCondition;

        public Board SolvedBoard { get; private set; }

        public static Board Solve(Board board) => Solve(board, b => b.IsSolved);

        public static Board Solve(Board board, Func<Board, bool> solvedCondition)
        {
            var solver = new Solver(solvedCondition);
            solver.SolveCore(board, 0, 0, new HashSet<int>());
            return solver.SolvedBoard;
        }

        public Solver(Func<Board, bool> solvedCondition) => _solvedCondition = solvedCondition;

        internal int SolveCore(Board board, int depth, int movesSinceFoundation, HashSet<int> visited)
        {
            visited.Add(board.GetHashCode());

            if (SolvedBoard != null)
            {
                return 0;
            }

            if (depth > _maxDepth)
            {
                return _maxDepth;
            }

            if (_solvedCondition(board))
            {
                SolvedBoard = board;
                return 0;
            }

            var (moves, foundFoundation) = board.GetValidMoves(true);

            movesSinceFoundation = foundFoundation ? 0 : ++movesSinceFoundation;

            var scale = 1f;
            if (movesSinceFoundation >= 18)
            {
                scale = 0.7f;
            }

            var jumpDepth = -1;

            if (scale == 1)
            {
                var addedBoards = new List<Board>();
                foreach (var move in moves)
                {
                    var next = board.Clone();
                    next.ExecuteMove(move, true);
                    addedBoards.Add(next);
                }

                foreach (var b in addedBoards.OrderByDescending(p => p.LastMoveRating))
                {
                    if (jumpDepth != -1 && jumpDepth < depth)
                    {
                        break;
                    }

                    if (visited.Contains(b.GetHashCode()))
                    {
                        continue;
                    }

                    jumpDepth = SolveCore(b, depth + 1, movesSinceFoundation, visited);
                }
            }

            if (jumpDepth == -1)
            {
                jumpDepth = (int)Math.Ceiling(depth * scale);
            }
            if (moves.Count == 0)
            {
                jumpDepth = (int)Math.Ceiling(depth * 0.7f);
            }

            return jumpDepth;
        }
    }
}