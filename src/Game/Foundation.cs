using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public class Foundation
    {
        private readonly short[] _state = new short[]
        {
            Card.EMPTY, // Suit.Clubs
            Card.EMPTY, // Suit.Diamonds
            Card.EMPTY, // Suit.Hearts
            Card.EMPTY, // Suit.Spades
        };

        public short this[int s] => _state[s];

        public Foundation(short clubsTop, short diamondsTop, short heartsTop, short spadesTop)
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
                return _state[1/*Suit.Diamonds*/] >= rank - 1
                    && _state[2/*Suit.Hearts*/] >= rank - 1;
            }
            else
            {
                return _state[0/*Suit.Clubs*/] >= rank - 1
                    && _state[3/*Suit.Spades*/] >= rank - 1;
            }
        }

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state[card.Suit]++;
        }

        public Foundation Clone()
        {
            const int SHORT_SIZE = 2;

            var clone = new Foundation();
            Unsafe.CopyBlock(ref Unsafe.As<short, byte>(ref clone._state[0]), ref Unsafe.As<short, byte>(ref _state[0]), 4 * SHORT_SIZE);
            return clone;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CC DD HH SS");
            for (byte s = 0; s < 4; s++)
            {
                var value = _state[s];
                sb.Append((value == Card.EMPTY ? "--" : Card.Get(s, (byte)value).ToString()));
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
                ? Enumerable.Range(0, v + 1).Select(r => Card.Get((byte)s, (byte)r))
                : Enumerable.Empty<Card>());
    }
}