using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using FreeCellSolver.Extensions;

namespace FreeCellSolver.Solvers
{
    public class Dfs
    {
        private readonly int _maxDepth;
        private readonly int _maxMovesSinceFoundation;
        private readonly float _backTrackPercent;

        public Board SolvedBoard { get; private set; }
        public int TotalVisitedNodes { get; private set; }
        public int SolvedFromId { get; private set; }

        public Dfs(int maxDepth, int maxMovesSinceFoundation, float backTrackPercent)
            => (_maxDepth, _maxMovesSinceFoundation, _backTrackPercent) = (maxDepth, maxMovesSinceFoundation, backTrackPercent);

        // Non-parallel version used primarilly for debugging
        public static Dfs Run(Board board) => Run(board, 200, 0.7f);

        public static Dfs Run(Board board, int maxDepth, float backTrackPercent)
        {
            var dfs = new Dfs(maxDepth, 17, backTrackPercent);
            Console.WriteLine($"Solver: DFS");
            dfs.Search(board.Clone(), 0);
            return dfs;
        }

        public static Task<Dfs> RunParallelAsync(Board board) => RunParallelAsync(board, 200, 0.7f);

        public static async Task<Dfs> RunParallelAsync(Board board, int maxDepth, float backTrackPercent)
        {
            var attempt = 0;
            var backTrackStep = 0.0501f;
            var maxMovesSinceFoundation = 17;
            var maxMovesSinceFoundationStep = 5;

            var dfs = new Dfs(maxDepth, maxMovesSinceFoundation, backTrackPercent);
            var states = ParallelHelper.GetStates(board.Clone(), Environment.ProcessorCount);

            while (dfs.SolvedBoard == null && dfs._backTrackPercent < 1f)
            {
                Console.WriteLine($"Solver: DFS - using {states.Count} cores - attempt #{++attempt}");
                var tasks = states.Select((b, i) => Task.Run(() => dfs.Search(b, i)));

                await Task.WhenAll(tasks);

                if (dfs.SolvedBoard != null)
                {
                    break;
                }

                dfs = new Dfs(maxDepth, maxMovesSinceFoundation + maxMovesSinceFoundationStep, backTrackPercent + backTrackStep);
                backTrackStep += 0.0501f;
                maxMovesSinceFoundationStep += 5;
            }

            return dfs;
        }

        private void Search(Board root, int stateId)
        {
            var closed = new HashSet<int>();
            var open = new Stack<Board>();
            var jumpDepth = 0;

            open.Push(root);

            while (open.Count > 0)
            {
                var board = open.Pop();
                var depth = board.MoveCount;

                if (board.IsSolved || SolvedBoard != null)
                {
                    Finalize(board, closed.Count, stateId);
                    break;
                }

                if (jumpDepth != 0 && jumpDepth < depth - 1)
                {
                    continue;
                }
                jumpDepth = 0;

                var hc = board.GetHashCode();
                if (closed.Contains(hc) || depth > _maxDepth)
                {
                    continue;
                }
                closed.Add(hc);

                var moves = board.GetValidMoves(out var foundFoundation, out var autoMove);
                Debug.Assert(autoMove && moves.Count == 0 || true);

                if (moves.Count == 0 || (board.MovesSinceFoundation >= _maxMovesSinceFoundation && !foundFoundation))
                {
                    jumpDepth = (int)Math.Ceiling(depth * _backTrackPercent);
                    continue;
                }

                var addedBoards = new Board[moves.Count];

                var c = 0;
                for (var i = moves.Count - 1; i >= 0; i--)
                {
                    var next = board.Clone();
                    if (!autoMove)
                    {
                        next.RateMove(moves[i]);
                    }
                    next.ExecuteMove(moves[i], board);
                    addedBoards[c++] = next;
                }

                // Asc order
                addedBoards.InsertionSort((x, y) => x.LastMoveRating - y.LastMoveRating);

                foreach (var b in addedBoards)
                {
                    open.Push(b);
                }
            }
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