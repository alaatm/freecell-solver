using System;
using System.IO;
using SkiaSharp;
using FreeCellSolver.Game;

namespace FreeCellSolver.Drawing.Extensions
{
    public static class BoardExtensions
    {
        public static SKImage ToImage(this Board board) => board.ToImage(1);

        public static SKImage ToImage(this Board board, float scale)
        {
            var reserveImage = board.Reserve.ToImage();
            var foundationImage = board.Foundation.ToImage();
            var dealImage = board.Tableaus.ToImage();

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

        public static void PrintMoves(this Board board, string path)
        {
            Board root = null;
            board.Traverse(b => root = b);

            if (root != null)
            {
                var replayBoard = root.Clone();
                replayBoard.ToImage().Save(Path.Join(path, "0.jpg"));

                var i = 1;
                foreach (var move in board.GetMoves())
                {
                    replayBoard.ExecuteMove(move, null);
                    replayBoard.ToImage().Save(Path.Join(path, $"{i++}.jpg"));
                }
            }
        }
    }
}