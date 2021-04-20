using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FreeCellSolver.Buffers;

namespace FreeCellSolver.Game
{
    public sealed class Foundation
    {
        // Index 0 -> Suit.Hearts
        // Index 1 -> Suit.Clubs
        // Index 2 -> Suit.Diamonds
        // Index 3 -> Suit.Spades
        private Arr04 _state;

        public byte this[int suit] => _state[suit];

        private Foundation() { }

        public static Foundation Create() => new();

        public static Foundation Create(int heartsTop, int clubsTop, int diamondsTop, int spadesTop)
        {
            var heartsNext = heartsTop == Ranks.Nil ? Ranks.Ace : heartsTop + 1;
            var clubsNext = clubsTop == Ranks.Nil ? Ranks.Ace : clubsTop + 1;
            var diamondsNext = diamondsTop == Ranks.Nil ? Ranks.Ace : diamondsTop + 1;
            var spadesNext = spadesTop == Ranks.Nil ? Ranks.Ace : spadesTop + 1;

            Debug.Assert(heartsNext >= Ranks.Ace && heartsNext <= Ranks.Rk + 1);
            Debug.Assert(clubsNext >= Ranks.Ace && clubsNext <= Ranks.Rk + 1);
            Debug.Assert(diamondsNext >= Ranks.Ace && diamondsNext <= Ranks.Rk + 1);
            Debug.Assert(spadesNext >= Ranks.Ace && spadesNext <= Ranks.Rk + 1);

            var f = new Foundation();
            f._state[0] = (byte)heartsNext;
            f._state[1] = (byte)clubsNext;
            f._state[2] = (byte)diamondsNext;
            f._state[3] = (byte)spadesNext;
            return f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                return _state[Suits.Hearts] >= rank
                    && _state[Suits.Diamonds] >= rank;
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

        public Foundation Clone() => new() { _state = _state };

        public bool Equals(Foundation other) => _state == other._state;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("HH CC DD SS");
            for (byte s = 0; s < 4; s++)
            {
                // Note for cases where _state[s] - 1 evaluates to -1, 
                // the byte cast will make the result 255 which is equal to Ranks.Nil
                var rank = (byte)(_state[s] - 1);
                sb.Append(rank == Ranks.Nil ? "--" : Card.Get(s, rank).ToString());
                if (s < 3)
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
            => _state.AsArray().SelectMany((nr, s) => nr != Ranks.Ace
                ? Enumerable.Range(Ranks.Ace, nr - Ranks.Ace).Select(r => Card.Get((byte)s, (byte)r))
                : Enumerable.Empty<Card>());
    }
}