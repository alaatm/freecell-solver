using System;

namespace FreeCellSolver.Game.Helpers
{
    public static class Ranks
    {
        public static Rank[] Values { get; } = (Rank[])Enum.GetValues(typeof(Rank));
    }
}