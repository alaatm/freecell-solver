using System.Linq;
using System.Collections.Generic;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    public class StateCostTree
    {
        private HashSet<Board> _hash = new HashSet<Board>();
        private SortedDictionary<int, List<Board>> _costMap = new SortedDictionary<int, List<Board>>();

        public int Count => _hash.Count;

        public void Add(Board board)
        {
            _hash.Add(board);

            var cost = board.Cost;
            if (!_costMap.ContainsKey(cost))
            {
                _costMap.Add(cost, new List<Board>());
            }

            _costMap[cost].Add(board);
        }

        public Board RemoveMin()
        {
            var min = _costMap.ElementAt(0);

            var best = min.Value[0];
            min.Value.Remove(best);
            _hash.Remove(best);

            if (min.Value.Count == 0)
            {
                _costMap.Remove(min.Key);
            }

            return best;
        }

        public Board GetValue(Board equalValue)
        {
            _hash.TryGetValue(equalValue, out var actualValue);
            return actualValue;
        }

        public bool Remove(Board board)
        {
            var removed = _hash.Remove(board);
            if (!removed)
            {
                return false;
            }

            var cost = board.Cost;
            var list = _costMap[cost];
            list.Remove(board);

            if (list.Count == 0)
            {
                _costMap.Remove(cost);
            }

            return true;
        }
    }
}