using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver.Game
{
    public static class Tableaus
    {
        public static Tableau[] Create(params Tableau[] tableaus)
        {
            Debug.Assert(tableaus.Length <= 8);
            var ts = new Tableau[8];

            for (var i = 0; i < tableaus.Length; i++)
            {
                ts[i] = tableaus[i].Clone();
            }

            for (var i = tableaus.Length; i < 8; i++)
            {
                ts[i] = Tableau.Create();
            }

            return ts;
        }

        public static int EmptyCount(this Tableau[] tableaus)
        {
            var emptyCount = 0;
            var ts = tableaus;

            for (var i = 0; i < ts.Length; i++)
            {
                if (ts[i].Size == 0)
                {
                    emptyCount++;
                }
            }

            return emptyCount;
        }

        public static Tableau[] CloneX(this Tableau[] tableaus) => new[]
        {
            tableaus[0].Clone(),
            tableaus[1].Clone(),
            tableaus[2].Clone(),
            tableaus[3].Clone(),
            tableaus[4].Clone(),
            tableaus[5].Clone(),
            tableaus[6].Clone(),
            tableaus[7].Clone(),
        };

        public static string Dump(this Tableau[] tableaus)
        {
            var sb = new StringBuilder();
            sb.AppendLine("00 01 02 03 04 05 06 07");
            sb.AppendLine("-- -- -- -- -- -- -- --");

            var maxSize = tableaus.Max(t => t.Size);

            for (var r = 0; r < maxSize; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var size = tableaus[c].Size;
                    sb.Append(size > r ? tableaus[c][r].ToString() : "  ");
                    sb.Append(c < 7 ? " " : "");
                }

                if (r < maxSize - 1)
                {
                    sb.Append(Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        // Used only for post moves asserts
        internal static IEnumerable<Card> AllCards(this Tableau[] tableaus) => tableaus.SelectMany(t => t.AllCards());
    }
}