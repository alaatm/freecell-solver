using FreeCellSolver.Game;
using SkiaSharp;

namespace FreeCellSolver.Drawing
{
    public class DeckImage
    {
        private readonly SKImage _img;
        private readonly int _offset = 15;

        public const int CardWidth = 180;
        public const int CardHeight = 270;

        public static DeckImage Instance { get; private set; }

        static DeckImage()
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream("FreeCellSolver.assets.deck.png");
            using var bmp = SKBitmap.Decode(stream);
            Instance = new DeckImage(SKImage.FromBitmap(bmp));
        }

        private DeckImage(SKImage img) => _img = img;

        public SKImage GetCard(Suit suit, Rank rank)
        {
            var s = ~(int)suit & 3;
            var r = (int)rank;

            var left = CardWidth * r + (_offset * (r + 1));
            var top = CardHeight * s + (_offset * (s + 1));

            return _img.Subset(SKRectI.Create(left, top, CardWidth, CardHeight));
        }
    }
}