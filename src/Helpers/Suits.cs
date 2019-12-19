using System.Collections.Generic;

namespace FreeCellSolver
{
    public static class Suits
    {
        public static IEnumerable<Suit> All()
        {
            yield return Suit.Hearts;
            yield return Suit.Clubs;
            yield return Suit.Diamonds;
            yield return Suit.Spades;
        }
    }
}