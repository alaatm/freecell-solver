using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public enum Color
    {
        Black,
        Red,
    }

    public enum Suit
    {
        Clubs = 0,
        Diamonds = 1,
        Hearts = 2,
        Spades = 3,
    }

    public enum Rank
    {
        Ace = 0,
        R2 = 1,
        R3 = 2,
        R4 = 3,
        R5 = 4,
        R6 = 5,
        R7 = 6,
        R8 = 7,
        R9 = 8,
        R10 = 9,
        RJ = 10,
        RQ = 11,
        RK = 12,
    }

    public class Card : IEquatable<Card>
    {
        private static Card[] _allCards = new Card[52];

        private static char[] _suits = SUITS.ToCharArray();
        private static char[] _ranks = RANKS.ToCharArray();
        private readonly int _rawValue;

        public const string SUITS = "CDHS";
        public const string RANKS = "A23456789TJQK";

        public Suit Suit { get; private set; }
        public Rank Rank { get; private set; }
        public Color Color { get; private set; }

        static Card()
        {
            // Pre-generate all cards
            var c = 0;
            foreach (var rank in Ranks.Values)
            {
                foreach (var suit in Suits.Values)
                {
                    var card = new Card((int)suit + ((int)rank << 2));
                    _allCards[c++] = card;
                }
            }
        }

        private Card(int rawValue)
        {
            Debug.Assert(rawValue >= 0 && rawValue < 52, "Invalid card.");
            _rawValue = rawValue;
            Suit = (Suit)(_rawValue & 3);
            Rank = (Rank)(_rawValue >> 2);
            Color = Suit == Suit.Hearts || Suit == Suit.Diamonds ? Color.Red : Color.Black;
        }

        // Note no error checks are made!
        public static Card Get(int rawValue) => _allCards[rawValue];

        // Note no error checks are made!
        public static Card Get(string card) => _allCards[
            Array.IndexOf(_suits, card[1]) +
            (Array.IndexOf(_ranks, card[0]) << 2)];

        // Note no error checks are made!
        public static Card Get(Suit suit, Rank rank) => _allCards[(int)suit + ((int)rank << 2)];

        public bool IsAbove(Card foundationTop)
            => Suit == foundationTop.Suit && Rank == foundationTop.Rank + 1;

        public bool IsBelow(Card tableauTop)
            => Color != tableauTop.Color && Rank + 1 == tableauTop.Rank;

        public override string ToString()
            => $"{_ranks[(int)Rank]}{_suits[(int)Suit]}";

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Card other) => other == null
            ? false
            : Suit == other.Suit && Rank == other.Rank;

        public override bool Equals(object obj) => obj is Card card && Equals(card);

        public override int GetHashCode() => _rawValue;

        public static bool operator ==(Card a, Card b) => Equals(a, b);

        public static bool operator !=(Card a, Card b) => !(a == b);
        #endregion
    }
}
