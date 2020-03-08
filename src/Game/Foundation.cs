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

        public short this[Suit s] => _state[(short)s];

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
            => _state[(short)card.Suit] == (short)card.Rank - 1;

        public bool CanAutoPlay(Card card)
        {
            if (!CanPush(card))
            {
                return false;
            }

            var rank = (int)card.Rank;

            if (rank <= (int)Rank.R2)
            {
                return true;
            }

            if (card.Color == Color.Black)
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
            _state[(short)card.Suit]++;
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
            for (var i = 0; i < 4; i++)
            {
                var value = _state[i];
                sb.Append((value == Card.EMPTY ? "--" : Card.Get((Suit)i, (Rank)value).ToString()));
                if (i < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
            => _state.SelectMany((v, i) => v != Card.EMPTY
                ? Enumerable.Range(0, v + 1).Select(r => Card.Get((Suit)i, (Rank)r))
                : Enumerable.Empty<Card>());
    }
}