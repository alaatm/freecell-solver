using System;

namespace FreeCellSolver
{
    public static class StringExtensions
    {
        public static bool IsReverseOf(this string val, string other)
            => val != null && val[0] == other[1] && val[1] == other[0];
    }
}