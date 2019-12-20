using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class BP
    {
        public Board Board { get; set; }
        public Board Parent { get; set; }

        public BP(Board board) => Board = board;
        public BP(Board board, Board parent) => (Board, Parent) = (board, parent);
    }

    public class ParallelSolver
    {
        private const int _maxDepth = 200;
        private Func<Board, bool> _solvedCondition;

        public Board SolvedBoard { get; private set; }

        public static Task<Board> SolveAsync(Board board) => SolveAsync(board, b => b.IsSolved);

        public static async Task<Board> SolveAsync(Board board, Func<Board, bool> solvedCondition)
        {
            var solver = new Solver(solvedCondition);
            var states = ParallelSolver.GetStates(board, Environment.ProcessorCount);
            Console.WriteLine($"Using {states.Count} cores");

            var tasks = states.Select(b => Task.Run(() => solver.SolveCore(b, 0, 0, new HashSet<int>())));
            await Task.WhenAll(tasks);
            return solver.SolvedBoard;
        }

        public ParallelSolver(Func<Board, bool> solvedCondition) => _solvedCondition = solvedCondition;

        private static List<Board> GetStates(Board initialBoard, int num)
        {
            var tree = new Dictionary<int, List<BP>>() { { 0, new List<BP> { new BP(initialBoard) } } };
            var depth = 0;

            while (true)
            {
                foreach (var bp in tree[depth++])
                {
                    if (!tree.ContainsKey(depth))
                    {
                        tree.Add(depth, new List<BP>());
                    }

                    var (moves, _) = bp.Board.GetValidMoves(true);
                    foreach (var move in moves)
                    {
                        var next = bp.Board.Clone();
                        next.ExecuteMove(move);
                        tree[depth].Add(new BP(next, bp.Board));
                    }
                }

                if (tree[depth].Count > num)
                {
                    break;
                }
            }

            var stateList = new List<List<Board>>();

            var count = 0;
            var parents = tree[depth - 1].Select(t => t.Board).ToList();
            for (var i = 0; i < parents.Count; i++)
            {
                var boardList = new List<Board>();

                var prevChilds = 0;
                for (var j = 0; j < i; j++)
                {
                    boardList.AddRange(tree[depth].Where(t => t.Parent == parents[j]).Select(t => t.Board));
                }

                var parent = parents[i];
                var children = tree[depth].Where(t => t.Parent == parent).ToList();
                boardList.AddRange(children.Select(t => t.Board));
                boardList.AddRange(parents.Skip(i + 1));

                count = prevChilds + children.Count + parents.Count - (i + 1);
                stateList.Add(boardList);
            }

            var boards = stateList.OrderByDescending(p => p.Count).FirstOrDefault(p => p.Count < num);
            return boards != null
                ? boards
                : tree.OrderByDescending(p => p.Value.Count).FirstOrDefault(p => p.Value.Count < 16).Value.Select(p => p.Board).ToList();
        }
    }
}