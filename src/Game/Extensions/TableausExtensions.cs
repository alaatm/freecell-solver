using System;
using System.IO;
using System.Text;
using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Game.Extensions
{
    public static class TableausExtensions
    {
        public static SKImage ToImage(this Tableaus tableaus)
        {
            // From tableau's ToImage()
            const float partialOffset = 0.27f;
            var topOffset = (int)Math.Round(DeckImage.CardHeight * partialOffset, 0);
            var spacing = 70;

            var width = DeckImage.CardWidth * 8 + spacing * 7;
            var height = (10 * topOffset) + DeckImage.CardHeight;

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);
            for (var i = 0; i < 8; i++)
            {
                var tableauImage = tableaus[i].ToImage();
                if (tableauImage != null)
                {
                    canvas.DrawImage(
                        tableauImage,
                        i * DeckImage.CardWidth + i * spacing,
                        0);
                }
            }

            return SKImage.FromBitmap(bmp);
        }
    }
}