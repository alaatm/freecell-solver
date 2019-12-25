using System;

namespace FreeCellSolver
{
    public static class Ranks
    {
        public static Rank[] Values { get; } = (Rank[])Enum.GetValues(typeof(Rank));
    }
}