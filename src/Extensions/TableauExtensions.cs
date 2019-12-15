using System;
using System.Linq;
using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class TableauExtensions
    {
        public static SKImage ToImage(this Tableau tableau)
        {
            const float partialOffset = 0.27f;
            var topOffset = (int)Math.Round(DeckImage.CardHeight * partialOffset, 0);

            if (tableau.IsEmpty)
            {
                return null;
            }

            var width = DeckImage.CardWidth;
            var height = ((tableau.Stack.Count() - 1) * topOffset) + DeckImage.CardHeight;

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);
            var reversedStack = tableau.Stack.Reverse();
            for (var i = 0; i < tableau.Stack.Count(); i++)
            {
                var card = reversedStack.ElementAt(i);
                canvas.DrawImage(card.ToImage(), 0, topOffset * i);
            }

            return SKImage.FromBitmap(bmp);
        }
    }
}