using System.Linq;
using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class ReserveExtensions
    {
        public static SKImage ToImage(this Reserve freeCells)
        {
            var list = freeCells.State;

            var spacing = 60;
            var width = DeckImage.CardWidth * 4 + spacing * 3;
            var height = DeckImage.CardHeight;

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);
            using var paint = new SKPaint { Color = new SKColor(255, 255, 255, 82) };
            using var bgPaint = new SKPaint { Color = SKColor.Parse("1b5e20") };
            for (var i = 0; i < list.Count(); i++)
            {
                var x = i * DeckImage.CardWidth + i * spacing;

                if (list.ElementAt(i) != null)
                {
                    canvas.DrawImage(list.ElementAt(i).ToImage(), x, 0);
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