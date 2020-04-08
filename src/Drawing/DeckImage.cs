using System;
using SkiaSharp;

namespace FreeCellSolver.Drawing
{
    public class DeckImage
    {
        private readonly SKImage _img;
        private static readonly Lazy<DeckImage> _instance = new Lazy<DeckImage>(Initialize);

        public const int CardWidth = 140;
        public const int CardHeight = 210;

        public static DeckImage Instance => _instance.Value;

        static DeckImage Initialize()
        {
            using var stream = typeof(DeckImage).Assembly.GetManifestResourceStream("FreeCellSolver.assets.deck.png");
            using var bmp = SKBitmap.Decode(stream);
            return new DeckImage(SKImage.FromBitmap(bmp));
        }

        private DeckImage(SKImage img) => _img = img;

        public SKImage GetCard(int suit, int rank)
        {
            var s = ~suit & 3;
            var r = rank;

            var left = CardWidth * r;
            var top = CardHeight * s;

            return _img.Subset(SKRectI.Create(left, top, CardWidth, CardHeight));
        }
    }
}