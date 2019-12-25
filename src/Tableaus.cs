using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FreeCellSolver
{
    public class Tableaus : IEquatable<Tableaus>
    {
        private List<Tableau> _state = new List<Tableau>(8)
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

        public Tableau this[int index] => _state[index];

        public int CardCount
            => _state[0].Size + _state[1].Size + _state[2].Size + _state[3].Size + _state[4].Size + _state[5].Size + _state[6].Size + _state[7].Size;

        public int EmptyTableauCount
        {
            get
            {
                var emptyCount = 0;

                for (var i = 0; i < 8; i++)
                {
                    if (_state[i].IsEmpty)
                    {
                        emptyCount++;
                    }
                }

                return emptyCount;
            }
        }

        public Tableaus(Tableau tableau1, Tableau tableau2, Tableau tableau3, Tableau tableau4, Tableau tableau5, Tableau tableau6, Tableau tableau7, Tableau tableau8)
        {
            _state[0] = tableau1.Clone();
            _state[1] = tableau2.Clone();
            _state[2] = tableau3.Clone();
            _state[3] = tableau4.Clone();
            _state[4] = tableau5.Clone();
            _state[5] = tableau6.Clone();
            _state[6] = tableau7.Clone();
            _state[7] = tableau8.Clone();
        }

        public bool CanReceive(Card card, int exclude)
        {
            for (var i = 0; i < 8 && card != null; i++)
            {
                if (exclude != i && _state[i].CanPush(card))
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("01 02 03 04 05 06 07 08");
            sb.AppendLine("-- -- -- -- -- -- -- --");

            for (var r = 0; r < _state.Max(t => t.Size); r++)
            {
                for (var c = 0; c < 8; c++)
                {
                    var size = _state[c].Size;
                    sb.Append(size > r ? _state[c][size - r - 1].ToString() : "  ");
                    sb.Append(c < 7 ? " " : "");
                }

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public Tableaus Clone() => new Tableaus(
            _state[0],
            _state[1],
            _state[2],
            _state[3],
            _state[4],
            _state[5],
            _state[6],
            _state[7]);

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Tableaus other) => other == null
            ? false
            : _state.SequenceEqual(other._state);

        public override bool Equals(object obj) => obj is Tableaus deal && Equals(deal);

        public override int GetHashCode()
        {
            var hc = _state[0].GetHashCode();
            for (var i = 1; i < _state.Count; i++)
            {
                hc = HashCode.Combine(hc, _state[i].GetHashCode());
            }
            return hc;
        }

        public static bool operator ==(Tableaus a, Tableaus b) => Equals(a, b);

        public static bool operator !=(Tableaus a, Tableaus b) => !(a == b);
        #endregion
    }
}