using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class Reserve
    {
        private int _freeCount = 4;
        private readonly List<Card> _state = new List<Card>
        {
            null,
            null,
            null,
            null,
        };

        public Card this[int i] => _state[i];

        public int FreeCount => _freeCount;

        public int OccupiedCount => 4 - _freeCount;

        public Reserve(Card card1, Card card2, Card card3, Card card4)
        {
            Debug.Assert((card1 != card2 && card1 != card3 && card1 != card4 && card1 != null) || card1 == null);
            Debug.Assert((card2 != card1 && card2 != card3 && card2 != card4 && card2 != null) || card2 == null);
            Debug.Assert((card3 != card1 && card3 != card2 && card3 != card4 && card3 != null) || card3 == null);
            Debug.Assert((card4 != card1 && card4 != card2 && card4 != card3 && card4 != null) || card4 == null);

            _freeCount = 4;
            _state[0] = card1;
            _state[1] = card2;
            _state[2] = card3;
            _state[3] = card4;
        }

        public Reserve() { }

        public (bool canInsert, int index) CanInsert(Card card)
        {
            Debug.Assert(!_state.Contains(card));
            return FreeCount > 0
                ? (true, _state.IndexOf(null))
                : (false, -1);
        }

        public bool CanInsert(int index, Card card)
        {
            Debug.Assert(!_state.Contains(card));
            Debug.Assert(index >= 0 && index < 4);
            return _state[index] == null;
        }

        public bool CanMove(Card card, Tableau tableau)
        {
            Debug.Assert(_state.Contains(card));

            if (tableau.IsEmpty)
            {
                return true;
            }

            return card.IsBelow(tableau.Top);
        }

        public bool CanMove(Card card, Foundation foundation)
        {
            Debug.Assert(_state.Contains(card));
            return foundation.CanPush(card);
        }

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(index, card));
            _state[index] = card;
            _freeCount--;
            Debug.Assert(_freeCount >= 0 && _freeCount <= 4);
        }

        public void Remove(Card card)
        {
            Debug.Assert(_state.Contains(card));
            var index = _state.IndexOf(card);
            _state[index] = null;
            _freeCount++;
            Debug.Assert(_freeCount >= 0 && _freeCount <= 4);
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

        public Reserve Clone() => new Reserve(_state[0], _state[1], _state[2], _state[3]) { _freeCount = this._freeCount };
    }
}