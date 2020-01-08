using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver.Game
{
    public class Foundation
    {
        private readonly short[] _state = new short[]
        {
            -1, // Suit.Clubs
            -1, // Suit.Diamonds
            -1, // Suit.Hearts
            -1, // Suit.Spades
        };

        public short this[Suit s] => _state[(short)s];

        public Foundation(short clubsTop, short diamondsTop, short heartsTop, short spadesTop)
        {
            Debug.Assert(clubsTop >= -1 && clubsTop < 13);
            Debug.Assert(diamondsTop >= -1 && diamondsTop < 13);
            Debug.Assert(heartsTop >= -1 && heartsTop < 13);
            Debug.Assert(spadesTop >= -1 && spadesTop < 13);

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

        public Foundation Clone() => new Foundation(
            _state[0],
            _state[1],
            _state[2],
            _state[3]);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CC DD HH SS");
            for (var i = 0; i < 4; i++)
            {
                var value = _state[i];
                sb.Append((value == -1 ? "--" : Card.Get((Suit)i, (Rank)value).ToString()));
                if (i < 3)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
            => _state.SelectMany((v, i) => v > -1
                ? Enumerable.Range(0, v + 1).Select(r => Card.Get((Suit)i, (Rank)r))
                : Enumerable.Empty<Card>());
    }
}