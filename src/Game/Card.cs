using System;
using System.Diagnostics;

namespace FreeCellSolver.Game
{
    public sealed class Colors
    {
        public const byte BLACK = 0;
        public const byte RED = 1;
    }

    public sealed class Suits
    {
        public const byte CLUBS = 0;
        public const byte DIAMONDS = 1;
        public const byte HEARTS = 2;
        public const byte SPADES = 3;
    }

    public sealed class Ranks
    {
        public const byte ACE = 0;
        public const byte R2 = 1;
        public const byte R3 = 2;
        public const byte R4 = 3;
        public const byte R5 = 4;
        public const byte R6 = 5;
        public const byte R7 = 6;
        public const byte R8 = 7;
        public const byte R9 = 8;
        public const byte R10 = 9;
        public const byte RJ = 10;
        public const byte RQ = 11;
        public const byte RK = 12;
    }

    public class Card
    {
        private static readonly Card[] _allCards = new Card[52];

        private static readonly char[] _suits = SUITS.ToCharArray();
        private static readonly char[] _ranks = RANKS.ToCharArray();

        public const short EMPTY = -1;
        public const string SUITS = "CDHS";
        public const string RANKS = "A23456789TJQK";

        public short RawValue { get; private set; }
        public byte Suit { get; private set; }
        public byte Rank { get; private set; }
        public byte Color { get; private set; }

        static Card()
        {
            // Pre-generate all cards
            var c = 0;
            for (var r = Ranks.ACE; r <= Ranks.RK; r++)
            {
                for (var s = Suits.CLUBS; s <= Suits.SPADES; s++)
                {
                    var card = new Card((short)(s + (r << 2)));
                    _allCards[c++] = card;
                }
            }
        }

        private Card(short rawValue)
        {
            Debug.Assert(rawValue >= 0 && rawValue < 52, "Invalid card.");
            RawValue = rawValue;
            Suit = (byte)(RawValue & 3);
            Rank = (byte)(RawValue >> 2);
            Color = Suit == Suits.HEARTS || Suit == Suits.DIAMONDS ? Colors.RED : Colors.BLACK;
        }

        // Note no error checks are made!
        public static Card Get(short rawValue) => rawValue < 0 ? null : _allCards[rawValue];

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
