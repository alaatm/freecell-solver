using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver.Game
{
    public sealed class Tableaus
    {
        private readonly Tableau[] _state = new Tableau[8];

        public Tableau this[int index] => _state[index];

        public int EmptyTableauCount
        {
            get
            {
                var emptyCount = 0;
                var state = _state;

                for (var i = 0; i < state.Length; i++)
                {
                    if (state[i].Size == 0)
                    {
                        emptyCount++;
                    }
                }

                return emptyCount;
            }
        }

        private Tableaus() { }

        public static Tableaus Create(params Tableau[] tableaus)
        {
            Debug.Assert(tableaus.Length <= 8);
            var ts = new Tableaus();

            for (var i = 0; i < tableaus.Length; i++)
            {
                ts._state[i] = tableaus[i].Clone();
            }

            for (var i = tableaus.Length; i < 8; i++)
            {
                ts._state[i] = Tableau.Create();
            }

            return ts;
        }

        public Tableaus Clone()
        {
            var copy = new Tableaus();
            copy._state[0] = _state[0].Clone();
            copy._state[1] = _state[1].Clone();
            copy._state[2] = _state[2].Clone();
            copy._state[3] = _state[3].Clone();
            copy._state[4] = _state[4].Clone();
            copy._state[5] = _state[5].Clone();
            copy._state[6] = _state[6].Clone();
            copy._state[7] = _state[7].Clone();
            return copy;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("00 01 02 03 04 05 06 07");
            sb.AppendLine("-- -- -- -- -- -- -- --");

            var maxSize = _state.Max(t => t.Size);

            for (var r = 0; r < maxSize; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var size = _state[c].Size;
                    sb.Append(size > r ? _state[c][r].ToString() : "  ");
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
        internal IEnumerable<Card> AllCards() => _state.SelectMany(t => t.AllCards());
    }
}