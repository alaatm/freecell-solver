using System;

namespace FreeCellSolver.Game.Helpers
{
    public static class Suits
    {
        public static Suit[] Values { get; } = (Suit[])Enum.GetValues(typeof(Suit));
    }
}