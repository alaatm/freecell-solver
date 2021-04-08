using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FreeCellSolver.Buffers
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct Arr18
    {
        internal byte _b00;
        internal byte _b01;
        internal byte _b02;
        internal byte _b03;
        internal byte _b04;
        internal byte _b05;
        internal byte _b06;
        internal byte _b07;
        internal byte _b08;
        internal byte _b09;
        internal byte _b10;
        internal byte _b11;
        internal byte _b12;
        internal byte _b13;
        internal byte _b14;
        internal byte _b15;
        internal byte _b16;
        internal byte _b17;

        public byte this[int index]
        {
            get => Unsafe.Add(ref _b00, index);
            set => Unsafe.Add(ref _b00, index) = value;
        }

        public bool Equals(Arr18 other) =>
            Unsafe.As<byte, long>(ref _b00) == Unsafe.As<byte, long>(ref other._b00) &&
            Unsafe.As<byte, long>(ref _b08) == Unsafe.As<byte, long>(ref other._b08) &&
            Unsafe.As<byte, short>(ref _b16) == Unsafe.As<byte, short>(ref other._b16);

        public override bool Equals(object obj) => obj is Arr18 arr && Equals(arr);

        public static bool operator ==(Arr18 left, Arr18 right) => left.Equals(right);

        public static bool operator !=(Arr18 left, Arr18 right) => !left.Equals(right);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(_b00); hash.Add(_b01); hash.Add(_b02); hash.Add(_b03);
            hash.Add(_b04); hash.Add(_b05); hash.Add(_b06); hash.Add(_b07);
            hash.Add(_b08); hash.Add(_b09); hash.Add(_b10); hash.Add(_b11);
            hash.Add(_b12); hash.Add(_b13); hash.Add(_b14); hash.Add(_b15);
            hash.Add(_b16); hash.Add(_b17);

            return hash.ToHashCode();
        }
    }

    internal static class Arr18Extensions
    {
        public static void CopyTo(this Arr18 source, ref Arr18 destination, int sourceIndex, int destinationIndex, int count)
            => Unsafe.CopyBlock(ref Unsafe.Add(ref destination._b00, destinationIndex), ref Unsafe.Add(ref source._b00, sourceIndex), (uint)count);

        public static bool SequenceEqual(this Arr18 left, Arr18 right, int startIndex, int count)
        {
            // For our use, count can never be more than 12, which is the max sorted size of a tableau
            Debug.Assert(count <= 12);
            Debug.Assert(startIndex + count <= 18);

            for (var i = startIndex; i < count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static byte[] AsArray(this Arr18 arr) => new[]
        {
            arr._b00, arr._b01, arr._b02, arr._b03,
            arr._b04, arr._b05, arr._b06, arr._b07,
            arr._b08, arr._b09, arr._b10, arr._b11,
            arr._b12, arr._b13, arr._b14, arr._b15,
            arr._b16, arr._b17,
        };
    }
}
