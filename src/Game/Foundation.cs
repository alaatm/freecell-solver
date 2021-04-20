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
            Debug.Assert(heartsTop >= Ranks.Nil && heartsTop <= Ranks.Rk);
            Debug.Assert(clubsTop >= Ranks.Nil && clubsTop <= Ranks.Rk);
            Debug.Assert(diamondsTop >= Ranks.Nil && diamondsTop <= Ranks.Rk);
            Debug.Assert(spadesTop >= Ranks.Nil && spadesTop <= Ranks.Rk);

            var f = new Foundation();
            f._state[0] = (byte)heartsTop;
            f._state[1] = (byte)clubsTop;
            f._state[2] = (byte)diamondsTop;
            f._state[3] = (byte)spadesTop;
            return f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                return _state[Suits.Hearts] >= rank - 1
                    && _state[Suits.Diamonds] >= rank - 1;
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

        public Foundation Clone() => new() { _state = _state };

        public bool Equals(Foundation other) => _state == other._state;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("HH CC DD SS");
            for (byte s = 0; s < 4; s++)
            {
                var rank = _state[s];
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
            => _state.AsArray().SelectMany((nr, s) => 
                Enumerable.Range(Ranks.Ace, nr).Select(r => Card.Get((byte)s, (byte)r)));
    }
}