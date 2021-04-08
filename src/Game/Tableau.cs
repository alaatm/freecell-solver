using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FreeCellSolver.Buffers;

namespace FreeCellSolver.Game
{
    public sealed class Tableau
    {
        private Arr18 _state;

        public Card Top { get; private set; }

        public int Size { get; private set; }

        public int SortedSize { get; private set; }

        public bool IsEmpty => Size == 0;

        public Card this[int index] => Card.Get(_state[index]);

        private Tableau() { }

        public static Tableau Create() => new();

        public static Tableau Create(string cards) =>
            Create(new[] { 0 }.SelectMany(i => cards
                .Replace(" ", "")
                .GroupBy(_ => i++ / 2)
                .Select(g => (byte)Card.Get(string.Join("", g)).RawValue)
            ).ToArray());

        internal static Tableau Create(Span<byte> cards)
        {
            var t = new Tableau();

            for (var i = 0; i < cards.Length; i++)
            {
                t._state[t.Size++] = cards[i];
            }

            t.SortedSize = t.CountSorted();
            return t;
        }

        public bool CanPush(Card card) => Size == 0 || card.IsBelow(Top);

        public bool CanPop() => Size > 0;

        public bool CanMove(Reserve reserve, out int index)
        {
            index = -1;
            return CanPop() && reserve.CanInsert(out index);
        }

        public bool CanMove(Foundation foundation)
            => CanPop() && foundation.CanPush(Top);

        private bool CanMove(Tableau target, int requestedCount) =>
            Size > 0
            && this != target
            && requestedCount > 0
            && requestedCount <= CountMovable(target)
            && target.CanPush(this[Size - requestedCount]);

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state[Size++] = (byte)card.RawValue;
            SortedSize++;
            Top = card;
            Debug.Assert(SortedSize == CountSorted());
        }

        public Card Pop()
        {
            Debug.Assert(CanPop());
            var size = --Size;
            var card = _state[size];

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

        public bool Move(Tableau target, int requestedCount)
        {
            Debug.Assert(CanMove(target, requestedCount));
            var stillSorted = SortedSize > requestedCount || Size == requestedCount;

            if (requestedCount == 1)
            {
                target.Push(Pop());
                return stillSorted;
            }

            _state.CopyTo(ref target._state, Size - requestedCount, target.Size, requestedCount);

            target.Top = Top;
            target.Size += requestedCount;
            target.SortedSize += requestedCount;

            Size -= requestedCount;
            if ((SortedSize -= requestedCount) < 1)
            {
                SortedSize = CountSorted();
            }
            else
            {
                Top = Card.Get(_state[Size - 1]);
            }

            Debug.Assert(SortedSize == CountSorted());
            return stillSorted;
        }

        public void Move(Reserve reserve, int index)
        {
            Debug.Assert(CanMove(reserve, out var idx) && idx == index);
            reserve.Insert(index, Pop());
        }

        public void Move(Foundation foundation)
        {
            Debug.Assert(CanMove(foundation));
            foundation.Push(Pop());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountMovable(Tableau target)
        {
            Debug.Assert(this != target);

            if (Size == 0)
            {
                return 0;
            }

            if (target.Size == 0)
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

            Debug.Assert(target.CanPush(this[Size - rankDiff]));

            return rankDiff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tableau Clone() => new()
        {
            _state = _state,
            Top = Top,
            Size = Size,
            SortedSize = SortedSize,
        };

        private int CountSorted()
        {
            Debug.Assert(Size >= 0);

            var size = Size;
            var state = _state;

            if (size <= 1)
            {
                Top = size == 0 ? Card.Null : Top = Card.Get(state[size - 1]);
                return size;
            }

            Top = Card.Get(state[size - 1]);

            var i = 0;
            var current = Top;
            var sortedSize = 1;
            do
            {
                var above = Card.Get(state[size - i - 2]);
                if (current.IsBelow(above))
                {
                    sortedSize++;
                }
                else
                {
                    break;
                }

                current = above;
                i++;
            } while (i < size - 1);

            return sortedSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Tableau other)
        {
            var size = Size;
            var sortedSize = SortedSize;

            if (sortedSize != other.SortedSize || size != other.Size)
            {
                return false;
            }

            if (size == 0)
            {
                Debug.Assert(other.Size == 0);
                return true;
            }

            if (Top != other.Top)
            {
                return false;
            }

            // At this point, both should have same size, same sorted size, same unsorted size and same top
            Debug.Assert(size == other.Size && sortedSize == other.SortedSize && Top == other.Top);
            // At this point, min size=1, min sorted=1, min unsorted=0
            Debug.Assert(size >= 1 && sortedSize >= 1);
            // Since all tableaus have a common ancestor, at this point (same size, same sorted size, same unsorted size, same top)
            // then the unsorted portion of both tableaus should be exactly the same
            Debug.Assert(_state.SequenceEqual(other._state, 0, size - sortedSize));

            // Equal if sorted portion on both sides is the same
            return _state.SequenceEqual(other._state, size - sortedSize, sortedSize - 1);
        }

        public override string ToString()
        {
            var size = Size;
            var sb = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                sb.Append(Card.Get(_state[i]));
                if (i < size - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards() => _state.AsArray().Take(Size).Select(c => Card.Get(c));
    }
}