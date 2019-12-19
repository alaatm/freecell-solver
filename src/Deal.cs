using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public class Deal : IEquatable<Deal>
    {
        public List<Tableau> Tableaus = new List<Tableau>(8)
        {
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
        };

        public Deal(Tableau tableau1, Tableau tableau2, Tableau tableau3, Tableau tableau4, Tableau tableau5, Tableau tableau6, Tableau tableau7, Tableau tableau8)
        {
            Tableaus[0] = tableau1.Clone();
            Tableaus[1] = tableau2.Clone();
            Tableaus[2] = tableau3.Clone();
            Tableaus[3] = tableau4.Clone();
            Tableaus[4] = tableau5.Clone();
            Tableaus[5] = tableau6.Clone();
            Tableaus[6] = tableau7.Clone();
            Tableaus[7] = tableau8.Clone();
        }

        public Deal(int dealNum)
        {
            var cards = Enumerable.Range(0, 52).Reverse().ToList();
            var seed = dealNum;

            for (var i = 0; i < cards.Count; i++)
            {
                var pos = 51 - (int)((seed = (seed * 214013 + 2531011) & int.MaxValue) >> 16) % (52 - i);
                cards.Swap(i, pos);
            }

            var tableaus = new int[8][];

            for (var i = 0; i < cards.Count; i++)
            {
                var c = i % 8;
                var r = i / 8;
                if (c == i)
                {
                    tableaus[i] = new int[c < 4 ? 7 : 6];
                }
                tableaus[c][r] = cards[i];
            }

            for (var c = 0; c < 8; c++)
            {
                Tableaus[c] = new Tableau(tableaus[c].Select(n => new Card(n)).ToList());
            }
        }

        public Deal(string deal)
        {
            if (!Regex.IsMatch(deal, @"^(?:(?:[A23456789TJQK][CDHS] ){7}[A23456789TJQK][CDHS](\r\n|\r|\n)){6}(?:[A23456789TJQK][CDHS] ){3}[A23456789TJQK][CDHS]$"))
            {
                throw new ArgumentException("Invalid deal string.");
            }

            var cards = deal.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var tableaus = new string[8][];

            for (var i = 0; i < cards.Length; i++)
            {
                var c = i % 8;
                var r = i / 8;
                if (c == i)
                {
                    tableaus[i] = new string[c < 4 ? 7 : 6];
                }
                tableaus[c][r] = cards[i];
            }

            for (var c = 0; c < 8; c++)
            {
                Tableaus[c] = new Tableau(tableaus[c].Select(n => new Card(n)).ToList());
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var r = 0; r < Tableaus.Max(t => t.Size); r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var size = Tableaus[c].Size;
                    if (size > r)
                    {
                        sb.Append(Tableaus[c][size - r - 1].ToString());
                    }
                    sb.Append(" ");
                }

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public Deal Clone() => new Deal(
            Tableaus[0],
            Tableaus[1],
            Tableaus[2],
            Tableaus[3],
            Tableaus[4],
            Tableaus[5],
            Tableaus[6],
            Tableaus[7]);

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Deal other) => other == null
            ? false
            : Tableaus.SequenceEqual(other.Tableaus);

        public override bool Equals(object obj) => obj is Deal deal && Equals(deal);

        public override int GetHashCode()
        {
            var hc = Tableaus[0].GetHashCode();
            for (var i = 1; i < Tableaus.Count; i++)
            {
                hc = HashCode.Combine(hc, Tableaus[i].GetHashCode());
            }
            return hc;
        }

        public static bool operator ==(Deal a, Deal b) => Equals(a, b);

        public static bool operator !=(Deal a, Deal b) => !(a == b);
        #endregion
    }
}