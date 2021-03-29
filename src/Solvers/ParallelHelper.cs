using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    internal static class ParallelHelper
    {
        internal static List<Board> GetStates(Board initialBoard, int num)
        {
            var hashes = new HashSet<Board>() { initialBoard };
            var tree = new Dictionary<int, List<Board>>() { { 0, new List<Board> { initialBoard } } };
            var depth = 0;

            while (true)
            {
                foreach (var b in tree[depth++])
                {
                    if (!tree.ContainsKey(depth))
                    {
                        tree.Add(depth, new List<Board>());
                    }

                    foreach (var move in b.GetValidMoves())
                    {
                        var next = b.ExecuteMove(move);
                        if (hashes.Add(next))
                        {
                            tree[depth].Add(next);
                        }
                    }
                }

                if (tree[depth].Count > num)
                {
                    break;
                }
            }

            Debug.Assert(tree[depth].Count > num);

            var boards = new List<Board>(tree[depth - 1]);
            foreach (var node in tree[depth - 1])
            {
                var descendants = tree[depth].Where(t => t.Prev == node).ToList();

                // Can we replace parent by all its descendants and remain within the allowed count?
                if (boards.Count + descendants.Count - 1 <= num)
                {
                    // Remove the parent
                    boards.Remove(descendants[0].Prev);
                    // Add all of its descendants
                    boards.AddRange(descendants);
                }
                else
                {
                    // Add the allowed number of descendants
                    var count = boards.Count;
                    for (var i = 0; i < num - count; i++)
                    {
                        boards.Add(descendants[i]);
                    }

                    // We won't be able to add anything else since we're maxed at this point
                    break;
                }
            }

            Debug.Assert(boards.Count == num);
            return boards;
        }
    }
}