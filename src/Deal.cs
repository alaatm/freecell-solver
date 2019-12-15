using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public class Deal : IEquatable<Deal>
    {
        private readonly List<Tableau> _tableaus = new List<Tableau>(8);

        public IEnumerable<Tableau> Tableaus => _tableaus.AsReadOnly();

        public Deal(IEnumerable<Tableau> tableaus) => _tableaus = new List<Tableau>(tableaus);

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
                _tableaus.Add(new Tableau(c, tableaus[c].Select(n => new Card(n)).ToList()));
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
                _tableaus.Add(new Tableau(c, tableaus[c].Select(n => new Card(n)).ToList()));
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var r = 0; r < Tableaus.Max(t => t.Stack.Count()); r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var stack = Tableaus.ElementAt(c).Stack.Reverse();
                    if (stack.Count() > r)
                    {
                        sb.Append(stack.ElementAt(r).ToString());
                    }
                    sb.Append(" ");
                }

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public Deal Clone() => new Deal(_tableaus.Select(t => t.Clone()));

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Deal other) => other == null
            ? false
            : _tableaus.SequenceEqual(other._tableaus);

        public override bool Equals(object obj) => obj is Deal deal && Equals(deal);

        public override int GetHashCode()
        {
            var hc = _tableaus[0].GetHashCode();
            for (var i = 1; i < _tableaus.Count; i++)
            {
                hc = HashCode.Combine(hc, _tableaus[i].GetHashCode());
            }
            return hc;
        }

        public static bool operator ==(Deal a, Deal b) => Equals(a, b);

        public static bool operator !=(Deal a, Deal b) => !(a == b);
        #endregion
    }
}