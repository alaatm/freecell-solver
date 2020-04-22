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
            Ranks.Nil, // Suit.Clubs
            Ranks.Nil, // Suit.Diamonds
            Ranks.Nil, // Suit.Hearts
            Ranks.Nil, // Suit.Spades
        };

        public sbyte this[int s] => _state[s];

        public Foundation(sbyte clubsTop, sbyte diamondsTop, sbyte heartsTop, sbyte spadesTop)
        {
            Debug.Assert(clubsTop >= Ranks.Nil && clubsTop <= Ranks.Rk);
            Debug.Assert(diamondsTop >= Ranks.Nil && diamondsTop <= Ranks.Rk);
            Debug.Assert(heartsTop >= Ranks.Nil && heartsTop <= Ranks.Rk);
            Debug.Assert(spadesTop >= Ranks.Nil && spadesTop <= Ranks.Rk);

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
                var rank = _state[s];
                sb.Append((rank == Ranks.Nil ? "--" : Card.Get(s, rank).ToString()));
                if (s < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
            => _state.SelectMany((r, s) => r != Ranks.Nil
                ? Enumerable.Range(Ranks.Ace, r + 1).Select(r => Card.Get((sbyte)s, (sbyte)r))
                : Enumerable.Empty<Card>());
    }
}