using System;

namespace FreeCellSolver.Game.Shared
{
    public static class Ranks
    {
        private static readonly Rank[] _ranks = (Rank[])Enum.GetValues(typeof(Rank));

        // Warning! caller might mutate!
        public static Rank[] Values() => _ranks;
    }
}