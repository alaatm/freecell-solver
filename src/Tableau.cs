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
        private FastAccessStack<Card> _state = new FastAccessStack<Card>(19);

        public int Size => _state.Size;

        public bool IsEmpty => _state.Size == 0;

        public Card this[int index] => _state[index];

        public Card Top => IsEmpty ? null : _state.Peek();

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
                _state.Push(card);
            }

            SortedSize = CountSorted();
        }

        public Tableau() { }

        public bool CanPush(Card card) => IsEmpty || card.IsBelow(Top);

        public bool CanPop() => !IsEmpty;

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
            !IsEmpty
            && this != target
            && requestedCount > 0
            && requestedCount <= CountMovable(target)
            && target.CanPush(this[requestedCount - 1]);

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state.Push(card);
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
            return card;
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

            if (IsEmpty)
            {
                return 0;
            }

            if (target.IsEmpty)
            {
                return SortedSize;
            }

            var lead = target.Top;
            var rankDiff = lead.Rank - Top.Rank;

            if (rankDiff <= 0)
            {
                return 0;
            }
            if (SortedSize < rankDiff)
            {
                return 0;
            }
            if ((rankDiff & 1) == (Top.Color == lead.Color ? 1 : 0))
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
                var poppedCards = new Card[move.Size];
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
            _state.Push(card);
            SortedSize = CountSorted();
        }

        public Tableau Clone() => new Tableau
        {
            _state = _state.Clone(),
            SortedSize = this.SortedSize,
        };

        private int CountSorted()
        {
            if (IsEmpty)
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
                if (card.IsBelow(_state[i + 1]))
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
                sb.AppendLine(_state[i].ToString());
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.All();

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Tableau other) => other == null
            ? false
            : _state.SequenceEqual(other._state);

        public override bool Equals(object obj) => obj is Tableau tableau && Equals(tableau);

        public override int GetHashCode()
        {
            var hc = 27;
            for (var i = 0; i < _state.Size; i++)
            {
                var card = _state[i];
                hc = HashCode.Combine(hc, card.GetHashCode());
            }
            return hc;
        }

        public static bool operator ==(Tableau a, Tableau b) => Equals(a, b);

        public static bool operator !=(Tableau a, Tableau b) => !(a == b);
        #endregion
    }
}