using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class Foundation
    {
        public readonly Dictionary<Suit, int> State = new Dictionary<Suit, int>()
        {
            { Suit.Hearts, -1 },
            { Suit.Clubs, -1 },
            { Suit.Diamonds, -1 },
            { Suit.Spades, -1 },
        };

        internal Foundation(Foundation foundation) => State = new Dictionary<Suit, int>(foundation.State);

        public Foundation() { }

        public bool CanPush(Card card)
            => State[card.Suit] == (int)card.Rank - 1;

        public void Push(Card card)
        {
            Debug.Assert(CanPush(card));
            State[card.Suit]++;
        }

        public int Size(Suit suit) => State[suit];
    }
}