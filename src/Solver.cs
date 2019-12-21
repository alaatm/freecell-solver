using System;
using System.Linq;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public enum SolveMethod
    {
        DFSStack,
        DFSRecursive,
    }

    public class Solver
    {
        private static object _lock = new object();

        private const int _maxDepth = 200;
        private Func<Board, bool> _solvedCondition;

        public Board SolvedBoard { get; private set; }
        public int TotalVisitedNodes { get; private set; }

        public static Solver Solve(Board board, SolveMethod method) => Solve(board, method, b => b.IsSolved);

        public static Solver Solve(Board board, SolveMethod method, Func<Board, bool> solvedCondition)
        {
            var solver = new Solver(solvedCondition);
            Console.WriteLine($"Solver: {method.ToString()}");

            switch (method)
            {
                case SolveMethod.DFSRecursive:
                    solver.SolveDFSRecursive(board, 0, new HashSet<int>());
                    break;
                case SolveMethod.DFSStack:
                    solver.SolveDFSStack(board);
                    break;
            }

            return solver;
        }

        public Solver(Func<Board, bool> solvedCondition) => _solvedCondition = solvedCondition;

        internal void SolveDFSStack(Board root)
        {
            var visited = new HashSet<int>();
            var jumpDepth = 0;

            var stack = new Stack<Board>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                if (SolvedBoard != null)
                {
                    return;
                }

                var board = stack.Pop();
                var depth = board.Moves.Count;

                if (jumpDepth != 0 && jumpDepth < depth - 1)
                {
                    continue;
                }
                jumpDepth = 0;

                var hc = board.GetHashCode();
                if (visited.Contains(hc) || depth > _maxDepth)
                {
                    continue;
                }

                if (_solvedCondition(board))
                {
                    lock (_lock)
                    {
                        SolvedBoard = board;
                        TotalVisitedNodes = visited.Count;
                        break;
                    }
                }

                visited.Add(hc);
                var (moves, foundFoundation) = board.GetValidMoves(true);

                if ((board.MovesSinceFoundation >= 17 && !foundFoundation) || moves.Count == 0)
                {
                    jumpDepth = (int)Math.Ceiling(depth * 0.7f);
                    continue;
                }

                var addedBoards = new List<Board>();
                // We're adding in reverse so that we maintain the order of states in case of equal last move rate
                for (var i = moves.Count - 1; i >= 0; i--)
                {
                    var next = board.Clone();
                    next.ExecuteMove(moves[i], true);
                    addedBoards.Add(next);
                }

                foreach (var b in addedBoards.OrderBy(p => p.LastMoveRating))
                {
                    stack.Push(b);
                }
            }
        }

        internal int SolveDFSRecursive(Board board, int depth, HashSet<int> visited)
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
                lock (_lock)
                {
                    SolvedBoard = board;
                    TotalVisitedNodes = visited.Count;
                    return 0;
                }
            }

            var (moves, foundFoundation) = board.GetValidMoves(true);

            var scale = 1f;
            if (board.MovesSinceFoundation >= 17 && !foundFoundation)
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

                    jumpDepth = SolveDFSRecursive(b, depth + 1, visited);
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