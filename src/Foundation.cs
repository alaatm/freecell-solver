using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public class Foundation : IEquatable<Foundation>
    {
        private readonly short[] _state = new short[]
        {
            -1, // Suit.Clubs
            -1, // Suit.Diamonds
            -1, // Suit.Hearts
            -1, // Suit.Spades
        };

        public short this[Suit s] => _state[(short)s];

        public bool IsComplete =>
            _state[0] + _state[1] + _state[2] + _state[3] + 4 == 52;

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

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state[(short)card.Suit]++;
        }

        internal void Undo(Move move, Board board)
        {
            var suit = move.To;
            var card = Card.Get((Suit)suit, (Rank)_state[suit]);

            if (move.Type == MoveType.ReserveToFoundation)
            {
                _state[suit]--;
                board.Reserve.UndoRemove(move.From, card);
            }
            else if (move.Type == MoveType.TableauToFoundation)
            {
                _state[suit]--;
                board.Tableaus[move.From].UndoPop(card);
            }
        }

        public Foundation Clone() => new Foundation(
            _state[0],
            _state[1],
            _state[2],
            _state[3]);

        public override string ToString()
        {
            var sb = new StringBuilder();
            var c = _state[0];
            var d = _state[1];
            var h = _state[2];
            var s = _state[3];
            sb.AppendLine("CC DD HH SS");
            sb.Append((c == -1 ? "--" : (c + 1).ToString().PadLeft(2)) + " ");
            sb.Append((d == -1 ? "--" : (d + 1).ToString().PadLeft(2)) + " ");
            sb.Append((h == -1 ? "--" : (h + 1).ToString().PadLeft(2)) + " ");
            sb.Append(s == -1 ? "--" : (s + 1).ToString().PadLeft(2));

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
        {
            foreach (var suit in Suits.Values)
            {
                for (var r = 0; r <= _state[(int)suit]; r++)
                {
                    yield return Card.Get(suit, (Rank)r);
                }
            }
        }

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Foundation other) => other == null
            ? false
            : _state[0] == other._state[0]
                && _state[1] == other._state[1]
                && _state[2] == other._state[2]
                && _state[3] == other._state[3];

        public override bool Equals(object obj) => obj is Foundation deal && Equals(deal);

        public override int GetHashCode() => HashCode.Combine(
            _state[0],
            _state[1],
            _state[2],
            _state[3]);

        public static bool operator ==(Foundation a, Foundation b) => Equals(a, b);

        public static bool operator !=(Foundation a, Foundation b) => !(a == b);
        #endregion
    }
}