using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
        private static Dictionary<(MoveType, int, int, int), Move> _possibleMoves = new Dictionary<(MoveType, int, int, int), Move>();

        public MoveType Type { get; private set; }
        public int From { get; private set; }
        public int To { get; private set; }
        public int Size { get; private set; }

        static Move()
        {
            // Cache all possible moves
            // Reserve -> foundation
            // Reserve -> tableau
            // Tableau -> foundation
            // Tableau -> reserve
            // Tableau -> tableau

            for (var r = 0; r < 4; r++)
            {
                var mt = MoveType.ReserveToFoundation;
                _possibleMoves.Add((mt, r, -1, 1), new Move(mt, r));

                mt = MoveType.ReserveToTableau;
                for (var t = 0; t < 8; t++)
                {
                    _possibleMoves.Add((mt, r, t, 1), new Move(mt, r, t));
                }
            }

            for (var t = 0; t < 8; t++)
            {
                var mt = MoveType.TableauToFoundation;
                _possibleMoves.Add((mt, t, -1, 1), new Move(mt, t));

                mt = MoveType.TableauToReserve;
                for (var r = 0; r < 4; r++)
                {
                    _possibleMoves.Add((mt, t, r, 1), new Move(mt, t, r));
                }

                mt = MoveType.TableauToTableau;
                for (var t2 = 0; t2 < 8; t2++)
                {
                    if (t != t2)
                    {
                        for (var count = 1; count < 13; count++)
                        {
                            _possibleMoves.Add((mt, t, t2, count), new Move(mt, t, t2, count));
                        }
                    }
                }
            }
        }

        public static Move Get(MoveType moveType, int from, int to = -1, int size = 1) => _possibleMoves[(moveType, from, to, size)];

        internal Move(MoveType type, int from, int to = -1, int size = 1)
        {
            Type = type;
            From = from;
            To = to;
            Size = size;
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
                    return $"{From}{To}{{{Size}}}";
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
            : Type == other.Type && From == other.From && To == other.To && Size == other.Size;

        public override bool Equals(object obj) => obj is Move move && Equals(move);

        public override int GetHashCode() => _hashCode ??= HashCode.Combine(Type, From, To, Size);

        public static bool operator ==(Move a, Move b) => Equals(a, b);

        public static bool operator !=(Move a, Move b) => !(a == b);
        #endregion
    }
}