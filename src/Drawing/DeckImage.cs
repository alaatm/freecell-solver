using FreeCellSolver.Game;
using SkiaSharp;

namespace FreeCellSolver.Drawing
{
    public class DeckImage
    {
        private readonly SKImage _img;

        public const int CardWidth = 150;
        public const int CardHeight = 225;

        public static DeckImage Instance { get; private set; }

        static DeckImage()
        {
            using var stream = typeof(DeckImage).Assembly.GetManifestResourceStream("FreeCellSolver.assets.deck.png");
            using var bmp = SKBitmap.Decode(stream);
            Instance = new DeckImage(SKImage.FromBitmap(bmp));
        }

        private DeckImage(SKImage img) => _img = img;

        public SKImage GetCard(Suit suit, Rank rank)
        {
            var s = ~(int)suit & 3;
            var r = (int)rank;

            var left = CardWidth * r;
            var top = CardHeight * s;

            return _img.Subset(SKRectI.Create(left, top, CardWidth, CardHeight));
        }
    }
}