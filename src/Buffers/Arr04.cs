using System;
using System.Diagnostics;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(byte value)
        {
            if (value == _b0) { return 0; }
            if (value == _b1) { return 1; }
            if (value == _b2) { return 2; }
            if (value == _b3) { return 3; }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(byte value, int len)
        {
            Debug.Assert(len <= 4);
            if (len == 0) { return -1; }
            if (len > 0 && value == _b0) { return 0; }
            if (len > 1 && value == _b1) { return 1; }
            if (len > 2 && value == _b2) { return 2; }
            if (len > 3 && value == _b3) { return 3; }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfWithMask(byte value, int len)
        {
            Debug.Assert(len <= 4);
            if (len == 0) { return -1; }
            if (len > 0 && (value == _b0 >> 4 || value == (_b0 & 0xf))) { return 0; }
            if (len > 1 && (value == _b1 >> 4 || value == (_b1 & 0xf))) { return 1; }
            if (len > 2 && (value == _b2 >> 4 || value == (_b2 & 0xf))) { return 2; }
            if (len > 3 && (value == _b3 >> 4 || value == (_b3 & 0xf))) { return 3; }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(byte value, ref byte len)
        {
            Debug.Assert(len is >= 0 and <= 4);

            for (var i = IndexOfWithMask(value, len); i >= 0; i = IndexOfWithMask(value, len))
            {
                len--;
                for (var j = i; j < len; j++)
                {
                    this[j] = this[j + 1];
                }
            }
        }

        public bool Equals(Arr04 other) => Unsafe.As<byte, int>(ref _b0) == Unsafe.As<byte, int>(ref other._b0);

        public override bool Equals(object obj) => obj is Arr04 arr && Equals(arr);

        public static bool operator ==(Arr04 left, Arr04 right) => left.Equals(right);

        public static bool operator !=(Arr04 left, Arr04 right) => !left.Equals(right);

        public override int GetHashCode() => HashCode.Combine(_b0, _b1, _b2, _b3);
    }

    internal static class Arr4Extensions
    {
        public static byte[] AsArray(this Arr04 arr) => new[] { arr._b0, arr._b1, arr._b2, arr._b3 };
    }
}
