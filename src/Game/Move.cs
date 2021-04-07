using System;
using System.Diagnostics;

namespace FreeCellSolver.Game
{
    public enum MoveType : byte
    {
        None,
        TableauToFoundation,
        TableauToReserve,
        TableauToTableau,
        ReserveToFoundation,
        ReserveToTableau,
    }

    public readonly struct Move
    {
        public MoveType Type { get; }
        public byte From { get; }
        public byte To { get; }
        public byte Size { get; }

        private Move(MoveType type, byte from, byte to, byte size)
        {
            Type = type;
            From = from;
            To = to;
            Size = size;
        }

        public static Move Get(MoveType type, int from, int to = 0, int size = 1)
        {
            Debug.Assert(
                (type == MoveType.ReserveToFoundation && from >= 0 && from < 4 && to == 0 && size == 1) ||
                (type == MoveType.ReserveToTableau && from >= 0 && from < 4 && to >= 0 && to < 8 && size == 1) ||
                (type == MoveType.TableauToFoundation && from >= 0 && from < 8 && to == 0 && size == 1) ||
                (type == MoveType.TableauToReserve && from >= 0 && from < 8 && to >= 0 && to < 4 && size == 1) ||
                (type == MoveType.TableauToTableau && from != to && from >= 0 && from < 8 && to >= 0 && to < 8 && size >= 1 && size <= 11)
            );

            return new Move(type, (byte)from, (byte)to, (byte)size);
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

        public override int GetHashCode() => HashCode.Combine(Type, From, To, Size);

        private bool Equals(Move other) => Type == other.Type && From == other.From && To == other.To && Size == other.Size;

        public override bool Equals(object obj) => obj is Move other && Equals(other);

        public static bool operator ==(Move left, Move right) => left.Equals(right);

        public static bool operator !=(Move left, Move right) => !left.Equals(right);
    }
}