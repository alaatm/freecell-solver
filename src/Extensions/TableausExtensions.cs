using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
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

    public class ConsoleErrorWriter : TextWriter
    {
        private readonly TextWriter _stdErr;

        public ConsoleErrorWriter(TextWriter stdErr) => _stdErr = stdErr;

        public override void WriteLine(string value)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _stdErr.WriteLine(value);
            Console.ResetColor();
        }

        public override void Write(string value)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _stdErr.Write(value);
            Console.ResetColor();
        }

        public override Encoding Encoding => Encoding.Default;

        public static void Set()
            => Console.SetError(new ConsoleErrorWriter(Console.Error));
    }
}