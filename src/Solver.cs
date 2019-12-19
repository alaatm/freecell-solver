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

        public static Board Solve(Board board) => Solve(board, b => b.Foundation.State.Values.Where(v => v != -1).Sum() + 4 == 52);

        public static Board Solve(Board board, Func<Board, bool> solvedCondition)
        {
            var solver = new Solver(solvedCondition);
            solver.SolveCore(board, 0, 0, new HashSet<int>());
            return solver.SolvedBoard;
        }

        public Solver(Func<Board, bool> solvedCondition) => _solvedCondition = solvedCondition;

        internal int SolveCore(Board board, int depth, int movesSinceFoundation, HashSet<int> visitedHashes)
        {
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
            if (movesSinceFoundation >= 20)
            {
                scale = 0.7f;
            }

            var addedBoards = new List<Board>();
            foreach (var moveString in moves)
            {
                var move = Move.Get(moveString);

                if (board.ShouldMove(move))
                {
                    var next = board.Clone();
                    if (next.Move(move, true))
                    {
                        addedBoards.Add(next);
                    }
                }
            }

            var jumpDepth = -1;

            if (scale == 1)
            {
                foreach (var b in addedBoards.OrderByDescending(p => p.LastMoveRating))
                {
                    if (jumpDepth != -1 && jumpDepth < depth)
                    {
                        break;
                    }

                    if (visitedHashes.Contains(b.GetHashCode()))
                    {
                        continue;
                    }

                    visitedHashes.Add(b.GetHashCode());
                    jumpDepth = SolveCore(b, depth + 1, movesSinceFoundation, visitedHashes);
                }
            }

            if (jumpDepth == -1)
            {
                jumpDepth = (int)Math.Ceiling(depth * scale);
            }
            if (addedBoards.Count == 0)
            {
                jumpDepth = (int)Math.Ceiling(depth * 0.7f);
            }

            return jumpDepth;
        }
    }
}