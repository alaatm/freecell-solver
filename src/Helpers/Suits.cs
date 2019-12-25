using System;

namespace FreeCellSolver
{
    public static class Suits
    {
        public static Suit[] Values { get; } = (Suit[])Enum.GetValues(typeof(Suit));
    }
}