using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public static Board FromDealNum(int dealNum)
        {
            var cards = Enumerable.Range(0, 52).Reverse().ToList();
            var seed = dealNum;

            for (var i = 0; i < cards.Count; i++)
            {
                var pos = 51 - (int)((seed = (seed * 214013 + 2531011) & int.MaxValue) >> 16) % (52 - i);
                cards.Swap(i, pos);
            }

            var tableaus_ = new short[8][];

            for (var i = 0; i < cards.Count; i++)
            {
                var c = (short)(i % 8);
                var r = (short)(i / 8);
                if (c == i)
                {
                    tableaus_[i] = new short[c < 4 ? 7 : 6];
                }
                tableaus_[c][r] = (short)cards[i];
            }

            var tableaus = new List<Tableau>(8);
            for (var c = 0; c < 8; c++)
            {
                tableaus.Add(new Tableau(tableaus_[c].Select(n => Card.Get(n)).ToArray()));
            }

            return new Board(new Tableaus(tableaus[0], tableaus[1], tableaus[2], tableaus[3], tableaus[4], tableaus[5], tableaus[6], tableaus[7]));
        }

        public static Board FromString(string deal)
        {
            if (!Regex.IsMatch(deal, @"^(?:(?:[A23456789TJQK][CDHS] ){7}[A23456789TJQK][CDHS](\r\n|\r|\n)){6}(?:[A23456789TJQK][CDHS] ){3}[A23456789TJQK][CDHS]$"))
            {
                throw new ArgumentException("Invalid deal string.");
            }

            var cards = deal.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var tableaus_ = new string[8][];

            for (var i = 0; i < cards.Length; i++)
            {
                var c = i % 8;
                var r = i / 8;
                if (c == i)
                {
                    tableaus_[i] = new string[c < 4 ? 7 : 6];
                }
                tableaus_[c][r] = cards[i];
            }

            var tableaus = new List<Tableau>(8);
            for (var c = 0; c < 8; c++)
            {
                tableaus.Add(new Tableau(tableaus_[c].Select(n => Card.Get(n)).ToArray()));
            }

            return new Board(new Tableaus(tableaus[0], tableaus[1], tableaus[2], tableaus[3], tableaus[4], tableaus[5], tableaus[6], tableaus[7]));
        }

        public static bool IsValid(this Board board)
        {
            var isValid = true;
            ConsoleErrorWriter.Set();
            var stdErr = Console.Error;

            var allCards = Enumerable.Range(0, 52).Select(c => Card.Get((short)c));

            var boardCards = board.AllCards.ToList();
            var uniqueCards = new HashSet<Card>(board.AllCards);

            if (uniqueCards.Count != 52)
            {
                var missing = String.Join(", ", allCards.Except(uniqueCards).Select(c => $"'{c.ToString()}'"));
                stdErr.WriteLine($"Invalid card count, should be '52' but found '{uniqueCards.Count}' cards.");
                stdErr.WriteLine($"The following card(s) are missing: {missing}");
                isValid = false;
            }
            else if (boardCards.Count != 52)
            {
                var duplicates = String.Join(", ", boardCards.GroupBy(x => x.RawValue).Where(g => g.Count() > 1).Select(g => $"'{Card.Get(g.Key).ToString()}'"));
                stdErr.WriteLine($"Invalid card count, should be '52' but found '{boardCards.Count}' cards.");
                stdErr.WriteLine($"The following card(s) are duplicates: {duplicates}");
                isValid = false;
            }

            return isValid;
        }
    }
}