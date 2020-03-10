using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Tableau
    {
        private const int _capacity = 19;
        private readonly sbyte[] _state = new sbyte[_capacity]
        {
            Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY,
            Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY, Card.EMPTY
        };

        public Card Top { get; private set; }

        public int Size { get; private set; }

        public int SortedSize { get; private set; }

        public bool IsEmpty => Size == 0;

        public Card this[int index] => Card.Get(_state[Size - index - 1]);

        public Tableau(string cards) : this(
            new[] { 0 }.SelectMany(i => cards
                .GroupBy(_ => i++ / 2)
                .Select(g => Card.Get(String.Join("", g)))
            ).ToArray())
        { }

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
            return size == 0 || card.IsBelow(Top);
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
            targetIndex = canMove ? card.Suit : -1;
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
            Top = card;
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
            else
            {
                Top = Card.Get(_state[size - 1]);
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
            var top = Top;
            var lead = target.Top;

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

        public Tableau Clone()
        {
            var size = Size;

            var clone = new Tableau();
            if (size > 0)
            {
                Unsafe.CopyBlock(ref Unsafe.As<sbyte, byte>(ref clone._state[0]), ref Unsafe.As<sbyte, byte>(ref _state[0]), (uint)size);
                clone.Top = Top;
                clone.Size = size;
                clone.SortedSize = SortedSize;
            }

            return clone;
        }

        private int CountSorted()
        {
            var size = Size;

            if (size == 0)
            {
                Top = null;
                return 0;
            }

            Top = Card.Get(_state[size - 1]);
            var sortedSize = 1;
            for (var i = 0; i < size - 1; i++)
            {
                var current = Card.Get(_state[size - i - 1]);
                var above = Card.Get(_state[size - (i + 1) - 1]);
                if (current.IsBelow(above))
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
            var size = Size;
            var sb = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                sb.Append(Card.Get(_state[i]).ToString());
                if (i < size - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.Take(Size).Select(c => Card.Get(c));
    }
}