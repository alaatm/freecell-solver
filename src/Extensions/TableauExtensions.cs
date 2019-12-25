using System;
using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class TableauExtensions
    {
        public static SKImage ToImage(this Tableau tableau)
        {
            const float partialOffset = 0.26f;
            var topOffset = (int)Math.Round(DeckImage.CardHeight * partialOffset, 0);

            if (tableau.IsEmpty)
            {
                return null;
            }

            var width = DeckImage.CardWidth;
            var height = ((tableau.Size - 1) * topOffset) + DeckImage.CardHeight;

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);
            for (var i = tableau.Size - 1; i >= 0; i--)
            {
                var card = tableau[i];
                canvas.DrawImage(card.ToImage(), 0, topOffset * (tableau.Size - i - 1));
            }

            return SKImage.FromBitmap(bmp);
        }
    }
}