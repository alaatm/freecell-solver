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
        private FastAccessStack _state = new FastAccessStack();

        public int Size => _state.Size;

        public bool IsEmpty => _state.Size == 0;

        public Card this[int index] => Card.Get(_state[index]);

        public Card Top => _state.Size == 0 ? null : Card.Get(_state.Peek());

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
                _state.Push(card.RawValue);
            }

            SortedSize = CountSorted();
        }

        public Tableau() { }

        public bool CanPush(Card card) => _state.Size == 0 || card.IsBelow(Top);

        public bool CanPop() => _state.Size > 0;

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
            _state.Size > 0
            && this != target
            && requestedCount > 0
            && requestedCount <= CountMovable(target)
            && target.CanPush(this[requestedCount - 1]);

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state.Push(card.RawValue);
            SortedSize++;
            Debug.Assert(SortedSize == CountSorted());
        }

        public Card Pop()
        {
            Debug.Assert(CanPop());
            var card = _state.Pop();

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

            if (_state.Size == 0)
            {
                return 0;
            }

            if (target._state.Size == 0)
            {
                return SortedSize;
            }

            // We can safely peek because we already check for emptiness above
            var top = Card.Get(_state.Peek());
            var lead = Card.Get(target._state.Peek());

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
                    poppedCards[i] = _state.Pop();
                }

                for (var i = poppedCards.Length - 1; i >= 0; i--)
                {
                    board.Tableaus[move.From]._state.Push(poppedCards[i]);
                }
            }

            SortedSize = CountSorted();
            board.Tableaus[move.From].SortedSize = board.Tableaus[move.From].CountSorted();
        }

        internal void UndoPop(Card card)
        {
            _state.Push(card.RawValue);
            SortedSize = CountSorted();
        }

        public Tableau Clone() => new Tableau
        {
            _state = _state.Clone(),
            SortedSize = this.SortedSize,
        };

        private int CountSorted()
        {
            if (_state.Size == 0)
            {
                return 0;
            }

            var sortedSize = 1;
            for (var i = 0; i < _state.Size - 1; i++)
            {
                if (i == _state.Size)
                {
                    break;
                }

                var card = _state[i];
                if (Card.Get(card).IsBelow(Card.Get(_state[i + 1])))
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
        internal IEnumerable<Card> AllCards() => _state.All().Select(c => Card.Get(c));

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Tableau other) => other == null
            ? false
            : _state.SequenceEqual(other._state);

        public override bool Equals(object obj) => obj is Tableau tableau && Equals(tableau);

        public override int GetHashCode()
        {
            var hash = new HashCode();

            for (var i = 0; i < _state.Size; i++)
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