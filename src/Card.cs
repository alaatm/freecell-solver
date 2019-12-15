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
        private static char[] _suits = SUITS.ToCharArray();
        private static char[] _ranks = RANKS.ToCharArray();

        public const string SUITS = "CDHS";
        public const string RANKS = "A23456789TJQK";

        public readonly int Num;
        public Suit Suit => (Suit)(Num & 3);
        public Rank Rank => (Rank)(Num >> 2);
        public Color Color => Suit == Suit.Hearts || Suit == Suit.Diamonds ? Color.Red : Color.Black;

        public Card(string card) : this(
            (Suit)Array.IndexOf(_suits, card[1]),
            (Rank)Array.IndexOf(_ranks, card[0]))
        { }

        public Card(Suit suit, Rank rank) : this((int)suit + ((int)rank << 2))
        { }

        public Card(int num)
        {
            Debug.Assert(num >= 0 && num < 52, "Invalid card.");
            Num = num;
        }

        public bool IsAbove(Card foundationTop)
            => Suit == foundationTop.Suit && Rank == foundationTop.Rank + 1;

        public bool IsBelow(Card tableauTop)
            => Color != tableauTop.Color && Rank + 1 == tableauTop.Rank;

        public override string ToString()
            => $"{_ranks[(int)Rank]}{_suits[(int)Suit]}";

        #region Equality overrides and overloads
        private int? _hashCode = null;
        public bool Equals([AllowNull] Card other) => other == null
            ? false
            : Suit == other.Suit && Rank == other.Rank;

        public override bool Equals(object obj) => obj is Card card && Equals(card);

        public override int GetHashCode() => _hashCode ??= HashCode.Combine(Suit, Rank);

        public static bool operator ==(Card a, Card b) => Equals(a, b);

        public static bool operator !=(Card a, Card b) => !(a == b);
        #endregion
    }
}
