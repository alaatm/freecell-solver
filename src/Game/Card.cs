using System;
using System.Diagnostics;

namespace FreeCellSolver.Game
{
    public static class Colors
    {
        public const int Black = 0;
        public const int Red = 1;
    }

    public static class Suits
    {
        public const int Clubs = 0;
        public const int Diamonds = 1;
        public const int Hearts = 2;
        public const int Spades = 3;
    }

    public static class Ranks
    {
        public const int Nil = byte.MaxValue;
        public const int Ace = 0;
        public const int R2 = 1;
        public const int R3 = 2;
        public const int R4 = 3;
        public const int R5 = 4;
        public const int R6 = 5;
        public const int R7 = 6;
        public const int R8 = 7;
        public const int R9 = 8;
        public const int R10 = 9;
        public const int Rj = 10;
        public const int Rq = 11;
        public const int Rk = 12;
    }

    public sealed class Card
    {
        private static readonly Card[] _allCards = new Card[53];

        private static readonly char[] _suits = "CDHS".ToCharArray();
        private static readonly char[] _ranks = "A23456789TJQK".ToCharArray();

        public const byte Nil = 52;
        public const Card Null = null;

        public readonly int RawValue;
        public readonly int Suit;
        public readonly int Rank;
        public readonly int Color;

        static Card()
        {
            // Pre-generate all cards
            var c = 0;
            for (var r = Ranks.Ace; r <= Ranks.Rk; r++)
            {
                for (var s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    var card = new Card(s + (r << 2));
                    _allCards[c++] = card;
                }
            }
        }

        private Card(int rawValue)
        {
            Debug.Assert(rawValue >= 0 && rawValue < 52, "Invalid card.");
            RawValue = rawValue;
            Suit = RawValue & 3;
            Rank = RawValue >> 2;
            Color = Suit == Suits.Hearts || Suit == Suits.Diamonds ? Colors.Red : Colors.Black;
        }

        // Note no error checks are made!
        public static Card Get(int rawValue) => _allCards[rawValue];

        // Note no error checks are made!
        public static Card Get(string card) => _allCards[
            Array.IndexOf(_suits, card[1]) +
            (Array.IndexOf(_ranks, card[0]) << 2)];

        // Note no error checks are made!
        public static Card Get(int suit, int rank) => _allCards[suit + (rank << 2)];

        public bool IsBelow(Card other)
            => Color != other.Color && Rank + 1 == other.Rank;

        public override string ToString()
            => $"{_ranks[Rank]}{_suits[Suit]}";
    }
}
