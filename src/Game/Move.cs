using System.Diagnostics;

namespace FreeCellSolver.Game
{
    public enum MoveType
    {
        TableauToFoundation,
        TableauToReserve,
        TableauToTableau,
        ReserveToFoundation,
        ReserveToTableau,
    }

    public sealed class Move
    {
        private static readonly Move[] _possibleMoves = new Move[816];

        public MoveType Type { get; private set; }
        public int From { get; private set; }
        public int To { get; private set; }
        public int Size { get; private set; }

        internal Move(MoveType type, int from, int to, int size = 1)
        {
            Type = type;
            From = from;
            To = to;
            Size = size;
        }

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
                var seed = 0;
                for (var f = 0; f < 4; f++)
                {
                    var index = seed + (r * 4 + f);
                    _possibleMoves[index] = new Move(mt, r, f);
                }

                mt = MoveType.ReserveToTableau;
                seed = 16;
                for (var t = 0; t < 8; t++)
                {
                    var index = seed + (r * 8 + t);
                    _possibleMoves[index] = new Move(mt, r, t);
                }
            }

            for (var t = 0; t < 8; t++)
            {
                var mt = MoveType.TableauToFoundation;
                var seed = 48;
                for (var f = 0; f < 4; f++)
                {
                    var index = seed + (t * 4 + f);
                    _possibleMoves[index] = new Move(mt, t, f);
                }

                mt = MoveType.TableauToReserve;
                seed = 80;
                for (var r = 0; r < 4; r++)
                {
                    var index = seed + (t * 4 + r);
                    _possibleMoves[index] = new Move(mt, t, r);
                }

                mt = MoveType.TableauToTableau;
                seed = 112;
                for (var t2 = 0; t2 < 8; t2++)
                {
                    for (var moveSize = 1; moveSize < 12; moveSize++)
                    {
                        var index = seed + (t * 88 + t2 * 11) + moveSize - 1;
                        _possibleMoves[index] = new Move(mt, t, t2, moveSize);
                    }
                }
            }
        }

        public static Move Get(MoveType type, int from, int to, int size = 1)
        {
            Debug.Assert(
                (type == MoveType.ReserveToFoundation && (from >= 0 && from < 4 && to >= 0 && to < 4 && size == 1)) ||
                (type == MoveType.ReserveToTableau && (from >= 0 && from < 4 && to >= 0 && to < 8 && size == 1)) ||
                ((type == MoveType.TableauToFoundation || type == MoveType.TableauToReserve) && (from >= 0 && from < 8 && to >= 0 && to < 4 && size == 1)) ||
                (type == MoveType.TableauToTableau && (from != to && from >= 0 && from < 8 && to >= 0 && to < 8 && size >= 1 & size <= 11))
            );

            var index = -1;

            switch (type)
            {
                case MoveType.ReserveToFoundation:
                    index = from * 4 + to;
                    break;
                case MoveType.ReserveToTableau:
                    index = 16 + (from * 8 + to);
                    break;
                case MoveType.TableauToFoundation:
                    index = 48 + (from * 4 + to);
                    break;
                case MoveType.TableauToReserve:
                    index = 80 + (from * 4 + to);
                    break;
                case MoveType.TableauToTableau:
                    index = 112 + (from * 88 + to * 11) + size - 1;
                    break;
            }

            return _possibleMoves[index];
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