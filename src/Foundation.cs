using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public class Foundation : IEquatable<Foundation>
    {
        private readonly Dictionary<Suit, int> _state = new Dictionary<Suit, int>()
        {
            { Suit.Hearts, -1 },
            { Suit.Clubs, -1 },
            { Suit.Diamonds, -1 },
            { Suit.Spades, -1 },
        };

        public int this[Suit s] => _state[s];

        public bool IsComplete => CountPlaced == 52;

        public int CountPlaced => _state[Suit.Hearts] + _state[Suit.Clubs] + _state[Suit.Diamonds] + _state[Suit.Spades] + 4;

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

        public Foundation Clone() => new Foundation(_state[Suit.Hearts], _state[Suit.Clubs], _state[Suit.Diamonds], _state[Suit.Spades]);

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