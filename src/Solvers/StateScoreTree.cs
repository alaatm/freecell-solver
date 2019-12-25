using System.Linq;
using System.Collections.Generic;

namespace FreeCellSolver.Solvers
{
    public class StateScoreTree
    {
        private HashSet<Board> _hash = new HashSet<Board>();
        private SortedDictionary<double, List<Board>> _scoreMap = new SortedDictionary<double, List<Board>>();

        public int Count => _hash.Count;

        public void Add(Board board)
        {
            _hash.Add(board);

            var score = board.Score;
            if (!_scoreMap.ContainsKey(score))
            {
                _scoreMap.Add(score, new List<Board>());
            }

            _scoreMap[score].Add(board);
        }

        public Board Remove()
        {
            var min = _scoreMap.ElementAt(0);

            var best = min.Value[0];
            min.Value.Remove(best);
            _hash.Remove(best);

            if (min.Value.Count == 0)
            {
                _scoreMap.Remove(min.Key);
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

            var score = board.Score;
            var list = _scoreMap[score];
            list.Remove(board);

            if (list.Count == 0)
            {
                _scoreMap.Remove(score);
            }

            return true;
        }
    }
}