using System;
using System.Diagnostics;
using FreeCellSolver.Game.Shared;

namespace FreeCellSolver.Game
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

    public class Card
    {
        private static readonly Card[] _allCards = new Card[52];

        private static readonly char[] _suits = SUITS.ToCharArray();
        private static readonly char[] _ranks = RANKS.ToCharArray();

        public const short EMPTY = -1;
        public const string SUITS = "CDHS";
        public const string RANKS = "A23456789TJQK";

        public short RawValue { get; private set; }
        public Suit Suit { get; private set; }
        public Rank Rank { get; private set; }
        public Color Color { get; private set; }

        static Card()
        {
            // Pre-generate all cards
            var c = 0;
            foreach (var rank in Ranks.Values())
            {
                foreach (var suit in Suits.Values())
                {
                    var card = new Card((short)((short)suit + ((short)rank << 2)));
                    _allCards[c++] = card;
                }
            }
        }

        private Card(short rawValue)
        {
            Debug.Assert(rawValue >= 0 && rawValue < 52, "Invalid card.");
            RawValue = rawValue;
            Suit = (Suit)(RawValue & 3);
            Rank = (Rank)(RawValue >> 2);
            Color = Suit == Suit.Hearts || Suit == Suit.Diamonds ? Color.Red : Color.Black;
        }

        // Note no error checks are made!
        public static Card Get(short rawValue) => rawValue < 0 ? null : _allCards[rawValue];

        // Note no error checks are made!
        public static Card Get(string card) => _allCards[
            Array.IndexOf(_suits, card[1]) +
            (Array.IndexOf(_ranks, card[0]) << 2)];

        // Note no error checks are made!
        public static Card Get(Suit suit, Rank rank) => _allCards[(short)suit + ((short)rank << 2)];

        public bool IsBelow(Card tableauTop)
            => Color != tableauTop.Color && Rank + 1 == tableauTop.Rank;

        public override string ToString()
            => $"{_ranks[(short)Rank]}{_suits[(short)Suit]}";
    }
}
