using System;
using System.Diagnostics;
using FreeCellSolver.Game.Extensions;

namespace FreeCellSolver.Game
{
    public static class Colors
    {
        public const int Red = 0;
        public const int Black = 1;
    }

    public static class Suits
    {
        public const int Hearts = 0;
        public const int Clubs = 1;
        public const int Diamonds = 2;
        public const int Spades = 3;
    }

    public static class Ranks
    {
        public const int Nil = 0;
        public const int Ace = 1;
        public const int R2 = 2;
        public const int R3 = 3;
        public const int R4 = 4;
        public const int R5 = 5;
        public const int R6 = 6;
        public const int R7 = 7;
        public const int R8 = 8;
        public const int R9 = 9;
        public const int R10 = 10;
        public const int Rj = 11;
        public const int Rq = 12;
        public const int Rk = 13;
    }

    public readonly struct Card : IEquatable<Card>
    {
        private static readonly char[] _suits = "HCDS".ToCharArray();
        private static readonly char[] _ranks = "A23456789TJQK".ToCharArray();

        public const byte Nil = 0;
        public static readonly Card Null = default;

        public readonly byte RawValue;
        public int Suit => RawValue & 3;
        public int Rank => RawValue >> 2;
        public int Color => RawValue & 1;

        private Card(int rawValue)
        {
            Debug.Assert((rawValue >= 4 && rawValue < 56) || rawValue == 0, "Invalid card.");
            RawValue = (byte)rawValue;
        }

        // Note no error checks are made!
        public static Card Get(int rawValue) => new(rawValue);

        // Note no error checks are made!
        public static Card Get(string card) => card.IsEmpty()
            ? default
            : new(
                Array.IndexOf(_suits, card[1]) +
                ((Array.IndexOf(_ranks, card[0]) + 1) << 2));

        // Note no error checks are made!
        public static Card Get(int suit, int rank) => new(suit + (rank << 2));

        public static Card[] All()
        {
            var i = 0;
            var cards = new Card[52];
            for (var r = Ranks.Ace; r <= Ranks.Rk; r++)
            {
                for (var s = Suits.Hearts; s <= Suits.Spades; s++)
                {
                    cards[i++] = Get(s, r);
                }
            }

            return cards;
        }

        public bool IsBelow(Card other)
            => Rank + 1 == other.Rank && Color != other.Color;

        public override string ToString() => RawValue == 0
            ? "--"
            : $"{_ranks[Rank - 1]}{_suits[Suit]}";

        public override int GetHashCode() => RawValue;

        public bool Equals(Card other) => RawValue == other.RawValue;

        public override bool Equals(object obj) => throw new NotImplementedException();

        public static bool operator ==(Card left, Card right) => left.Equals(right);

        public static bool operator !=(Card left, Card right) => !left.Equals(right);
    }
}
