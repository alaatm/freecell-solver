using System.Linq;
using System.Collections.Generic;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers.Shared
{
    internal static class ParallelHelper
    {
        internal static HashSet<Board> GetStates(Board initialBoard, int num)
        {
            var tree = new Dictionary<int, HashSet<Board>>() { { 0, new HashSet<Board> { initialBoard } } };
            var depth = 0;

            while (true)
            {
                foreach (var b in tree[depth++])
                {
                    if (!tree.ContainsKey(depth))
                    {
                        tree.Add(depth, new HashSet<Board>());
                    }

                    foreach (var move in b.GetValidMoves(out _))
                    {
                        var next = b.Clone();
                        next.ExecuteMove(move, b);
                        tree[depth].Add(next);
                    }
                }

                if (tree[depth].Count > num)
                {
                    break;
                }
            }

            // TODO:
            // If nominees count is > cpu core count then we should .Take(cpuCount-1) then take the parent.
            // We can avoid processing the same boards by making the solvers delay execution of the last
            // board in the states array (the parent that contains dups) so that they can be immediatelly
            // skipped because they'll already be in the closed set.
            // Same applies when nominees count is < cpu core count. We should take all of them and then
            // append the parent at the end and have solvers delay it's processing.
            var nominees = new List<HashSet<Board>>();
            foreach (var node in tree[depth - 1])
            {
                var descendants = tree[depth].Where(t => t.Prev == node);
                var adjacents = tree[depth - 1].Where(n => n != node);
                nominees.Add(new HashSet<Board>(descendants.Concat(adjacents)));
            }

            var boards = nominees.OrderByDescending(p => p.Count).FirstOrDefault(p => p.Count <= num);
            return boards ?? tree[depth - 1];
        }
    }
}