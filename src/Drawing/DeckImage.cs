using System;
using SkiaSharp;

namespace FreeCellSolver.Drawing
{
    public sealed class DeckImage
    {
        private readonly SKImage _img;
        private static readonly Lazy<DeckImage> _instance = new(Initialize);

        private const int CardWidth = 140;
        private const int CardHeight = 210;

        public static DeckImage Instance => _instance.Value;

        private static DeckImage Initialize()
        {
            using var stream = typeof(DeckImage).Assembly.GetManifestResourceStream("FreeCellSolver.assets.deck.png");
            using var bmp = SKBitmap.Decode(stream);
            return new DeckImage(SKImage.FromBitmap(bmp));
        }

        private DeckImage(SKImage img) => _img = img;

        public SKImage GetCard(int suit, int rank)
        {
            var s = suit;
            var r = rank - 1; // Ace is rank 1 so we subtract 1

            var left = CardWidth * r;
            var top = CardHeight * s;

            return _img.Subset(SKRectI.Create(left, top, CardWidth, CardHeight));
        }
    }
}