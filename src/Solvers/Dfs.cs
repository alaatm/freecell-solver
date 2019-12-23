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
        private readonly Board _board;
        private readonly int _maxDepth;
        private readonly float _backTrackPercent;

        public Board SolvedBoard { get; private set; }
        public int TotalVisitedNodes { get; private set; }
        public int SolvedFromId { get; private set; }

        public Dfs(Board board, int maxDepth, float backTrackPercent)
            => (_board, _maxDepth, _backTrackPercent) = (board, maxDepth, backTrackPercent);

        public static Dfs Run(Board board, DfsSolveMethod method) => Run(board, method, 200, 0.7f);

        public static Dfs Run(Board board, DfsSolveMethod method, int maxDepth, float backTrackPercent)
        {
            var dfs = new Dfs(board, maxDepth, backTrackPercent);
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
            var dfs = new Dfs(board, maxDepth, backTrackPercent);
            var states = ParallelHelper.GetStates(board, Environment.ProcessorCount);
            Console.WriteLine($"Solver: DFS-{method.ToString()} - using {states.Count} cores");

            IEnumerable<Task> tasks = null;

            switch (method)
            {
                case DfsSolveMethod.Recursive:
                    // Note we start at depth of move count for parallel
                    tasks = states.Select((b, i) => Task.Run(() => dfs.DfsRecursive(b, b.Moves.Count, new HashSet<int>(), i)));
                    break;
                case DfsSolveMethod.Stack:
                    tasks = states.Select((b, i) => Task.Run(() => dfs.DfsStack(b, i)));
                    break;
            }

            await Task.WhenAll(tasks);
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
                    lock (this)
                    {
                        Finalize(board, visited.Count, stateId);
                        break;
                    }
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

                var (edges, backtrack) = board.GetEdges(true);

                if (backtrack)
                {
                    jumpDepth = (int)Math.Ceiling(depth * _backTrackPercent);
                    continue;
                }

                foreach (var b in edges)
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
                lock (this)
                {
                    Finalize(board, visited.Count, stateId);
                    return 0;
                }
            }

            if (depth > _maxDepth)
            {
                return _maxDepth;
            }

            visited.Add(board.GetHashCode());
            var (edges, backtrack) = board.GetEdges();

            if (backtrack)
            {
                return (int)Math.Ceiling(depth * _backTrackPercent);
            }

            foreach (var b in edges)
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
            if (SolvedBoard == null)
            {
                SolvedBoard = board;
                TotalVisitedNodes = visitedCount;
                SolvedFromId = stateId;
            }
        }
    }
}