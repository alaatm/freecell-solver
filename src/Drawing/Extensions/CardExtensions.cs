using SkiaSharp;
using FreeCellSolver.Game;

namespace FreeCellSolver.Drawing.Extensions
{
    public static class CardExtensions
    {
        public static SKImage ToImage(this Card card)
            => DeckImage.Instance.GetCard(card.Suit, card.Rank);
    }
}