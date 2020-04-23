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
        public static readonly Move[] _possibleMoves = new Move[780];

        public MoveType Type { get; }
        public int From { get; }
        public int To { get; }
        public int Size { get; }

        internal Move(MoveType type, int from, int to = 0, int size = 1)
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
                _possibleMoves[seed + r] = new Move(mt, r);

                mt = MoveType.ReserveToTableau;
                seed = 4;
                for (var t = 0; t < 8; t++)
                {
                    var index = seed + (r * 8) + t;
                    _possibleMoves[index] = new Move(mt, r, t);
                }
            }

            for (var t = 0; t < 8; t++)
            {
                var mt = MoveType.TableauToFoundation;
                var seed = 36;
                _possibleMoves[seed + t] = new Move(mt, t);

                mt = MoveType.TableauToReserve;
                seed = 44;
                for (var r = 0; r < 4; r++)
                {
                    var index = seed + (t * 4) + r;
                    _possibleMoves[index] = new Move(mt, t, r);
                }

                mt = MoveType.TableauToTableau;
                seed = 76;
                for (var t2 = 0; t2 < 8; t2++)
                {
                    for (var moveSize = 1; moveSize < 12; moveSize++)
                    {
                        var index = seed + (t * 88) + (t2 * 11) + moveSize - 1;
                        _possibleMoves[index] = new Move(mt, t, t2, moveSize);
                    }
                }
            }
        }

        public static Move Get(MoveType type, int from, int to = 0, int size = 1)
        {
            Debug.Assert(
                (type == MoveType.ReserveToFoundation && from >= 0 && from < 4 && to == 0 && size == 1) ||
                (type == MoveType.ReserveToTableau && from >= 0 && from < 4 && to >= 0 && to < 8 && size == 1) ||
                (type == MoveType.TableauToFoundation && from >= 0 && from < 8 && to == 0 && size == 1) ||
                (type == MoveType.TableauToReserve && from >= 0 && from < 8 && to >= 0 && to < 4 && size == 1) ||
                (type == MoveType.TableauToTableau && from != to && from >= 0 && from < 8 && to >= 0 && to < 8 && size >= 1 & size <= 11)
            );

            var index = type switch
            {
                MoveType.ReserveToFoundation => from,
                MoveType.ReserveToTableau => 4 + (from * 8) + to,
                MoveType.TableauToFoundation => 36 + from,
                MoveType.TableauToReserve => 44 + (from * 4) + to,
                MoveType.TableauToTableau => 76 + (from * 88) + (to * 11) + size - 1,
                _ => -1,
            };

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