using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    internal static class ParallelHelper
    {
        internal static List<Board> GetNodes(Board root, int num)
        {
            var set = new HashSet<Board>() { root };
            var tree = new Dictionary<int, List<Board>>() { { 0, new List<Board> { root } } };
            var depth = 0;

            while (true)
            {
                foreach (var node in tree[depth++])
                {
                    if (!tree.ContainsKey(depth))
                    {
                        tree.Add(depth, new List<Board>());
                    }

                    foreach (var move in node.GetValidMoves())
                    {
                        var next = node.ExecuteMove(move);
                        if (set.Add(next))
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

            var nodes = new List<Board>(tree[depth - 1]);
            foreach (var node in tree[depth - 1])
            {
                var descendants = tree[depth].Where(t => t.Prev == node).ToArray();

                // Can we replace parent by all its descendants and remain within the allowed count?
                if (nodes.Count + descendants.Length - 1 <= num)
                {
                    // Remove the parent
                    nodes.Remove(descendants[0].Prev);
                    // Add all of its descendants
                    nodes.AddRange(descendants);
                }
                else
                {
                    // Add the allowed number of descendants
                    var count = nodes.Count;
                    for (var i = 0; i < num - count; i++)
                    {
                        nodes.Add(descendants[i]);
                    }

                    // We won't be able to add anything else since we're maxed at this point
                    break;
                }
            }

            Debug.Assert(nodes.Count == num);
            return nodes;
        }
    }
}