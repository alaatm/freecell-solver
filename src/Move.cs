using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FreeCellSolver
{
    public enum Location
    {
        Reserve,
        Foundation,
        Tableau,
    }

    public class Move : IEquatable<Move>
    {
        private static Dictionary<string, Move> _possibleMoves = new Dictionary<string, Move>();

        public string MoveString { get; private set; }
        public Location Source { get; private set; }
        public Location Target { get; private set; }
        public int? SourceIndex { get; private set; }
        public int? TargetIndex { get; private set; }

        static Move()
        {
            // Cache all possible moves
            // Reserve -> foundation
            // Reserve -> tableau
            // Tableau -> foundation
            // Tableau -> reserve
            // Tableau -> tableau

            foreach (var c in "abcd")
            {
                var move = $"{c}h";
                _possibleMoves.Add(move, Parse(move));

                for (var t = 0; t < 8; t++)
                {
                    move = $"{c}{t}";
                    _possibleMoves.Add(move, Parse(move));
                }
            }

            for (var t = 0; t < 8; t++)
            {
                var move = $"{t}h";
                _possibleMoves.Add(move, Parse(move));

                foreach (var c in "abcd")
                {
                    move = $"{t}{c}";
                    _possibleMoves.Add(move, Parse(move));
                }

                for (var t2 = 0; t2 < 8; t2++)
                {
                    if (t != t2)
                    {
                        move = $"{t}{t2}";
                        _possibleMoves.Add(move, Parse(move));
                    }
                }
            }
        }

        public static Move Get(string move) => _possibleMoves[move];

        private static Move Parse(string move)
        {
            Debug.Assert(Regex.IsMatch(move, @"^(?:^([01234567][01234567]|[01234567][abcd]|[01234567]h|[abcd][01234567]|[abcd]h)$)$"));

            var s = move[0].ToString();
            var t = move[1].ToString();

            Location? source = null;
            Location? target = null;
            int? sourceIndex = null;
            int? targetIndex = null;

            if (Regex.IsMatch(s, "[01234567]"))
            {
                source = Location.Tableau;
                sourceIndex = int.Parse(s);
            }
            else if (Regex.IsMatch(s, "[abcd]"))
            {
                source = Location.Reserve;
                sourceIndex = "abcd".IndexOf(s);
            }

            if (Regex.IsMatch(t, "[01234567]"))
            {
                target = Location.Tableau;
                targetIndex = int.Parse(t);
            }
            else if (Regex.IsMatch(t, "[abcd]"))
            {
                target = Location.Reserve;
                targetIndex = "abcd".IndexOf(t);
            }
            else if (t == "h")
            {
                target = Location.Foundation;
            }

            return new Move(source.Value, target.Value, sourceIndex, targetIndex) { MoveString = move };
        }

        internal Move(Location source, Location target, int? sourceIndex, int? targetIndex)
        {
            Source = source;
            Target = target;
            SourceIndex = sourceIndex;
            TargetIndex = targetIndex;
        }

        public override string ToString() => MoveString;

        #region Equality overrides and overloads
        private int? _hashCode = null;
        public bool Equals([AllowNull] Move other) => other == null
            ? false
            : Source == other.Source && Target == other.Target && SourceIndex == other.SourceIndex && TargetIndex == other.TargetIndex;

        public override bool Equals(object obj) => obj is Move move && Equals(move);

        public override int GetHashCode() => _hashCode ??= HashCode.Combine(Source, Target, SourceIndex, TargetIndex);

        public static bool operator ==(Move a, Move b) => Equals(a, b);

        public static bool operator !=(Move a, Move b) => !(a == b);
        #endregion
    }
}