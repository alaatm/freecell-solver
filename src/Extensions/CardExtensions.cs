using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class CardExtensions
    {
        public static SKImage ToImage(this Card card)
            => DeckImage.Instance.GetCard(card.Suit, card.Rank);
    }
}