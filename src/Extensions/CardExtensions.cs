using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class Cards
    {
        // Spades
        public static Card AceOfSpades => Card.Get("AS");
        public static Card TwoOfSpades => Card.Get("2S");
        public static Card ThreeOfSpades => Card.Get("3S");
        public static Card FourOfSpades => Card.Get("4S");
        public static Card FiveOfSpades => Card.Get("5S");
        public static Card SixOfSpades => Card.Get("6S");
        public static Card SevenOfSpades => Card.Get("7S");
        public static Card EightfSpades => Card.Get("8S");
        public static Card NineOfSpades => Card.Get("9S");
        public static Card TenOfSpades => Card.Get("TS");
        public static Card JackOfSpades => Card.Get("JS");
        public static Card QueenOfSpades => Card.Get("QS");
        public static Card KingOfSpades => Card.Get("KS");

        // Hearts
        public static Card AceOfHearts => Card.Get("AH");
        public static Card TwoOfHearts => Card.Get("2H");
        public static Card ThreeOfHearts => Card.Get("3H");
        public static Card FourOfHearts => Card.Get("4H");
        public static Card FiveOfHearts => Card.Get("5H");
        public static Card SixOfHearts => Card.Get("6H");
        public static Card SevenOfHearts => Card.Get("7H");
        public static Card EightfHearts => Card.Get("8H");
        public static Card NineOfHearts => Card.Get("9H");
        public static Card TenOfHearts => Card.Get("TH");
        public static Card JackOfHearts => Card.Get("JH");
        public static Card QueenOfHearts => Card.Get("QH");
        public static Card KingOfHearts => Card.Get("KH");

        // Diamonds
        public static Card AceOfDiamonds => Card.Get("AD");
        public static Card TwoOfDiamonds => Card.Get("2D");
        public static Card ThreeOfDiamonds => Card.Get("3D");
        public static Card FourOfDiamonds => Card.Get("4D");
        public static Card FiveOfDiamonds => Card.Get("5D");
        public static Card SixOfDiamonds => Card.Get("6D");
        public static Card SevenOfDiamonds => Card.Get("7D");
        public static Card EightfDiamonds => Card.Get("8D");
        public static Card NineOfDiamonds => Card.Get("9D");
        public static Card TenOfDiamonds => Card.Get("TD");
        public static Card JackOfDiamonds => Card.Get("JD");
        public static Card QueenOfDiamonds => Card.Get("QD");
        public static Card KingOfDiamonds => Card.Get("KD");

        // Clubs
        public static Card AceOfClubs => Card.Get("AC");
        public static Card TwoOfClubs => Card.Get("2C");
        public static Card ThreeOfClubs => Card.Get("3C");
        public static Card FourOfClubs => Card.Get("4C");
        public static Card FiveOfClubs => Card.Get("5C");
        public static Card SixOfClubs => Card.Get("6C");
        public static Card SevenOfClubs => Card.Get("7C");
        public static Card EightfClubs => Card.Get("8C");
        public static Card NineOfClubs => Card.Get("9C");
        public static Card TenOfClubs => Card.Get("TC");
        public static Card JackOfClubs => Card.Get("JC");
        public static Card QueenOfClubs => Card.Get("QC");
        public static Card KingOfClubs => Card.Get("KC");

        public static SKImage ToImage(this Card card)
            => DeckImage.Instance.GetCard(card.Suit, card.Rank);

    }
}