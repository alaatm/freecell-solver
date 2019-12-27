using System.Diagnostics;
using System.Collections.Generic;

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

    public class Move
    {
        private static readonly Dictionary<(MoveType, int, int, int), Move> _possibleMoves = new Dictionary<(MoveType, int, int, int), Move>();

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
                for (var f = 0; f < 4; f++)
                {
                    _possibleMoves.Add((mt, r, f, 1), new Move(mt, r, f));
                }

                mt = MoveType.ReserveToTableau;
                for (var t = 0; t < 8; t++)
                {
                    _possibleMoves.Add((mt, r, t, 1), new Move(mt, r, t));
                }
            }

            for (var t = 0; t < 8; t++)
            {
                var mt = MoveType.TableauToFoundation;
                for (var f = 0; f < 4; f++)
                {
                    _possibleMoves.Add((mt, t, f, 1), new Move(mt, t, f));
                }

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

        public static Move Get(MoveType moveType, int from, int to, int size = 1) => _possibleMoves[(moveType, from, to, size)];

        internal Move(MoveType type, int from, int to, int size = 1)
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
    }
}