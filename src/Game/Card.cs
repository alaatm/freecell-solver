using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Colors
    {
        public const sbyte Black = 0;
        public const sbyte Red = 1;
    }

    public sealed class Suits
    {
        public const sbyte Clubs = 0;
        public const sbyte Diamonds = 1;
        public const sbyte Hearts = 2;
        public const sbyte Spades = 3;
    }

    public sealed class Ranks
    {
        public const sbyte Ace = 0;
        public const sbyte R2 = 1;
        public const sbyte R3 = 2;
        public const sbyte R4 = 3;
        public const sbyte R5 = 4;
        public const sbyte R6 = 5;
        public const sbyte R7 = 6;
        public const sbyte R8 = 7;
        public const sbyte R9 = 8;
        public const sbyte R10 = 9;
        public const sbyte Rj = 10;
        public const sbyte Rq = 11;
        public const sbyte Rk = 12;
    }

    public sealed class Card
    {
        private static readonly Card[] _allCards = new Card[52];

        private static readonly char[] _suits = "CDHS".ToCharArray();
        private static readonly char[] _ranks = "A23456789TJQK".ToCharArray();

        public const sbyte Empty = -1;

        public sbyte RawValue { get; }
        public sbyte Suit { get; }
        public sbyte Rank { get; }
        public sbyte Color { get; }

        static Card()
        {
            // Pre-generate all cards
            var c = 0;
            for (var r = Ranks.Ace; r <= Ranks.Rk; r++)
            {
                for (var s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    var card = new Card((sbyte)(s + (r << 2)));
                    _allCards[c++] = card;
                }
            }
        }

        private Card(sbyte rawValue)
        {
            Debug.Assert(rawValue >= 0 && rawValue < 52, "Invalid card.");
            RawValue = rawValue;
            Suit = (sbyte)(RawValue & 3);
            Rank = (sbyte)(RawValue >> 2);
            Color = Suit == Suits.Hearts || Suit == Suits.Diamonds ? Colors.Red : Colors.Black;
        }

        // Note no error checks are made!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Card Get(int rawValue) => rawValue == Empty ? null : _allCards[rawValue];

        // Note no error checks are made!
        public static Card Get(string card) => _allCards[
            Array.IndexOf(_suits, card[1]) +
            (Array.IndexOf(_ranks, card[0]) << 2)];

        // Note no error checks are made!
        public static Card Get(sbyte suit, sbyte rank) => _allCards[suit + (rank << 2)];

        public bool IsBelow(Card tableauTop)
            => Color != tableauTop.Color && Rank + 1 == tableauTop.Rank;

        public override string ToString()
            => $"{_ranks[Rank]}{_suits[Suit]}";
    }
}
