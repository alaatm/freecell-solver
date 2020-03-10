using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Foundation
    {
        private readonly sbyte[] _state = new sbyte[]
        {
            Card.EMPTY, // Suit.Clubs
            Card.EMPTY, // Suit.Diamonds
            Card.EMPTY, // Suit.Hearts
            Card.EMPTY, // Suit.Spades
        };

        public sbyte this[int s] => _state[s];

        public Foundation(sbyte clubsTop, sbyte diamondsTop, sbyte heartsTop, sbyte spadesTop)
        {
            Debug.Assert(clubsTop >= Card.EMPTY && clubsTop < 13);
            Debug.Assert(diamondsTop >= Card.EMPTY && diamondsTop < 13);
            Debug.Assert(heartsTop >= Card.EMPTY && heartsTop < 13);
            Debug.Assert(spadesTop >= Card.EMPTY && spadesTop < 13);

            _state[0] = clubsTop;
            _state[1] = diamondsTop;
            _state[2] = heartsTop;
            _state[3] = spadesTop;
        }

        public Foundation() { }

        public bool CanPush(Card card)
            => _state[card.Suit] == card.Rank - 1;

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

            if (card.Color == Colors.BLACK)
            {
                return _state[Suits.DIAMONDS] >= rank - 1
                    && _state[Suits.HEARTS] >= rank - 1;
            }
            else
            {
                return _state[Suits.CLUBS] >= rank - 1
                    && _state[Suits.SPADES] >= rank - 1;
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CC DD HH SS");
            for (sbyte s = 0; s < 4; s++)
            {
                var value = _state[s];
                sb.Append((value == Card.EMPTY ? "--" : Card.Get(s, value).ToString()));
                if (s < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
            => _state.SelectMany((v, s) => v != Card.EMPTY
                ? Enumerable.Range(0, v + 1).Select(r => Card.Get((sbyte)s, (sbyte)r))
                : Enumerable.Empty<Card>());
    }
}