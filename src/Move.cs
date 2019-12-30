using System.Diagnostics;

namespace FreeCellSolver
{
    public enum MoveType
    {
        TableauToFoundation,
        TableauToReserve,
        TableauToTableau,
        ReserveToFoundation,
        ReserveToTableau,
    }

    public class Move
    {
        public MoveType Type { get; private set; }
        public int From { get; private set; }
        public int To { get; private set; }
        public int Size { get; private set; }

        internal Move(MoveType type, int from, int to, int size = 1)
        {
            Debug.Assert(
                (type == MoveType.ReserveToFoundation && (from >= 0 && from < 4 && to >= 0 && to < 4 && size == 1)) ||
                (type == MoveType.ReserveToTableau && (from >= 0 && from < 4 && to >= 0 && to < 8 && size == 1)) ||
                ((type == MoveType.TableauToFoundation || type == MoveType.TableauToReserve) && (from >= 0 && from < 8 && to >= 0 && to < 4 && size == 1)) ||
                (type == MoveType.TableauToTableau && (from != to && from >= 0 && from < 8 && to >= 0 && to < 8 && size >= 1 & size < 11))
            );

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