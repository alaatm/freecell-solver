using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class Cards
    {
        public static SKImage ToImage(this Card card)
            => DeckImage.Instance.GetCard(card.Suit, card.Rank);
    }
}