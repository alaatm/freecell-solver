using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public class Tableau : IEquatable<Tableau>
    {
        private const int _capacity = 19;
        private readonly short[] _state = new short[_capacity];

        public int Size { get; private set; }

        public bool IsEmpty => Size == 0;

        public Card this[int index] => Card.Get(_state[Size - index - 1]);

        public Card Top => Size == 0 ? null : Card.Get(_state[Size - 1]);

        /// <summary>
        /// Returns sorted cards that are only at the bottom of the stack
        /// </summary>
        /// <value></value>
        public int SortedSize { get; private set; }

        public Tableau(params string[] cards) : this(cards.Select(c => Card.Get(c)).ToArray()) { }

        public Tableau(params Card[] cards)
        {
            foreach (var card in cards)
            {
                _state[Size++] = card.RawValue;
            }

            SortedSize = CountSorted();
        }

        private Tableau() { }

        public bool CanPush(Card card)
        {
            var size = Size;
            return size == 0 || card.IsBelow(Card.Get(_state[size - 1]));
        }

        public bool CanPop() => Size > 0;

        public bool CanMove(Reserve reserve, out int index)
        {
            index = -1;
            return CanPop() && reserve.CanInsert(out index);
        }

        public bool CanMove(Foundation foundation, out int targetIndex)
        {
            var card = Top;
            var canMove = CanPop() && foundation.CanPush(card);
            targetIndex = canMove ? (int)card.Suit : -1;
            return canMove;
        }

        private bool CanMove(Tableau target, int requestedCount) =>
            Size > 0
            && this != target
            && requestedCount > 0
            && requestedCount <= CountMovable(target)
            && target.CanPush(this[requestedCount - 1]);

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state[Size++] = card.RawValue;
            SortedSize++;
            Debug.Assert(SortedSize == CountSorted());
        }

        public Card Pop()
        {
            Debug.Assert(CanPop());
            var size = --Size;
            var card = _state[size];
            _state[size] = 0;

            if (--SortedSize < 1)
            {
                SortedSize = CountSorted();
            }

            Debug.Assert(SortedSize == CountSorted());
            return Card.Get(card);
        }

        public void Move(Tableau target, int requestedCount)
        {
            Debug.Assert(CanMove(target, requestedCount));

            var poppedCards = new Card[requestedCount];
            for (var i = 0; i < requestedCount; i++)
            {
                poppedCards[i] = Pop();
            }

            for (var i = poppedCards.Length - 1; i >= 0; i--)
            {
                target.Push(poppedCards[i]);
            }
        }

        public void Move(Reserve reserve, int index)
        {
            Debug.Assert(CanMove(reserve, out var idx) && idx == index);
            reserve.Insert(index, Pop());
        }

        public void Move(Foundation foundation)
        {
            Debug.Assert(CanMove(foundation, out _));
            foundation.Push(Pop());
        }

        public int CountMovable(Tableau target)
        {
            Debug.Assert(this != target);

            var size = Size;
            var targetSize = target.Size;

            if (size == 0)
            {
                return 0;
            }

            if (targetSize == 0)
            {
                return SortedSize;
            }

            // We can safely peek because we already check for emptiness above
            var top = Card.Get(_state[size - 1]);
            var lead = Card.Get(target._state[targetSize - 1]);

            var rankDiff = lead.Rank - top.Rank;

            if (rankDiff <= 0)
            {
                return 0;
            }
            if (SortedSize < rankDiff)
            {
                return 0;
            }
            if ((rankDiff & 1) == (top.Color == lead.Color ? 1 : 0))
            {
                return 0;
            }

            Debug.Assert(target.CanPush(this[rankDiff - 1]));

            return rankDiff;
        }

        internal void Undo(Move move, Board board)
        {
            if (move.Type == MoveType.ReserveToTableau)
            {
                board.Reserve.UndoRemove(move.From, Pop());
            }
            else if (move.Type == MoveType.TableauToTableau)
            {
                var poppedCards = new short[move.Size];
                for (var i = 0; i < move.Size; i++)
                {
                    var size = --Size;
                    poppedCards[i] = _state[size];
                    _state[size] = 0;
                }

                for (var i = poppedCards.Length - 1; i >= 0; i--)
                {
                    board.Tableaus[move.From]._state[Size++] = poppedCards[i];
                }
            }

            SortedSize = CountSorted();
            board.Tableaus[move.From].SortedSize = board.Tableaus[move.From].CountSorted();
        }

        internal void UndoPop(Card card)
        {
            _state[Size++] = card.RawValue;
            SortedSize = CountSorted();
        }

        public Tableau Clone()
        {
            const int SHORT_SIZE = 2;

            var clone = new Tableau();
            Buffer.BlockCopy(_state, 0, clone._state, 0, _capacity * SHORT_SIZE);
            clone.Size = Size;
            clone.SortedSize = SortedSize;

            return clone;
        }

        private int CountSorted()
        {
            if (Size == 0)
            {
                return 0;
            }

            var sortedSize = 1;
            for (var i = 0; i < Size - 1; i++)
            {
                if (i == Size)
                {
                    break;
                }

                var card = this[i];
                if (card.IsBelow(this[i + 1]))
                {
                    sortedSize++;
                }
                else
                {
                    break;
                }
            }

            return sortedSize;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (var i = Size - 1; i >= 0; i--)
            {
                sb.AppendLine(Card.Get(_state[i]).ToString());
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.Take(Size).Select(c => Card.Get(c));

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Tableau other) => other == null
            ? false
            : _state.SequenceEqual(other._state);

        public override bool Equals(object obj) => obj is Tableau tableau && Equals(tableau);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            var size = Size;

            for (var i = 0; i < size; i++)
            {
                hash.Add(_state[i]);
            }

            return hash.ToHashCode();
        }

        public static bool operator ==(Tableau a, Tableau b) => Equals(a, b);

        public static bool operator !=(Tableau a, Tableau b) => !(a == b);
        #endregion
    }
}