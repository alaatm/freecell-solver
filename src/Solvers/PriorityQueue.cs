using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    public class PriorityQueue
    {
        private readonly HashSet<Board> _hash = new HashSet<Board>();
        private readonly SortedList<int, Queue<Board>> _costMap = new SortedList<int, Queue<Board>>();

        public int Count => _hash.Count;

        public void Enqueue(Board board)
        {
            Debug.Assert(!_hash.Contains(board));
            _hash.Add(board);

            var cost = board.Cost;
            if (!_costMap.ContainsKey(cost))
            {
                _costMap.Add(cost, new Queue<Board>());
            }

            _costMap[cost].Enqueue(board);
        }

        public Board Dequeue()
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

        public bool Contains(Board equalValue) => _hash.Contains(equalValue);
    }

    // Same as PriorityQueue above except that it supports removing items at arbitrary locations.
    public class DynamicPriorityQueue
    {
        private readonly HashSet<Board> _hash = new HashSet<Board>();
        private readonly SortedList<int, List<Board>> _costMap = new SortedList<int, List<Board>>();

        public int Count => _hash.Count;

        public void Enqueue(Board board)
        {
            Debug.Assert(!_hash.Contains(board));
            _hash.Add(board);

            var cost = board.Cost;
            if (!_costMap.ContainsKey(cost))
            {
                _costMap.Add(cost, new List<Board>());
            }

            _costMap[cost].Add(board);
        }

        public Board Dequeue()
        {
            var (keyMin, valMin) = _costMap.Min();

            var best = valMin[0];
            valMin.RemoveAt(0);
            _hash.Remove(best);

            if (valMin.Count == 0)
            {
                _costMap.Remove(keyMin);
            }

            return best;
        }

        public bool TryGetValue(Board equalValue, out Board actualValue) => _hash.TryGetValue(equalValue, out actualValue);

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

    public static class SortedListExtensions
    {
        public static (K keyMin, V valMin) Min<K, V>(this SortedList<K, V> dict) => (dict.Keys[0], dict.Values[0]);
    }
}