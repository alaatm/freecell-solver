using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver
{
    public class Reserve
    {
        public readonly List<Card> State = new List<Card>
        {
            null,
            null,
            null,
            null,
        };

        public IEnumerable<(int index, Card card)> Occupied
        {
            get
            {
                for (var i = 0; i < State.Count; i++)
                {
                    if (State[i] != null)
                    {
                        yield return (i, State[i]);
                    }
                }
            }
        }

        public int FreeCount => State.Count(c => c == null);

        internal Reserve(Reserve reserve) => State = reserve.State.ToList();

        public Reserve() { }

        public (bool canInsert, int index) CanInsert(Card card)
        {
            Debug.Assert(!State.Contains(card));
            return FreeCount > 0
                ? (true, State.IndexOf(null))
                : (false, -1);
        }

        public bool CanInsert(int index, Card card)
        {
            Debug.Assert(!State.Contains(card));
            Debug.Assert(index >= 0 && index < 4);
            return State[index] == null;
        }

        public bool CanMove(Card card, Tableau tableau)
        {
            Debug.Assert(State.Contains(card));

            if (tableau.IsEmpty)
            {
                return true;
            }

            return card.IsBelow(tableau.Top);
        }

        public bool CanMove(Card card, Foundation foundation)
        {
            Debug.Assert(State.Contains(card));
            return foundation.CanPush(card);
        }

        public void Insert(int index, Card card)
        {
            Debug.Assert(CanInsert(index, card));
            State[index] = card;
        }

        public void Remove(Card card)
        {
            Debug.Assert(State.Contains(card));

            var index = State.IndexOf(card);
            State[index] = null;
        }

        public void Move(Card card, Tableau tableau)
        {
            Debug.Assert(CanMove(card, tableau));
            Remove(card);
            tableau.Push(card);
        }

        public void Move(Card card, Foundation foundation)
        {
            Debug.Assert(CanMove(card, foundation));
            Remove(card);
            foundation.Push(card);
        }
    }
}