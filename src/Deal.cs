using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public static class Deal
    {

        public static Tableaus FromDealNum(int dealNum)
        {
            var cards = Enumerable.Range(0, 52).Reverse().ToList();
            var seed = dealNum;

            for (var i = 0; i < cards.Count; i++)
            {
                var pos = 51 - (int)((seed = (seed * 214013 + 2531011) & int.MaxValue) >> 16) % (52 - i);
                cards.Swap(i, pos);
            }

            var tableaus_ = new int[8][];

            for (var i = 0; i < cards.Count; i++)
            {
                var c = i % 8;
                var r = i / 8;
                if (c == i)
                {
                    tableaus_[i] = new int[c < 4 ? 7 : 6];
                }
                tableaus_[c][r] = cards[i];
            }

            var tableaus = new List<Tableau>();
            for (var c = 0; c < 8; c++)
            {
                tableaus.Add(new Tableau(tableaus_[c].Select(n => Card.Get(n)).ToArray()));
            }

            return new Tableaus(tableaus[0], tableaus[1], tableaus[2], tableaus[3], tableaus[4], tableaus[5], tableaus[6], tableaus[7]);
        }

        public static Tableaus FromString(string deal)
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

            var tableaus = new List<Tableau>();
            for (var c = 0; c < 8; c++)
            {
                tableaus.Add(new Tableau(tableaus_[c].Select(n => Card.Get(n)).ToArray()));
            }

            return new Tableaus(tableaus[0], tableaus[1], tableaus[2], tableaus[3], tableaus[4], tableaus[5], tableaus[6], tableaus[7]);
        }
    }
}