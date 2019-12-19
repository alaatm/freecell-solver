using System;
using System.IO;
using System.Text;
using SkiaSharp;
using FreeCellSolver.Drawing;

namespace FreeCellSolver.Extensions
{
    public static class TableausExtensions
    {
        public static SKImage ToImage(this Tableaus deal)
        {
            // From tableau's ToImage()
            const float partialOffset = 0.27f;
            var topOffset = (int)Math.Round(DeckImage.CardHeight * partialOffset, 0);
            var spacing = 70;

            var width = DeckImage.CardWidth * 8 + spacing * 7;
            var height = (6 * topOffset) + DeckImage.CardHeight;

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);
            for (var i = 0; i < 8; i++)
            {
                var tableauImage = deal[i].ToImage();
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

        public static bool IsValid(this Tableaus deal)
        {
            var isValid = true;
            ConsoleErrorWriter.Set();
            var stdErr = Console.Error;

            var totalCardCount = deal.CardCount;
            if (totalCardCount != 52)
            {
                stdErr.WriteLine($"Total card count is invalid, should be '52' but is '{totalCardCount}'.");
                isValid = false;
            }

            for (var i = 0; i < 4; i++)
            {
                if (deal[i].Size != 7)
                {
                    stdErr.WriteLine($"Tableau #{i + 1} has incorrect number of cards.");
                    isValid = false;
                }
            }

            for (var i = 4; i < 8; i++)
            {
                if (deal[i].Size != 6)
                {
                    stdErr.WriteLine($"Tableau #{i + 1} has incorrect number of cards.");
                    isValid = false;
                }
            }

            var deck = Deck.Get();

            for (var t = 0; t < 8; t++)
            {
                var tableau = deal[t];
                for (var i = 0; i < tableau.Size; i++)
                {
                    var card = tableau[i];
                    if (!deck.Contains(card))
                    {
                        stdErr.WriteLine($"Card '{card.ToString()}' is duplicate at tableau #{i + 1}");
                        isValid = false;
                    }
                    deck.Remove(card);
                }
            }

            return isValid;
        }
    }

    public class ConsoleErrorWriter : TextWriter
    {
        private TextWriter _stdErr;

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