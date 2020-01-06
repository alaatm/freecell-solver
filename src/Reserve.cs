using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class Reserve
    {
        private readonly short[] _state = new short[]
        {
            -1,
            -1,
            -1,
            -1,
        };

        public Card this[int i] => Card.Get(_state[i]);

        public int FreeCount { get; private set; } = 4;

        public Reserve(short card1, short card2, short card3, short card4)
        {
            Debug.Assert((card1 != card2 && card1 != card3 && card1 != card4 && card1 != -1) || card1 == -1);
            Debug.Assert((card2 != card1 && card2 != card3 && card2 != card4 && card2 != -1) || card2 == -1);
            Debug.Assert((card3 != card1 && card3 != card2 && card3 != card4 && card3 != -1) || card3 == -1);
            Debug.Assert((card4 != card1 && card4 != card2 && card4 != card3 && card4 != -1) || card4 == -1);

            _state[0] = card1;
            _state[1] = card2;
            _state[2] = card3;
            _state[3] = card4;

            FreeCount -= card1 != -1 ? 1 : 0;
            FreeCount -= card2 != -1 ? 1 : 0;
            FreeCount -= card3 != -1 ? 1 : 0;
            FreeCount -= card4 != -1 ? 1 : 0;
        }

        public Reserve() { }

        public bool CanInsert(out int index)
        {
            index = Array.IndexOf(_state, (short)-1);
            return FreeCount > 0;
        }

        private bool CanRemove(int index) => _state[index] != -1;

        public bool CanMove(int index, Tableau tableau)
            => CanRemove(index) && tableau.CanPush(Card.Get(_state[index]));

        public bool CanMove(int index, Foundation foundation, out int targetIndex)
        {
            var card = Card.Get(_state[index]);
            var canMove = CanRemove(index) && foundation.CanPush(card);
            targetIndex = canMove ? (short)card.Suit : -1;
            return canMove;
        }

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(out var idx) && idx == index);
            _state[index] = card.RawValue;
            FreeCount--;
            Debug.Assert(FreeCount == _state.Count(c => c == -1));
        }

        private Card Remove(int index)
        {
            FreeCount++;
            Debug.Assert(CanRemove(index));

            var card = _state[index];
            _state[index] = -1;
            Debug.Assert(FreeCount == _state.Count(c => c == -1));

            return Card.Get(card);
        }

        public void Move(int index, Tableau tableau)
        {
            Debug.Assert(CanMove(index, tableau));
            tableau.Push(Remove(index));
        }

        public void Move(int index, Foundation foundation)
        {
            Debug.Assert(CanMove(index, foundation, out _));
            foundation.Push(Remove(index));
        }

        public Reserve Clone() => new Reserve(_state[0], _state[1], _state[2], _state[3]);

        public override string ToString()
        {
            var c1 = _state[0] >= 0 ? Card.Get(_state[0]) : null;
            var c2 = _state[1] >= 0 ? Card.Get(_state[1]) : null;
            var c3 = _state[2] >= 0 ? Card.Get(_state[2]) : null;
            var c4 = _state[3] >= 0 ? Card.Get(_state[3]) : null;

            var sb = new StringBuilder();
            sb.AppendLine("01 02 03 04");
            sb.Append((c1?.ToString() ?? "--") + " ");
            sb.Append((c2?.ToString() ?? "--") + " ");
            sb.Append((c3?.ToString() ?? "--") + " ");
            sb.Append(c4?.ToString() ?? "--");

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.Where(c => c != -1).Select(c => Card.Get(c));
    }
}