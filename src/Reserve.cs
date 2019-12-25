using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public class Reserve : IEquatable<Reserve>
    {
        private readonly List<Card> _state = new List<Card>
        {
            null,
            null,
            null,
            null,
        };

        public Card this[int i] => _state[i];

        public int FreeCount { get; private set; } = 4;

        public Reserve(Card card1, Card card2, Card card3, Card card4)
        {
            Debug.Assert((card1 != card2 && card1 != card3 && card1 != card4 && card1 != null) || card1 == null);
            Debug.Assert((card2 != card1 && card2 != card3 && card2 != card4 && card2 != null) || card2 == null);
            Debug.Assert((card3 != card1 && card3 != card2 && card3 != card4 && card3 != null) || card3 == null);
            Debug.Assert((card4 != card1 && card4 != card2 && card4 != card3 && card4 != null) || card4 == null);

            _state[0] = card1;
            _state[1] = card2;
            _state[2] = card3;
            _state[3] = card4;

            FreeCount -= card1 != null ? 1 : 0;
            FreeCount -= card2 != null ? 1 : 0;
            FreeCount -= card3 != null ? 1 : 0;
            FreeCount -= card4 != null ? 1 : 0;
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
            FreeCount--;
            Debug.Assert(FreeCount >= 0 && FreeCount <= 4);
        }

        public void Remove(Card card)
        {
            Debug.Assert(_state.Contains(card));
            var index = _state.IndexOf(card);
            _state[index] = null;
            FreeCount++;
            Debug.Assert(FreeCount >= 0 && FreeCount <= 4);
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

        public Reserve Clone() => new Reserve(_state[0], _state[1], _state[2], _state[3]);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("01 02 03 04");
            sb.Append((_state[0]?.ToString() ?? "--") + " ");
            sb.Append((_state[1]?.ToString() ?? "--") + " ");
            sb.Append((_state[2]?.ToString() ?? "--") + " ");
            sb.Append(_state[3]?.ToString() ?? "--");

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.Where(c => c != null);

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Reserve other) => other == null
            ? false
            : _state[0] == other._state[0]
                && _state[1] == other._state[1]
                && _state[2] == other._state[2]
                && _state[3] == other._state[3];

        public override bool Equals(object obj) => obj is Reserve deal && Equals(deal);

        public override int GetHashCode() => HashCode.Combine(
            _state[0],
            _state[1],
            _state[2],
            _state[3]);

        public static bool operator ==(Reserve a, Reserve b) => Equals(a, b);

        public static bool operator !=(Reserve a, Reserve b) => !(a == b);
        #endregion
    }
}