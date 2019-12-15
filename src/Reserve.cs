using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class Reserve
    {
        private readonly List<Card> _list = new List<Card>
        {
            null,
            null,
            null,
            null,
        };

        public IEnumerable<Card> State => _list.ToList();

        public IEnumerable<(int index, Card card)> Occupied
        {
            get
            {
                var ret = new List<(int index, Card card)>();
                for (var i = 0; i < _list.Count; i++)
                {
                    if (_list[i] != null)
                    {
                        ret.Add((i, _list[i]));
                    }
                }

                return ret;
            }
        }

        public int FreeCount => _list.Count(c => c == null);

        internal Reserve(Reserve reserve) => _list = reserve._list.ToList();

        public Reserve() { }

        public (bool canInsert, int index) CanInsert(Card card)
        {
            Debug.Assert(!_list.Contains(card));
            return FreeCount > 0
                ? (true, _list.IndexOf(null))
                : (false, -1);
        }

        public bool CanInsert(int index, Card card)
        {
            Debug.Assert(!_list.Contains(card));
            Debug.Assert(index >= 0 && index < 4);
            return _list[index] == null;
        }

        public bool CanMove(Card card, Tableau tableau)
        {
            Debug.Assert(_list.Contains(card));

            if (tableau.IsEmpty)
            {
                return true;
            }

            return card.IsBelow(tableau.Top);
        }

        public bool CanMove(Card card, Foundation foundation)
        {
            Debug.Assert(_list.Contains(card));
            return foundation.CanPush(card);
        }

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(index, card));
            _list[index] = card;
        }

        public void Remove(Card card)
        {
            Debug.Assert(_list.Contains(card));

            var index = _list.IndexOf(card);
            _list[index] = null;
        }

        public void Move(Card card, Tableau tableau)
        {
            Debug.Assert(CanMove(card, tableau));
            Remove(card);
            tableau.Push(card);
        }

        public void Move(Card card, Foundation foundation)
        {
            Debug.Assert(CanMove(card, foundation));
            Remove(card);
            foundation.Push(card);
        }
    }
}