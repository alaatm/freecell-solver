using System;

namespace FreeCellSolver.Game.Shared
{
    public static class Suits
    {
        private static readonly Suit[] _suits = (Suit[])Enum.GetValues(typeof(Suit));

        // Warning! caller might mutate!
        public static Suit[] Values() => _suits;
    }
}