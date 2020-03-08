using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public class Reserve
    {
        private readonly short[] _state = new short[]
        {
            Card.EMPTY,
            Card.EMPTY,
            Card.EMPTY,
            Card.EMPTY,
        };

        public Card this[int i] => Card.Get(_state[i]);

        public int FreeCount { get; private set; } = 4;

        public Reserve(short card1, short card2, short card3, short card4)
        {
            Debug.Assert((card1 != card2 && card1 != card3 && card1 != card4 && card1 != Card.EMPTY) || card1 == Card.EMPTY);
            Debug.Assert((card2 != card1 && card2 != card3 && card2 != card4 && card2 != Card.EMPTY) || card2 == Card.EMPTY);
            Debug.Assert((card3 != card1 && card3 != card2 && card3 != card4 && card3 != Card.EMPTY) || card3 == Card.EMPTY);
            Debug.Assert((card4 != card1 && card4 != card2 && card4 != card3 && card4 != Card.EMPTY) || card4 == Card.EMPTY);

            _state[0] = card1;
            _state[1] = card2;
            _state[2] = card3;
            _state[3] = card4;

            FreeCount -= card1 != Card.EMPTY ? 1 : 0;
            FreeCount -= card2 != Card.EMPTY ? 1 : 0;
            FreeCount -= card3 != Card.EMPTY ? 1 : 0;
            FreeCount -= card4 != Card.EMPTY ? 1 : 0;
        }

        public Reserve() { }

        public bool CanInsert(out int index)
        {
            index = Array.IndexOf(_state, Card.EMPTY);
            return FreeCount > 0;
        }

        private bool CanRemove(int index) => _state[index] != Card.EMPTY;

        public bool CanMove(int index, Tableau tableau)
            => CanRemove(index) && tableau.CanPush(Card.Get(_state[index]));

        public bool CanMove(int index, Foundation foundation, out int targetIndex)
        {
            var card = Card.Get(_state[index]);
            var canMove = CanRemove(index) && foundation.CanPush(card);
            targetIndex = canMove ? (short)card.Suit : Card.EMPTY;
            return canMove;
        }

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(out var idx) && idx == index);
            _state[index] = card.RawValue;
            FreeCount--;
            Debug.Assert(FreeCount == _state.Count(c => c == Card.EMPTY));
        }

        private Card Remove(int index)
        {
            FreeCount++;
            Debug.Assert(CanRemove(index));

            var card = _state[index];
            _state[index] = Card.EMPTY;
            Debug.Assert(FreeCount == _state.Count(c => c == Card.EMPTY));

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

        public Reserve Clone()
        {
            const int SHORT_SIZE = 2;

            var clone = new Reserve();
            clone.FreeCount = FreeCount;
            Unsafe.CopyBlock(ref Unsafe.As<short, byte>(ref clone._state[0]), ref Unsafe.As<short, byte>(ref _state[0]), 4 * SHORT_SIZE);
            return clone;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("01 02 03 04");
            for (var i = 0; i < 4; i++)
            {
                var value = _state[i];
                sb.Append((value == Card.EMPTY ? "--" : Card.Get(value).ToString()));
                if (i < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.Where(c => c != Card.EMPTY).Select(c => Card.Get(c));
    }
}