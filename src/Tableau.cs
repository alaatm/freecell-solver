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

        public IEnumerable<Card> SortedStack => _stack.Take(SortedSize);

        public Card Top => IsEmpty ? null : _stack.Peek();

        /// <summary>
        /// Returns sorted cards that are only at the top of the stack
        /// </summary>
        /// <value></value>
        public int SortedSize
        {
            get
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
        }

        public Tableau(string cards)
        {
            Debug.Assert(cards.Length % 2 == 0);

            for (var i = 0; i < cards.Length; i += 2)
            {
                _stack.Push(new Card(cards.Substring(i, 2)));
            }
        }

        public Tableau(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                _stack.Push(card);
            }
        }

        public Tableau() { }

        public bool CanPush(Card card) => IsEmpty || card.IsBelow(Top);

        public bool CanPop() => !IsEmpty;

        public bool CanMove(Reserve reserve, int index) => !IsEmpty && reserve.CanInsert(index, Top);

        public bool CanMove(Foundation foundation) => !IsEmpty && foundation.CanPush(Top);

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _stack.Push(card);
        }

        public Card Pop()
        {
            Debug.Assert(CanPop());
            return _stack.Pop();
        }

        public void Move(Tableau target, int requestedCount)
        {
            var movableCount = CountMovable(target);

            Debug.Assert(!IsEmpty);
            Debug.Assert(this != target);
            Debug.Assert(requestedCount <= movableCount);

            var poppedCards = new List<Card>();
            for (var i = 0; i < requestedCount; i++)
            {
                poppedCards.Add(Pop());
            }

            poppedCards.Reverse();
            foreach (var card in poppedCards)
            {
                target.Push(card);
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

            var movable = SortedSize;
            foreach (var card in SortedStack.Reverse())
            {
                if (card.IsBelow(target.Top))
                {
                    break;
                }
                else
                {
                    movable--;
                }
            }

            return movable;
        }

        public Tableau Clone() => new Tableau
        {
            _stack = _stack.Clone()
        };

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