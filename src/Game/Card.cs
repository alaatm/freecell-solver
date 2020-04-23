using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Colors
    {
        public const byte Black = 0;
        public const byte Red = 1;
    }

    public sealed class Suits
    {
        public const byte Clubs = 0;
        public const byte Diamonds = 1;
        public const byte Hearts = 2;
        public const byte Spades = 3;
    }

    public sealed class Ranks
    {
        public const int Nil = -1;
        public const byte Ace = 0;
        public const byte R2 = 1;
        public const byte R3 = 2;
        public const byte R4 = 3;
        public const byte R5 = 4;
        public const byte R6 = 5;
        public const byte R7 = 6;
        public const byte R8 = 7;
        public const byte R9 = 8;
        public const byte R10 = 9;
        public const byte Rj = 10;
        public const byte Rq = 11;
        public const byte Rk = 12;
    }

    public sealed class Card
    {
        private static readonly Card[] _allCards = new Card[52];

        private static readonly char[] _suits = "CDHS".ToCharArray();
        private static readonly char[] _ranks = "A23456789TJQK".ToCharArray();

        public const byte Nil = byte.MaxValue;
        public const Card Null = null;

        public byte RawValue { get; }
        public byte Suit { get; }
        public byte Rank { get; }
        public byte Color { get; }

        static Card()
        {
            // Pre-generate all cards
            var c = 0;
            for (var r = Ranks.Ace; r <= Ranks.Rk; r++)
            {
                for (var s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    var card = new Card((byte)(s + (r << 2)));
                    _allCards[c++] = card;
                }
            }
        }

        private Card(byte rawValue)
        {
            Debug.Assert(rawValue >= 0 && rawValue < 52, "Invalid card.");
            RawValue = rawValue;
            Suit = (byte)(RawValue & 3);
            Rank = (byte)(RawValue >> 2);
            Color = Suit == Suits.Hearts || Suit == Suits.Diamonds ? Colors.Red : Colors.Black;
        }

        // Note no error checks are made!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Card Get(int rawValue) => _allCards[rawValue];

        // Note no error checks are made!
        public static Card Get(string card) => _allCards[
            Array.IndexOf(_suits, card[1]) +
            (Array.IndexOf(_ranks, card[0]) << 2)];

        // Note no error checks are made!
        public static Card Get(byte suit, byte rank) => _allCards[suit + (rank << 2)];

        public bool IsBelow(Card tableauTop)
            => Color != tableauTop.Color && Rank + 1 == tableauTop.Rank;

        public override string ToString()
            => $"{_ranks[Rank]}{_suits[Suit]}";
    }
}
