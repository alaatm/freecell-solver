using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FreeCellSolver.Solvers
{
    public enum DfsSolveMethod
    {
        Recursive,
        Stack,
    }

    public class Dfs
    {
        private readonly int _maxDepth;
        private readonly float _backTrackPercent;

        public Board SolvedBoard { get; private set; }
        public int TotalVisitedNodes { get; private set; }
        public int SolvedFromId { get; private set; }

        public Dfs(int maxDepth, float backTrackPercent)
            => (_maxDepth, _backTrackPercent) = (maxDepth, backTrackPercent);

        // Non-parallel version used primarilly for debugging
        public static Dfs Run(Board board, DfsSolveMethod method) => Run(board, method, 200, 0.7f);

        public static Dfs Run(Board board, DfsSolveMethod method, int maxDepth, float backTrackPercent)
        {
            var dfs = new Dfs(maxDepth, backTrackPercent);
            Console.WriteLine($"Solver: DFS-{method.ToString()}");

            switch (method)
            {
                case DfsSolveMethod.Recursive:
                    dfs.DfsRecursive(board, 0, new HashSet<int>(), 0);
                    break;
                case DfsSolveMethod.Stack:
                    dfs.DfsStack(board, 0);
                    break;
            }

            return dfs;
        }

        public static Task<Dfs> RunParallelAsync(Board board, DfsSolveMethod method) => RunParallelAsync(board, method, 200, 0.7f);

        public static async Task<Dfs> RunParallelAsync(Board board, DfsSolveMethod method, int maxDepth, float backTrackPercent)
        {
            var attempt = 0;
            var backTrackStep = 0.0501f;
            var dfs = new Dfs(maxDepth, backTrackPercent);
            var states = ParallelHelper.GetStates(board, Environment.ProcessorCount);

            while (dfs.SolvedBoard == null && dfs._backTrackPercent < 1f)
            {
                Console.WriteLine($"Solver: DFS-{method.ToString()} - using {states.Count} cores - attempt #{++attempt}");
                IEnumerable<Task> tasks = null;

                switch (method)
                {
                    case DfsSolveMethod.Recursive:
                        // Note we start at depth of move count for parallel
                        tasks = states.Select((b, i) => Task.Run(() => dfs.DfsRecursive(b.Clone(), b.Moves.Count, new HashSet<int>(), i)));
                        break;
                    case DfsSolveMethod.Stack:
                        tasks = states.Select((b, i) => Task.Run(() => dfs.DfsStack(b.Clone(), i)));
                        break;
                }

                await Task.WhenAll(tasks);

                if (dfs.SolvedBoard != null)
                {
                    break;
                }

                dfs = new Dfs(maxDepth, backTrackPercent + backTrackStep);
                backTrackStep += 0.0501f;
            }

            return dfs;
        }

        private void DfsStack(Board root, int stateId)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<Board>();
            var jumpDepth = 0;

            stack.Push(root);

            while (stack.Count > 0)
            {
                var board = stack.Pop();
                var depth = board.Moves.Count;

                if (board.IsSolved || SolvedBoard != null)
                {
                    Finalize(board, visited.Count, stateId);
                    break;
                }

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
                visited.Add(hc);

                var moves = board.GetValidMoves(out var foundFoundation, out var autoMove);

                if (moves.Count == 0 || (board.MovesSinceFoundation >= 17 && !foundFoundation))
                {
                    jumpDepth = (int)Math.Ceiling(depth * _backTrackPercent);
                    continue;
                }

                var addedBoards = new Board[moves.Count];

                var c = 0;
                for (var i = moves.Count - 1; i >= 0; i--)
                {
                    var next = board.Clone();
                    next.ExecuteMove(moves[i], !autoMove);
                    addedBoards[c++] = next;
                }

                foreach (var b in addedBoards.OrderBy(p => p.LastMoveRating))
                {
                    stack.Push(b);
                }
            }
        }

        private int DfsRecursive(Board board, int depth, HashSet<int> visited, int stateId)
        {
            var jumpDepth = -1;

            if (board.IsSolved || SolvedBoard != null)
            {
                Finalize(board, visited.Count, stateId);
                return 0;
            }

            if (depth > _maxDepth)
            {
                return _maxDepth;
            }

            visited.Add(board.GetHashCode());

            var moves = board.GetValidMoves(out var foundFoundation, out var autoMove);

            if (moves.Count == 0 || (board.MovesSinceFoundation >= 17 && !foundFoundation))
            {
                return (int)Math.Ceiling(depth * _backTrackPercent);
            }

            var addedBoards = new Board[moves.Count];

            var c = 0;
            foreach (var move in moves)
            {
                var next = board.Clone();
                next.ExecuteMove(move, !autoMove);
                addedBoards[c++] = next;
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

                jumpDepth = DfsRecursive(b, depth + 1, visited, stateId);
            }

            return jumpDepth;
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