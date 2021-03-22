using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

                for (var i = 0; i < 8; i++)
                {
                    if (_state[i].Size == 0)
                    {
                        emptyCount++;
                    }
                }

                return emptyCount;
            }
        }

        public Tableaus(params Tableau[] tableaus)
        {
            Debug.Assert(tableaus.Length <= 8);

            for (var i = 0; i < tableaus.Length; i++)
            {
                _state[i] = tableaus[i].Clone();
            }

            for (var i = tableaus.Length; i < 8; i++)
            {
                _state[i] = new Tableau();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Tableaus(Tableaus copy)
        {
            _state[0] = copy._state[0].Clone();
            _state[1] = copy._state[1].Clone();
            _state[2] = copy._state[2].Clone();
            _state[3] = copy._state[3].Clone();
            _state[4] = copy._state[4].Clone();
            _state[5] = copy._state[5].Clone();
            _state[6] = copy._state[6].Clone();
            _state[7] = copy._state[7].Clone();
        }

        public Tableaus Clone() => new Tableaus(this);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("01 02 03 04 05 06 07 08");
            sb.AppendLine("-- -- -- -- -- -- -- --");

            var maxSize = _state.Max(t => t.Size);

            for (var r = 0; r < maxSize; r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var size = _state[c].Size;
                    sb.Append(size > r ? _state[c][size - r - 1].ToString() : "  ");
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