using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Reserve
    {
        private readonly sbyte[] _state =
        {
            Card.Empty,
            Card.Empty,
            Card.Empty,
            Card.Empty,
        };

        public Card this[int i] => Card.Get(_state[i]);

        public int FreeCount { get; private set; } = 4;

        public Reserve(sbyte card1, sbyte card2, sbyte card3, sbyte card4)
        {
            Debug.Assert((card1 != card2 && card1 != card3 && card1 != card4 && card1 != Card.Empty) || card1 == Card.Empty);
            Debug.Assert((card2 != card1 && card2 != card3 && card2 != card4 && card2 != Card.Empty) || card2 == Card.Empty);
            Debug.Assert((card3 != card1 && card3 != card2 && card3 != card4 && card3 != Card.Empty) || card3 == Card.Empty);
            Debug.Assert((card4 != card1 && card4 != card2 && card4 != card3 && card4 != Card.Empty) || card4 == Card.Empty);

            _state[0] = card1;
            _state[1] = card2;
            _state[2] = card3;
            _state[3] = card4;

            FreeCount -= card1 != Card.Empty ? 1 : 0;
            FreeCount -= card2 != Card.Empty ? 1 : 0;
            FreeCount -= card3 != Card.Empty ? 1 : 0;
            FreeCount -= card4 != Card.Empty ? 1 : 0;
        }

        public Reserve() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte GetValue(int i) => _state[i];

        public bool CanInsert(out int index)
        {
            index = _state.AsSpan().IndexOf(Card.Empty);
            return FreeCount > 0;
        }

        private bool CanRemove(int index) => _state[index] != Card.Empty;

        public bool CanMove(int index, Tableau tableau)
            => CanRemove(index) && tableau.CanPush(Card.Get(_state[index]));

        public bool CanMove(int index, Foundation foundation, out int targetIndex)
        {
            Card card = null;
            var canMove = CanRemove(index) && foundation.CanPush(card = Card.Get(_state[index]));
            targetIndex = canMove ? card.Suit : Card.Empty;

            return canMove;
        }

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(out var idx) && idx == index);
            _state[index] = card.RawValue;
            FreeCount--;
            Debug.Assert(FreeCount == _state.Count(c => c == Card.Empty));
        }

        private Card Remove(int index)
        {
            FreeCount++;
            Debug.Assert(CanRemove(index));

            var card = _state[index];
            _state[index] = Card.Empty;
            Debug.Assert(FreeCount == _state.Count(c => c == Card.Empty));

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
            var clone = new Reserve { FreeCount = FreeCount };
            Unsafe.CopyBlock(ref Unsafe.As<sbyte, byte>(ref clone._state[0]), ref Unsafe.As<sbyte, byte>(ref _state[0]), 4);
            return clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Reserve other)
        {
            if (FreeCount != other.FreeCount)
            {
                return false;
            }

            if (FreeCount == 4)
            {
                return true;
            }

            // We have same number of occupied slots, verify that cards in them are the same
            // regardless of order
            for (var i = 0; i < 4; i++)
            {
                var card = _state[i];
                if (card != Card.Empty)
                {
                    var found = false;
                    for (var j = 0; j < 4; j++)
                    {
                        if (card == other._state[j])
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("01 02 03 04");
            for (var i = 0; i < 4; i++)
            {
                var value = _state[i];
                sb.Append((value == Card.Empty ? "--" : Card.Get(value).ToString()));
                if (i < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.Where(c => c != Card.Empty).Select(c => Card.Get(c));
    }
}