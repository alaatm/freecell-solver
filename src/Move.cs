using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FreeCellSolver
{
    public enum MoveType
    {
        None,
        TableauToFoundation,
        TableauToReserve,
        TableauToTableau,
        ReserveToFoundation,
        ReserveToTableau,
    }

    public class Move : IEquatable<Move>
    {
        private static Dictionary<string, Move> _possibleMoves = new Dictionary<string, Move>();

        public MoveType Type { get; private set; }
        public int From { get; private set; }
        public int To { get; private set; }

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

            var fromTableau = Regex.IsMatch(s, "[01234567]");
            var toTableau = Regex.IsMatch(t, "[01234567]");
            var fromReserve = Regex.IsMatch(s, "[abcd]");
            var toReserve = Regex.IsMatch(t, "[abcd]");
            var toHome = t == "h";

            const string r = "abcd";

            if (fromTableau && toHome)
            {
                return new Move(MoveType.TableauToFoundation, int.Parse(s));
            }
            else if (fromTableau && toTableau)
            {
                return new Move(MoveType.TableauToTableau, int.Parse(s), int.Parse(t));
            }
            else if (fromTableau && toReserve)
            {
                return new Move(MoveType.TableauToReserve, int.Parse(s), r.IndexOf(t));
            }
            else if (fromReserve && toHome)
            {
                return new Move(MoveType.ReserveToFoundation, r.IndexOf(s));
            }
            else if (fromReserve && toTableau)
            {
                return new Move(MoveType.ReserveToTableau, r.IndexOf(s), int.Parse(t));
            }

            Debug.Assert(false);
            return null;
        }

        internal Move(MoveType type, int from, int to = -1)
        {
            Type = type;
            From = from;
            To = to;
        }

        public override string ToString()
        {
            const string r = "abcd";

            switch (Type)
            {
                case MoveType.TableauToFoundation:
                    return $"{From}h";
                case MoveType.TableauToReserve:
                    return $"{From}{r[To]}";
                case MoveType.TableauToTableau:
                    return $"{From}{To}";
                case MoveType.ReserveToFoundation:
                    return $"{r[From]}h";
                case MoveType.ReserveToTableau:
                    return $"{r[From]}{To}";
            }

            Debug.Assert(false);
            return null;
        }

        #region Equality overrides and overloads
        private int? _hashCode = null;
        public bool Equals([AllowNull] Move other) => other == null
            ? false
            : Type == other.Type && From == other.From && To == other.To;

        public override bool Equals(object obj) => obj is Move move && Equals(move);

        public override int GetHashCode() => _hashCode ??= HashCode.Combine(Type, From, To);

        public static bool operator ==(Move a, Move b) => Equals(a, b);

        public static bool operator !=(Move a, Move b) => !(a == b);
        #endregion
    }
}