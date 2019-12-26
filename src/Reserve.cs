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

        public bool CanInsert(out int index)
        {
            index = _state.IndexOf(null);
            return FreeCount > 0;
        }

        private bool CanRemove(int index) => _state[index] != null;

        public bool CanMove(int index, Tableau tableau)
            => CanRemove(index) && tableau.CanPush(_state[index]);

        public bool CanMove(int index, Foundation foundation, out int targetIndex)
        {
            var card = _state[index];
            var canMove = CanRemove(index) && foundation.CanPush(card);
            targetIndex = canMove ? (int)card.Suit : -1;
            return canMove;
        }

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(out var idx) && idx == index);
            _state[index] = card;
            FreeCount--;
            Debug.Assert(FreeCount == _state.Count(c => c == null));
        }

        private Card Remove(int index)
        {
            FreeCount++;
            Debug.Assert(CanRemove(index));

            var card = _state[index];
            _state[index] = null;
            Debug.Assert(FreeCount == _state.Count(c => c == null));

            return card;
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

        internal void Undo(Move move, Board board)
        {
            if (move.Type == MoveType.TableauToReserve)
            {
                board.Tableaus[move.From].UndoPop(Remove(move.To));
            }
        }

        internal void UndoRemove(int index, Card card)
        {
            Debug.Assert(_state[index] == null);
            _state[index] = card;
            FreeCount--;
            Debug.Assert(FreeCount == _state.Count(c => c == null));
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