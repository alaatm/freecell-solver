using System;

namespace FreeCellSolver.Game.Shared
{
    public static class Suits
    {
        public static Suit[] Values() => (Suit[])Enum.GetValues(typeof(Suit));
    }
}