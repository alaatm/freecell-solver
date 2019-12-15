using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class Foundation
    {
        private readonly Dictionary<Suit, int> _foundation = new Dictionary<Suit, int>()
        {
            { Suit.Hearts, -1 },
            { Suit.Clubs, -1 },
            { Suit.Diamonds, -1 },
            { Suit.Spades, -1 },
        };

        public Dictionary<Suit, int> State => new Dictionary<Suit, int>(_foundation);

        internal Foundation(Foundation foundation) => _foundation = new Dictionary<Suit, int>(foundation._foundation);

        public Foundation() { }

        public bool CanPush(Card card)
            => _foundation[card.Suit] == (int)card.Rank - 1;

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            _foundation[card.Suit]++;
        }

        public int Size(Suit suit) => _foundation[suit];
    }
}