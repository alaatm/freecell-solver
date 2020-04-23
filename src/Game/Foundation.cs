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
        // Index 0 -> Suit.Clubs
        // Index 1 -> Suit.Diamonds
        // Index 2 -> Suit.Hearts
        // Index 3 -> Suit.Spades
        private readonly byte[] _state = new byte[4];

        public byte this[int s] => _state[s];

        public Foundation(int clubsTop, int diamondsTop, int heartsTop, int spadesTop)
        {
            var clubsNext = clubsTop + 1;
            var diamondsNext = diamondsTop + 1;
            var heartsNext = heartsTop + 1;
            var spadesNext = spadesTop + 1;

            Debug.Assert(clubsNext >= Ranks.Ace && clubsNext <= Ranks.Rk + 1);
            Debug.Assert(diamondsNext >= Ranks.Ace && diamondsNext <= Ranks.Rk + 1);
            Debug.Assert(heartsNext >= Ranks.Ace && heartsNext <= Ranks.Rk + 1);
            Debug.Assert(spadesNext >= Ranks.Ace && spadesNext <= Ranks.Rk + 1);

            _state[0] = (byte)clubsNext;
            _state[1] = (byte)diamondsNext;
            _state[2] = (byte)heartsNext;
            _state[3] = (byte)spadesNext;
        }

        public Foundation() { }

        public bool CanPush(Card card) => _state[card.Suit] == card.Rank;

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
                return _state[Suits.Diamonds] >= rank
                    && _state[Suits.Hearts] >= rank;
            }
            else
            {
                return _state[Suits.Clubs] >= rank
                    && _state[Suits.Spades] >= rank;
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
            Unsafe.CopyBlock(ref clone._state[0], ref _state[0], 4);
            return clone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Foundation other) =>
            MemoryMarshal.Cast<byte, int>(_state)[0] == MemoryMarshal.Cast<byte, int>(other._state)[0];

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CC DD HH SS");
            for (byte s = 0; s < 4; s++)
            {
                var rank = _state[s] - 1;
                sb.Append(rank == Ranks.Nil ? "--" : Card.Get(s, (byte)rank).ToString());
                if (s < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
            => _state.SelectMany((nr, s) => nr != Ranks.Ace
                ? Enumerable.Range(Ranks.Ace, nr - Ranks.Ace).Select(r => Card.Get((byte)s, (byte)r))
                : Enumerable.Empty<Card>());
    }
}