using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Foundation
    {
        private readonly sbyte[] _state =
        {
            Card.Empty, // Suit.Clubs
            Card.Empty, // Suit.Diamonds
            Card.Empty, // Suit.Hearts
            Card.Empty, // Suit.Spades
        };

        public sbyte this[int s] => _state[s];

        public Foundation(sbyte clubsTop, sbyte diamondsTop, sbyte heartsTop, sbyte spadesTop)
        {
            Debug.Assert(clubsTop >= Card.Empty && clubsTop < 13);
            Debug.Assert(diamondsTop >= Card.Empty && diamondsTop < 13);
            Debug.Assert(heartsTop >= Card.Empty && heartsTop < 13);
            Debug.Assert(spadesTop >= Card.Empty && spadesTop < 13);

            _state[0] = clubsTop;
            _state[1] = diamondsTop;
            _state[2] = heartsTop;
            _state[3] = spadesTop;
        }

        public Foundation() { }

        public bool CanPush(Card card) => _state[card.Suit] == card.Rank - 1;

        public bool CanAutoPlay(Card card)
        {
            if (!CanPush(card))
            {
                return false;
            }

            var rank = card.Rank;

            if (rank <= Ranks.R2)
            {
                return true;
            }

            if (card.Color == Colors.Black)
            {
                return _state[Suits.Diamonds] >= rank - 1
                    && _state[Suits.Hearts] >= rank - 1;
            }
            else
            {
                return _state[Suits.Clubs] >= rank - 1
                    && _state[Suits.Spades] >= rank - 1;
            }
        }

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state[card.Suit]++;
        }

        public Foundation Clone()
        {
            var clone = new Foundation();
            Unsafe.CopyBlock(ref Unsafe.As<sbyte, byte>(ref clone._state[0]), ref Unsafe.As<sbyte, byte>(ref _state[0]), 4);
            return clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Foundation other) =>
            MemoryMarshal.Cast<sbyte, int>(_state)[0] == MemoryMarshal.Cast<sbyte, int>(other._state)[0];

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CC DD HH SS");
            for (sbyte s = 0; s < 4; s++)
            {
                var value = _state[s];
                sb.Append((value == Card.Empty ? "--" : Card.Get(s, value).ToString()));
                if (s < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
            => _state.SelectMany((v, s) => v != Card.Empty
                ? Enumerable.Range(0, v + 1).Select(r => Card.Get((sbyte)s, (sbyte)r))
                : Enumerable.Empty<Card>());
    }
}