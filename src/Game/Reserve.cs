using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game.Extensions;
using FreeCellSolver.Buffers;

namespace FreeCellSolver.Game
{
    public sealed class Reserve
    {
        private Arr04 _state;

        public Card this[int i] => Card.Get(_state[i]);

        public int FreeCount { get; private set; } = 4;

        private Reserve() { }

        // Do not call other overloads of Create() since this is called from
        // Board.FromDealNumber() and thus should be optimized.
        public static Reserve Create()
        {
            var r = new Reserve();
            r._state[0] = Card.Nil;
            r._state[1] = Card.Nil;
            r._state[2] = Card.Nil;
            r._state[3] = Card.Nil;
            return r;
        }

        public static Reserve Create(string card1) => Create(card1, null, null, null);

        public static Reserve Create(string card1, string card2) => Create(card1, card2, null, null);

        public static Reserve Create(string card1, string card2, string card3) => Create(card1, card2, card3, null);

        public static Reserve Create(string card1, string card2, string card3, string card4)
        {
            var c1 = card1.IsEmpty() ? Card.Nil : Card.Get(card1).RawValue;
            var c2 = card2.IsEmpty() ? Card.Nil : Card.Get(card2).RawValue;
            var c3 = card3.IsEmpty() ? Card.Nil : Card.Get(card3).RawValue;
            var c4 = card4.IsEmpty() ? Card.Nil : Card.Get(card4).RawValue;

            Debug.Assert((c1 != c2 && c1 != c3 && c1 != c4 && c1 != Card.Nil) || c1 == Card.Nil);
            Debug.Assert((c2 != c1 && c2 != c3 && c2 != c4 && c2 != Card.Nil) || c2 == Card.Nil);
            Debug.Assert((c3 != c1 && c3 != c2 && c3 != c4 && c3 != Card.Nil) || c3 == Card.Nil);
            Debug.Assert((c4 != c1 && c4 != c2 && c4 != c3 && c4 != Card.Nil) || c4 == Card.Nil);

            var r = new Reserve();
            r._state[0] = (byte)c1;
            r._state[1] = (byte)c2;
            r._state[2] = (byte)c3;
            r._state[3] = (byte)c4;

            r.FreeCount -= c1 != Card.Nil ? 1 : 0;
            r.FreeCount -= c2 != Card.Nil ? 1 : 0;
            r.FreeCount -= c3 != Card.Nil ? 1 : 0;
            r.FreeCount -= c4 != Card.Nil ? 1 : 0;

            return r;
        }

        public byte GetValue(int i) => _state[i];

        public bool CanInsert(out int index)
        {
            index = _state.IndexOf(Card.Nil);
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
            _state[index] = (byte)card.RawValue;
            FreeCount--;
            Debug.Assert(FreeCount == _state.AsArray().Count(c => c == Card.Nil));
        }

        private Card Remove(int index)
        {
            FreeCount++;
            Debug.Assert(CanRemove(index));

            var card = _state[index];
            _state[index] = Card.Nil;
            Debug.Assert(FreeCount == _state.AsArray().Count(c => c == Card.Nil));

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

        public Reserve Clone() => new() { _state = _state, FreeCount = FreeCount };

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
            var state = _state;
            var otherState = other._state;

            for (var i = 0; i < 4; i++)
            {
                var card = state[i];
                if (card != Card.Nil && otherState.IndexOf(card) == -1)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("00 01 02 03");
            for (var i = 0; i < 4; i++)
            {
                var value = _state[i];
                sb.Append((value == Card.Nil ? "--" : Card.Get(value).ToString()));
                if (i < 3)
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.AsArray().Where(c => c != Card.Nil).Select(c => Card.Get(c));
    }
}