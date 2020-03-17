using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FreeCellSolver.Extensions;

namespace FreeCellSolver.Game.Extensions
{
    public static class BoardExtensions
    {
        public static Board FromDealNum(int dealNum)
        {
            var cards = Enumerable.Range(0, 52).Reverse().ToList();
            var seed = dealNum;

            for (var i = 0; i < cards.Count; i++)
            {
                seed = ((seed * 214013) + 2531011) & int.MaxValue;
                var pos = 51 - ((seed >> 16) % (52 - i));
                cards.Swap(i, pos);
            }

            var tableaus_ = new sbyte[8][];

            for (sbyte i = 0; i < cards.Count; i++)
            {
                var c = i % 8;
                var r = i / 8;
                if (c == i)
                {
                    tableaus_[i] = new sbyte[c < 4 ? 7 : 6];
                }
                tableaus_[c][r] = (sbyte)cards[i];
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

            var allCards = Enumerable.Range(0, 52).Select(c => Card.Get((sbyte)c));

            var boardCards = board.AllCards.ToList();
            var uniqueCards = new HashSet<Card>(board.AllCards);

            if (uniqueCards.Count != 52)
            {
                var missing = string.Join(", ", allCards.Except(uniqueCards).Select(c => $"'{c}'"));
                Console.Error.WriteLine($"Invalid card count, should be '52' but found '{uniqueCards.Count}' cards.");
                Console.Error.WriteLine($"The following card(s) are missing: {missing}");
                isValid = false;
            }
            else if (boardCards.Count != 52)
            {
                var duplicates = string.Join(", ", boardCards.GroupBy(x => x.RawValue).Where(g => g.Count() > 1).Select(g => $"'{Card.Get(g.Key)}'"));
                Console.Error.WriteLine($"Invalid card count, should be '52' but found '{boardCards.Count}' cards.");
                Console.Error.WriteLine($"The following card(s) are duplicates: {duplicates}");
                isValid = false;
            }

            return isValid;
        }

        public static string AsJson(this Board board)
        {
            var json = new StringBuilder();
            json.Append("[");
            for (var i = 0; i < 8; i++)
            {
                var t = board.Tableaus[i];
                json.Append("[");
                for (var j = 0; j < t.Size; j++)
                {
                    json.Append($"{t[t.Size - j - 1].RawValue},");
                }
                json.Append("],");
            }
            json.Append("];");

            return json.ToString();
        }
    }
}