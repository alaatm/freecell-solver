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
        private readonly byte[] _state =
        {
            Card.Nil,
            Card.Nil,
            Card.Nil,
            Card.Nil,
        };

        public Card this[int i]
        {
            get
            {
                var rawVal = _state[i];
                return rawVal == Card.Nil
                    ? Card.Null
                    : Card.Get(rawVal);
            }
        }

        public int FreeCount { get; private set; } = 4;

        public Reserve() { }

        public Reserve(string card1, string card2 = null, string card3 = null, string card4 = null) : this(
            string.IsNullOrWhiteSpace(card1) ? Card.Nil : Card.Get(card1).RawValue,
            string.IsNullOrWhiteSpace(card2) ? Card.Nil : Card.Get(card2).RawValue,
            string.IsNullOrWhiteSpace(card3) ? Card.Nil : Card.Get(card3).RawValue,
            string.IsNullOrWhiteSpace(card4) ? Card.Nil : Card.Get(card4).RawValue)
        { }

        private Reserve(byte card1, byte card2, byte card3, byte card4)
        {
            Debug.Assert((card1 != card2 && card1 != card3 && card1 != card4 && card1 != Card.Nil) || card1 == Card.Nil);
            Debug.Assert((card2 != card1 && card2 != card3 && card2 != card4 && card2 != Card.Nil) || card2 == Card.Nil);
            Debug.Assert((card3 != card1 && card3 != card2 && card3 != card4 && card3 != Card.Nil) || card3 == Card.Nil);
            Debug.Assert((card4 != card1 && card4 != card2 && card4 != card3 && card4 != Card.Nil) || card4 == Card.Nil);

            _state[0] = card1;
            _state[1] = card2;
            _state[2] = card3;
            _state[3] = card4;

            FreeCount -= card1 != Card.Nil ? 1 : 0;
            FreeCount -= card2 != Card.Nil ? 1 : 0;
            FreeCount -= card3 != Card.Nil ? 1 : 0;
            FreeCount -= card4 != Card.Nil ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetValue(int i) => _state[i];

        public bool CanInsert(out int index)
        {
            index = _state.AsSpan().IndexOf(Card.Nil);
            return FreeCount > 0;
        }

        private bool CanRemove(int index) => _state[index] != Card.Nil;

        public bool CanMove(int index, Tableau tableau)
            => CanRemove(index) && tableau.CanPush(Card.Get(_state[index]));

        public bool CanMove(int index, Foundation foundation)
            => CanRemove(index) && foundation.CanPush(Card.Get(_state[index]));

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(out var idx) && idx == index);
            _state[index] = card.RawValue;
            FreeCount--;
            Debug.Assert(FreeCount == _state.Count(c => c == Card.Nil));
        }

        private Card Remove(int index)
        {
            FreeCount++;
            Debug.Assert(CanRemove(index));

            var card = _state[index];
            _state[index] = Card.Nil;
            Debug.Assert(FreeCount == _state.Count(c => c == Card.Nil));

            return Card.Get(card);
        }

        public void Move(int index, Tableau tableau)
        {
            Debug.Assert(CanMove(index, tableau));
            tableau.Push(Remove(index));
        }

        public void Move(int index, Foundation foundation)
        {
            Debug.Assert(CanMove(index, foundation));
            foundation.Push(Remove(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Reserve Clone()
        {
            var clone = new Reserve { FreeCount = FreeCount };
            Unsafe.CopyBlock(ref clone._state[0], ref _state[0], 4);
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
                if (card != Card.Nil)
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
                sb.Append((value == Card.Nil ? "--" : Card.Get(value).ToString()));
                if (i < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.Where(c => c != Card.Nil).Select(c => Card.Get(c));
    }
}