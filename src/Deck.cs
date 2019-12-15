using System.Collections.Generic;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public static class Deck
    {
        private static List<Card> _deck;

        public static List<Card> Get()
        {
            if (_deck == null)
            {
                _deck = new List<Card>(52);
                var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };

                foreach (var suit in suits)
                {
                    for (var r = 0; r < 13; r++)
                    {
                        var card = new Card(suit, (Rank)r);
                        _deck.Add(card);
                    }
                }
            }

            return new List<Card>(_deck);
        }

        public static IEnumerable<Card> Random() => Get().Shuffle();
    }
}