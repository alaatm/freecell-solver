using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using FreeCellSolver.Game;
using FreeCellSolver.Extensions;
using FreeCellSolver.Solvers.Shared;

namespace FreeCellSolver.Solvers
{
    public class Dfs : ISolver
    {
        private static ConcurrentDictionary<int, byte> _closed;

        private readonly int _maxDepth;
        private readonly int _maxMovesSinceFoundation;
        private readonly float _backTrackPercent;

        public Board SolvedBoard { get; private set; }
        public int SolvedFromId { get; private set; }
        public int VisitedNodes => _closed.Count;

        public Dfs(int maxDepth, int maxMovesSinceFoundation, float backTrackPercent)
            => (_maxDepth, _maxMovesSinceFoundation, _backTrackPercent) = (maxDepth, maxMovesSinceFoundation, backTrackPercent);

        // Non-parallel version used primarilly for debugging
        public static ISolver Run(Board board)
        {
            Console.WriteLine($"Solver: DFS");

            const int maxDepth = 200;
            const int maxMovesSinceFoundation = 17;
            const float backTrackPercent = 0.7f;

            var clone = board.Clone();
            clone.AutoPlay();

            // Should obviously use a local HashSet<int> here but we don't care much about this
            // non parallel version, its only here for debugging.
            _closed = new ConcurrentDictionary<int, byte>(1, 1000);

            var dfs = new Dfs(maxDepth, maxMovesSinceFoundation, backTrackPercent);
            dfs.Search(clone, 0);
            return dfs;
        }

        public static async Task<ISolver> RunParallelAsync(Board board)
        {
            const int maxDepth = 200;
            const int maxMovesSinceFoundation = 17;
            const float backTrackPercent = 0.7f;

            var attempt = 0;

            var backTrackPercentStep = 0.0501f;
            var maxMovesSinceFoundationStep = 5;

            var clone = board.Clone();
            clone.AutoPlay();

            Dfs dfs;
            var states = ParallelHelper.GetStates(clone, Environment.ProcessorCount);

            do
            {
                Console.WriteLine($"Solver: DFS - using {states.Count} cores - attempt #{attempt + 1}");

                _closed = new ConcurrentDictionary<int, byte>(states.Count, 1000);

                dfs = new Dfs(
                    maxDepth,
                    maxMovesSinceFoundation + maxMovesSinceFoundationStep * attempt,
                    backTrackPercent + backTrackPercentStep * attempt);

                var tasks = states.Select((b, i) => Task.Run(() => dfs.Search(b, i)));
                await Task.WhenAll(tasks);
                attempt++;
            } while (dfs.SolvedBoard == null && dfs._backTrackPercent + backTrackPercentStep < 1f);

            return dfs;
        }

        private void Search(Board root, int stateId)
        {
            var open = new Stack<Board>();
            var jumpDepth = 0;

            open.Push(root);

            while (open.Count > 0)
            {
                var board = open.Pop();
                var depth = board.MoveCount;

                if (board.IsSolved || SolvedBoard != null)
                {
                    Finalize(board, stateId);
                    break;
                }

                if (jumpDepth != 0 && jumpDepth < depth - 1)
                {
                    continue;
                }
                jumpDepth = 0;

                var hc = board.GetHashCode();
                if (_closed.ContainsKey(hc) || depth > _maxDepth)
                {
                    continue;
                }
                _closed.AddOrUpdate(hc, 1, (k, v) => 1);

                var moves = board.GetValidMoves(out var foundFoundation);

                if (board.MovesSinceFoundation >= _maxMovesSinceFoundation && !foundFoundation)
                {
                    jumpDepth = (int)Math.Ceiling(depth * _backTrackPercent);
                    continue;
                }

                var addedBoards = new Board[moves.Count];

                var c = 0;
                for (var i = moves.Count - 1; i >= 0; i--)
                {
                    var next = board.Clone();
                    next.RateMove(moves[i]);
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