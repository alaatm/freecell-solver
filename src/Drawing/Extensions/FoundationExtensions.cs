using SkiaSharp;
using FreeCellSolver.Game;
using FreeCellSolver.Game.Helpers;

namespace FreeCellSolver.Drawing.Extensions
{
    public static class FoundationExtensions
    {
        public static SKImage ToImage(this Foundation foundation)
        {
            var spacing = 60;
            var width = DeckImage.CardWidth * 4 + spacing * 3;
            var height = DeckImage.CardHeight;

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);
            using var paint = new SKPaint { Color = new SKColor(255, 255, 255, 82) };
            using var bgPaint = new SKPaint { Color = SKColor.Parse("1b5e20") };
            for (var i = 0; i < Suits.Values.Length; i++)
            {
                var x = i * DeckImage.CardWidth + i * spacing;
                var suit = Suits.Values[i];
                var rank = foundation[suit];
                var card = rank >= 0 ? Card.Get(suit, (Rank)rank) : null;

                if (card != null)
                {
                    canvas.DrawImage(card.ToImage(), x, 0);
                }
                else
                {
                    canvas.DrawRoundRect(x, 0, DeckImage.CardWidth, height, 10, 10, paint);
                    canvas.DrawRoundRect(x + 10, 10, DeckImage.CardWidth - 20, height - 20, 10, 10, bgPaint);
                }
            }

            return SKImage.FromBitmap(bmp);
        }
    }
}