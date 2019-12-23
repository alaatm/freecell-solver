using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public class Tableau : IEquatable<Tableau>
    {
        private FastAccessStack<Card> _stack = new FastAccessStack<Card>(19);

        public int Size => _stack.Size;

        public bool IsEmpty => _stack.Size == 0;

        public Card this[int index] => _stack[index];

        public Card Top => IsEmpty ? null : _stack.Peek();

        /// <summary>
        /// Returns sorted cards that are only at the bottom of the stack
        /// </summary>
        /// <value></value>
        public int SortedSize { get; private set; }

        public Tableau(string cards)
        {
            Debug.Assert(cards.Length % 2 == 0);

            for (var i = 0; i < cards.Length; i += 2)
            {
                _stack.Push(new Card(cards.Substring(i, 2)));
            }

            SortedSize = CountSorted();
        }

        public Tableau(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                _stack.Push(card);
            }

            SortedSize = CountSorted();
        }

        public Tableau() { }

        public bool CanPush(Card card) => IsEmpty || card.IsBelow(Top);

        public bool CanPop() => !IsEmpty;

        public bool CanMove(Reserve reserve, int index) => !IsEmpty && reserve.CanInsert(index, Top);

        public bool CanMove(Foundation foundation) => !IsEmpty && foundation.CanPush(Top);

        public bool CanMove(Tableau target, int requestedCount) =>
            !IsEmpty
            && this != target
            && requestedCount > 0
            && requestedCount <= CountMovable(target)
            && target.CanPush(this[requestedCount - 1]);

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _stack.Push(card);
            SortedSize++;
            Debug.Assert(SortedSize >= 1 && SortedSize <= _stack.Size);
        }

        public Card Pop()
        {
            Debug.Assert(CanPop());
            var card = _stack.Pop();

            if (--SortedSize < 1)
            {
                SortedSize = CountSorted();
            }

            Debug.Assert((SortedSize >= 1 && SortedSize <= _stack.Size) || (IsEmpty && SortedSize == 0));
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
            Debug.Assert(CanMove(reserve, index));
            reserve.Insert(index, Pop());
        }

        public void Move(Foundation foundation)
        {
            Debug.Assert(CanMove(foundation));
            foundation.Push(Pop());
        }

        public int CountMovable(Tableau target)
        {
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

            return rankDiff;
        }

        public Tableau Clone() => new Tableau
        {
            _stack = _stack.Clone(),
            SortedSize = this.SortedSize,
        };

        private int CountSorted()
        {
            if (IsEmpty)
            {
                return 0;
            }

            var sortedSize = 1;
            for (var i = 0; i < _stack.Size - 1; i++)
            {
                if (i == _stack.Size)
                {
                    break;
                }

                var card = _stack[i];
                if (card.IsBelow(_stack[i + 1]))
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

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Tableau other) => other == null
            ? false
            : _stack.SequenceEqual(other._stack);

        public override bool Equals(object obj) => obj is Tableau tableau && Equals(tableau);

        public override int GetHashCode()
        {
            var hc = 27;
            for (var i = 0; i < _stack.Size; i++)
            {
                var card = _stack[i];
                hc = HashCode.Combine(hc, card.GetHashCode());
            }
            return hc;
        }

        public static bool operator ==(Tableau a, Tableau b) => Equals(a, b);

        public static bool operator !=(Tableau a, Tableau b) => !(a == b);
        #endregion
    }
}