using System.Collections.Generic;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    public class StateCostTree
    {
        private readonly HashSet<Board> _hash = new HashSet<Board>();
        private readonly SortedList<int, Queue<Board>> _costMap = new SortedList<int, Queue<Board>>();

        public int Count => _hash.Count;

        public void Add(Board board)
        {
            _hash.Add(board);

            var cost = board.Cost;
            if (!_costMap.ContainsKey(cost))
            {
                _costMap.Add(cost, new Queue<Board>());
            }

            _costMap[cost].Enqueue(board);
        }

        public Board RemoveMin()
        {
            var (keyMin, valMin) = _costMap.Min();

            var best = valMin.Dequeue();
            _hash.Remove(best);

            if (valMin.Count == 0)
            {
                _costMap.Remove(keyMin);
            }

            return best;
        }

        public Board GetValue(Board equalValue)
        {
            _hash.TryGetValue(equalValue, out var actualValue);
            return actualValue;
        }

        // public bool Remove(Board board)
        // {
        //     var removed = _hash.Remove(board);
        //     if (!removed)
        //     {
        //         return false;
        //     }

        //     var cost = board.Cost;
        //     var list = _costMap[cost];
        //     list.Remove(board);

        //     if (list.Count == 0)
        //     {
        //         _costMap.Remove(cost);
        //     }

        //     return true;
        // }
    }

    public static class SortedListExtensions
    {
        public static (K keyMin, V valMin) Min<K, V>(this SortedList<K, V> dict) => (dict.Keys[0], dict.Values[0]);
    }
}