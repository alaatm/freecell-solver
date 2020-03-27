using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FreeCellSolver.Game.Extensions
{
    public static class BoardExtensions
    {
        public static Board FromDealNum(int dealNum)
        {
            Span<sbyte> crv = stackalloc sbyte[] 
            { 
                51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41,
                40, 39, 38, 37, 36, 35, 34, 33, 32, 31,
                30, 29, 28, 27, 26, 25, 24, 23, 22, 21,
                20, 19, 18, 17, 16, 15, 14, 13, 12, 11,
                10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 
            };
            var seed = dealNum;

            for (var i = 0; i < crv.Length; i++)
            {
                seed = ((seed * 214013) + 2531011) & int.MaxValue;
                var pos = 51 - ((seed >> 16) % (52 - i));

                var tmp = crv[i];
                crv[i] = crv[pos];
                crv[pos] = tmp;
            }

            // IDX: 0  8  16 24 32 40 48
            // T0 : JD KD 2S 4C 3S 6D 6S

            // IDX: 1  9  17 25 33 41 49
            // T1 : 2D KC KS 5C TD 8S 9C

            // IDX: 2  10 18 26 34 42 50
            // T2 : 9H 9S 9D TS 4S 8D 2H

            // IDX: 3  11 19 27 35 43 51
            // T3 : JC 5S QD QH TH QS 6H

            // IDX: 4  12 20 28 36 44
            // T4 : 5D AD JS 4H 8H 6C

            // IDX: 5  13 21 29 37 45
            // T5 : 7H QC AS AC 2C 3D

            // IDX: 6  14 22 30 38 46
            // T6 : 7C KH AH 4D JH 8C

            // IDX: 7  15 23 31 39 47
            // T7 : 5H 3H 3C 7S 7D TC

            var col = 0;
            var tableaus = new Tableau[8];
            Span<sbyte> cards = stackalloc sbyte[8];

            for (var i = 0; i < crv.Length;)
            {
                var count = col < 4 ? 7 : 6;

                for (var j = 0; j < count; j++)
                {
                    cards[j] = crv[col + (j * 8)];
                }

                tableaus[col++] = new Tableau(cards.Slice(0, count));
                i += count;
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