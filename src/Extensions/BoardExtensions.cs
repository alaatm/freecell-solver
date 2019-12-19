using System;
using System.IO;
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


        public static Board GetExtremlyFastBoard() => new Board(Deal.FromString(
@"QH 6C 8D 4D 3C 8C 8H 9S
QC 7D 9D TS KH QD 2C 7S
5D AS 2H TD JS 3H AH JC
8S 7C 6S TC 2S 6D AD 9C
KD JH 3S 6H 5C QS 3D 7H
5H 2D 9H 4S JD 4H 5S KC
KS 4C AC TH"));
        // 00:00:00.0901497
        public static Board GetFastBoard() => new Board(Deal.FromDealNum(5911382));
        // 00:00:16.2200133
        public static Board GetNormalBoard() => new Board(Deal.FromDealNum(1024999));
        public static Board GetSlowBoard() => new Board(Deal.FromString(
@"QH 6C 8D 4D 3C 8C 8H 9S
QC 7D 9D TS KH AC 2C 7S
5D AS 2H TD JS 3H AH JC
8S 7C 6S TC 2S 6D AD 9C
KD JH 3S 6H 5C QS 3D 7H
5H 2D 9H 4S JD 4H 5S KC
KS 4C QD TH"));
        // >> 28 MIN
        public static Board GetExtremlySlowBoard() => new Board(Deal.FromDealNum(2401571));

        public static Board GetSolitareDeck() => new Board(Deal.FromString(
@"QC 5H 2D AS QH QD 4C AD
KS 7C AC TC 8D QS 2C 4H
6H 6S 7H 5D 7S AH KD 3D
9H 5C 2H JC 9S 7D TS 6C
8S TH 9C 4S KC 5S KH 3C
JD 8H 3H 6D 8C JH TD 9D
4D 2S 3S JS"));

        public static Board GetTestBoard()
        {
            var t1 = new Tableau("5CKD");
            var t2 = new Tableau("TC9D8C");
            var t3 = new Tableau("");
            var t4 = new Tableau("QD");
            var t5 = new Tableau("JC");
            var t6 = new Tableau("7C");
            var t7 = new Tableau("KC");
            var t8 = new Tableau("QCJD");
            var reserve = new Reserve(Cards.TenOfDiamonds, Cards.SixOfClubs, Cards.NineOfClubs, null);
            var foundation = new Foundation(12, 3, 7, 12);
            var board = new Board(new Tableaus(t1, t2, t3, t4, t5, t6, t7, t8), reserve, foundation);

            return board;
        }
    }
}