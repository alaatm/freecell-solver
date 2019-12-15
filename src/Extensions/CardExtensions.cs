using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class Cards
    {
        // Spades
        public static Card AceOfSpades => new Card("AS");
        public static Card TwoOfSpades => new Card("2S");
        public static Card ThreeOfSpades => new Card("3S");
        public static Card FourOfSpades => new Card("4S");
        public static Card FiveOfSpades => new Card("5S");
        public static Card SixOfSpades => new Card("6S");
        public static Card SevenOfSpades => new Card("7S");
        public static Card EightfSpades => new Card("8S");
        public static Card NineOfSpades => new Card("9S");
        public static Card TenOfSpades => new Card("TS");
        public static Card JackOfSpades => new Card("JS");
        public static Card QueenOfSpades => new Card("QS");
        public static Card KingOfSpades => new Card("KS");

        // Hearts
        public static Card AceOfHearts => new Card("AH");
        public static Card TwoOfHearts => new Card("2H");
        public static Card ThreeOfHearts => new Card("3H");
        public static Card FourOfHearts => new Card("4H");
        public static Card FiveOfHearts => new Card("5H");
        public static Card SixOfHearts => new Card("6H");
        public static Card SevenOfHearts => new Card("7H");
        public static Card EightfHearts => new Card("8H");
        public static Card NineOfHearts => new Card("9H");
        public static Card TenOfHearts => new Card("TH");
        public static Card JackOfHearts => new Card("JH");
        public static Card QueenOfHearts => new Card("QH");
        public static Card KingOfHearts => new Card("KH");

        // Diamonds
        public static Card AceOfDiamonds => new Card("AD");
        public static Card TwoOfDiamonds => new Card("2D");
        public static Card ThreeOfDiamonds => new Card("3D");
        public static Card FourOfDiamonds => new Card("4D");
        public static Card FiveOfDiamonds => new Card("5D");
        public static Card SixOfDiamonds => new Card("6D");
        public static Card SevenOfDiamonds => new Card("7D");
        public static Card EightfDiamonds => new Card("8D");
        public static Card NineOfDiamonds => new Card("9D");
        public static Card TenOfDiamonds => new Card("TD");
        public static Card JackOfDiamonds => new Card("JD");
        public static Card QueenOfDiamonds => new Card("QD");
        public static Card KingOfDiamonds => new Card("KD");

        // Clubs
        public static Card AceOfClubs => new Card("AC");
        public static Card TwoOfClubs => new Card("2C");
        public static Card ThreeOfClubs => new Card("3C");
        public static Card FourOfClubs => new Card("4C");
        public static Card FiveOfClubs => new Card("5C");
        public static Card SixOfClubs => new Card("6C");
        public static Card SevenOfClubs => new Card("7C");
        public static Card EightfClubs => new Card("8C");
        public static Card NineOfClubs => new Card("9C");
        public static Card TenOfClubs => new Card("TC");
        public static Card JackOfClubs => new Card("JC");
        public static Card QueenOfClubs => new Card("QC");
        public static Card KingOfClubs => new Card("KC");

        public static SKImage ToImage(this Card card)
            => DeckImage.Instance.GetCard(card.Suit, card.Rank);

    }
}