using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public class Foundation : IEquatable<Foundation>
    {
        private readonly Dictionary<Suit, int> _state = new Dictionary<Suit, int>()
        {
            { Suit.Clubs, -1 },
            { Suit.Diamonds, -1 },
            { Suit.Hearts, -1 },
            { Suit.Spades, -1 },
        };

        public int this[Suit s] => _state[s];

        public bool IsComplete =>
            _state[Suit.Hearts] + _state[Suit.Clubs] + _state[Suit.Diamonds] + _state[Suit.Spades] + 4 == 52;

        public Foundation(int heartsTop, int clubsTop, int diamondsTop, int spadesTop)
        {
            Debug.Assert(heartsTop >= -1 && heartsTop < 13);
            Debug.Assert(clubsTop >= -1 && clubsTop < 13);
            Debug.Assert(diamondsTop >= -1 && diamondsTop < 13);
            Debug.Assert(spadesTop >= -1 && spadesTop < 13);

            _state[Suit.Hearts] = heartsTop;
            _state[Suit.Clubs] = clubsTop;
            _state[Suit.Diamonds] = diamondsTop;
            _state[Suit.Spades] = spadesTop;
        }

        public Foundation() { }

        public bool CanPush(Card card)
            => _state[card.Suit] == (int)card.Rank - 1;

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _state[card.Suit]++;
        }

        internal void Undo(Move move, Board board)
        {
            var suit = (Suit)move.To;
            var card = Card.Get(suit, (Rank)_state[suit]);

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
            _state[Suit.Hearts],
            _state[Suit.Clubs],
            _state[Suit.Diamonds],
            _state[Suit.Spades]);

        public override string ToString()
        {
            var sb = new StringBuilder();
            var h = _state[Suit.Hearts];
            var c = _state[Suit.Clubs];
            var d = _state[Suit.Diamonds];
            var s = _state[Suit.Spades];
            sb.AppendLine("HH CC DD SS");
            sb.Append((h == -1 ? "--" : (h + 1).ToString().PadLeft(2)) + " ");
            sb.Append((c == -1 ? "--" : (c + 1).ToString().PadLeft(2)) + " ");
            sb.Append((d == -1 ? "--" : (d + 1).ToString().PadLeft(2)) + " ");
            sb.Append(s == -1 ? "--" : (s + 1).ToString().PadLeft(2));

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards()
        {
            foreach (var suit in Suits.Values)
            {
                for (var r = 0; r <= _state[suit]; r++)
                {
                    yield return Card.Get(suit, (Rank)r);
                }
            }
        }

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Foundation other) => other == null
            ? false
            : _state[Suit.Hearts] == other._state[Suit.Hearts]
                && _state[Suit.Clubs] == other._state[Suit.Clubs]
                && _state[Suit.Diamonds] == other._state[Suit.Diamonds]
                && _state[Suit.Spades] == other._state[Suit.Spades];

        public override bool Equals(object obj) => obj is Foundation deal && Equals(deal);

        public override int GetHashCode() => HashCode.Combine(
            _state[Suit.Hearts],
            _state[Suit.Clubs],
            _state[Suit.Diamonds],
            _state[Suit.Spades]);

        public static bool operator ==(Foundation a, Foundation b) => Equals(a, b);

        public static bool operator !=(Foundation a, Foundation b) => !(a == b);
        #endregion
    }
}