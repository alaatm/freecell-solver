using System;

namespace FreeCellSolver.Game.Shared
{
    public static class Suits
    {
        public static Suit[] Values { get; } = (Suit[])Enum.GetValues(typeof(Suit));
    }
}