using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FreeCellSolver.Buffers
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct Arr04
    {
        internal byte _b0;
        internal byte _b1;
        internal byte _b2;
        internal byte _b3;

        public byte this[int index]
        {
            get => Unsafe.Add(ref _b0, index);
            set => Unsafe.Add(ref _b0, index) = value;
        }

        public bool Equals(Arr04 other) => Unsafe.As<byte, int>(ref _b0) == Unsafe.As<byte, int>(ref other._b0);

        public override bool Equals(object obj) => obj is Arr04 arr && Equals(arr);

        public static bool operator ==(Arr04 left, Arr04 right) => left.Equals(right);

        public static bool operator !=(Arr04 left, Arr04 right) => !left.Equals(right);

        public override int GetHashCode() => HashCode.Combine(_b0, _b1, _b2, _b3);
    }

    internal static class Arr4Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this Arr04 arr, byte value)
        {
            if (value == arr[0]) { return 0; }
            if (value == arr[1]) { return 1; }
            if (value == arr[2]) { return 2; }
            if (value == arr[3]) { return 3; }
            return -1;
        }

        public static byte[] AsArray(this Arr04 arr) => new[] { arr._b0, arr._b1, arr._b2, arr._b3 };
    }
}
