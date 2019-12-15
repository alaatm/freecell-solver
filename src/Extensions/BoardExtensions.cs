using System;
using SkiaSharp;

namespace FreeCellSolver.Extensions
{
    public static class BoardExtensions
    {
        public static SKImage ToImage(this Board board) => board.ToImage(1);

        public static SKImage ToImage(this Board board, float scale)
        {
            var reserveImage = board.Reserve.ToImage();
            var foundationImage = board.Foundation.ToImage();
            var dealImage = board.Deal.ToImage();

            var topMargin = 60;
            var bottomMargin = 100;
            var horizontalMargin = 150;
            var verticalSpacing = 60;

            var width = dealImage.Width + horizontalMargin * 2;
            var height = reserveImage.Height + verticalSpacing + dealImage.Height + topMargin + bottomMargin;

            var topLeftX = (width / 2 - reserveImage.Width) / 2;
            var topRightX = topLeftX + width / 2;

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);
            using var paint = new SKPaint { Color = SKColor.Parse("1b5e20") };

            canvas.DrawRect(0, 0, width, height, paint);
            canvas.DrawImage(reserveImage, topLeftX, topMargin);
            canvas.DrawImage(foundationImage, topRightX, topMargin);
            canvas.DrawImage(dealImage, horizontalMargin, topMargin + reserveImage.Height + verticalSpacing);

            if (scale > 0 && scale < 1)
            {
                var w = (int)Math.Round(width * scale, 0);
                var h = (int)Math.Round(height * scale, 0);

                bmp = bmp.Resize(new SKImageInfo(w, h), SKFilterQuality.High);
            }

            return SKImage.FromBitmap(bmp);
        }

        // 00:00:00.0901497
        public static Board GetFastBoard() => new Board(new Deal(5911382));
        // 00:00:16.2200133
        public static Board GetNormalBoard() => new Board(new Deal(1024999));
        public static Board GetSlowBoard() => new Board(new Deal(
@"QH 6C 8D 4D 3C 8C 8H 9S
QC 7D 9D TS KH AC 2C 7S
5D AS 2H TD JS 3H AH JC
8S 7C 6S TC 2S 6D AD 9C
KD JH 3S 6H 5C QS 3D 7H
5H 2D 9H 4S JD 4H 5S KC
KS 4C QD TH"));
        // >> 10 MIN
        public static Board GetExtremlySlowBoard() => new Board(new Deal(2401571));
    }
}