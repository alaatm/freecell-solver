using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public class Board : IEquatable<Board>
    {
        private string _lastMoveString;
        private Deal _originalDeal;

        public List<Move> Moves { get; private set; } = new List<Move>();
        public Reserve Reserve { get; private set; } = new Reserve();
        public Foundation Foundation { get; private set; } = new Foundation();
        public Deal Deal { get; private set; }
        public bool IsSolved => Foundation.State.Values.Where(v => v != -1).Sum() + 4 == 52;

        public int LastMoveRating { get; private set; }

        public Board(Deal deal)
        {
            _originalDeal = deal.Clone();
            Deal = deal;
        }

        public bool ShouldMove(string moveString)
        {
            // Do not move if this is an exact opposite of the previous move
            if (_lastMoveString.IsReverseOf(moveString))
            {
                return false;
            }

            return true;
        }

        /// 01234567 : Tableau
        /// abcd     : Freecell/Reserve
        /// h        : Home/Foundation
        /// 
        /// 72       : Tableau 7 to Tableau 2
        /// 7c       : Tableau 7 to Reserve 3
        /// 3h       : Tableau 3 to Foundation
        /// b0       : Reserve 1 to Tableau 0
        /// ah       : Reserve 0 to Foundation
        public void Move(string moveString, bool rate = false)
        {
            _lastMoveString = moveString;
            Card card;
            var move = FreeCellSolver.Move.Parse(moveString);
            Moves.Add(move);

            if (rate)
            {
                RateMove(move);
            }

            switch (move.Source)
            {
                case Location.Tableau:
                    var sourceTableau = Deal.Tableaus[move.SourceIndex.Value];

                    switch (move.Target)
                    {
                        case Location.Tableau:
                            var targetTableau = Deal.Tableaus[move.TargetIndex.Value];
                            sourceTableau.Move(targetTableau, 1);
                            break;
                        case Location.Reserve:
                            sourceTableau.Move(Reserve, move.TargetIndex.Value);
                            break;
                        case Location.Foundation:
                            sourceTableau.Move(Foundation);
                            break;
                    }
                    break;
                case Location.Reserve:
                    card = Reserve.State[move.SourceIndex.Value];

                    switch (move.Target)
                    {
                        case Location.Tableau:
                            var targetTableau = Deal.Tableaus[move.TargetIndex.Value];
                            Reserve.Move(card, targetTableau);
                            break;
                        case Location.Foundation:
                            Reserve.Move(card, Foundation);
                            break;
                    }
                    break;
            }

            Debug.Assert(
                Deal.Tableaus.Sum(t => t.Stack.Count)
                + Reserve.Occupied.Count()
                + Foundation.State.Values.Where(v => v != -1).Select(n => n + 1).Sum() == 52);
        }

        public Board Clone()
        {
            var board = new Board(Deal.Clone());
            // board._lastMoveString = new string(_lastMoveString);
            board._originalDeal = _originalDeal.Clone();
            board.Moves = Moves.ToList();
            board.Reserve = new Reserve(Reserve);
            board.Foundation = new Foundation(Foundation);
            return board;
        }

        public void PrintMoves(string path)
        {
            var replayBoard = new Board(_originalDeal);
            replayBoard.ToImage().Save(Path.Join(path, "0.jpg"));

            var i = 1;
            foreach (var move in Moves)
            {
                replayBoard.Move(move.ToString());
                replayBoard.ToImage().Save(Path.Join(path, $"{i++}.jpg"));
            }
        }

        private void RateMove(Move move)
        {
            const int RATING_FOUNDATION = 1000;
            const int RATING_CLOSEDTABLEAUFOLLOWUP = 20;
            const int RATING_FREEFOUNDATIONTARGET = 15;
            const int RATING_OPENTABLEAU = 15;
            const int RATING_FREETABLEAUTARGET = 10;
            const int RATING_OPENRESERVE = 10;
            const int RATING_TABLEAU = 2;
            const int RATING_RESERVE = -1;
            const int RATING_BURYFOUNDATIONTARGET = -5;
            const int RATING_CLOSEDTABLEAU = -10;

            LastMoveRating = 0;
            Card cardToBeMoved = null;

            // Reward move to foundation
            if (move.Target == Location.Foundation)
            {
                LastMoveRating += RATING_FOUNDATION;
            }

            if (move.Source == Location.Tableau)
            {
                var sourceTableau = Deal.Tableaus[move.SourceIndex.Value];
                var stack = sourceTableau.Stack;
                cardToBeMoved = sourceTableau.Top;

                // Reward emptying tableau slot
                if (stack.Count == 1)
                {
                    LastMoveRating += RATING_OPENTABLEAU;
                }

                // Reward unburing foundation targets
                for (var i = 1; i < stack.Count; i++)
                {
                    if (Foundation.CanPush(stack.ElementAt(i)))
                    {
                        LastMoveRating += RATING_FREEFOUNDATIONTARGET - (i - 1);
                    }
                }

                // Reward a newly discovered tableau-to-tableau move
                var cardToBeTop = stack.Count > 1 ? stack.ElementAt(1) : null;
                if (cardToBeTop != null && Deal.Tableaus.Any(t => t.CanPush(cardToBeTop)))
                {
                    LastMoveRating += RATING_FREETABLEAUTARGET;
                }
            }

            // Reward opening reserve slot
            if (move.Source == Location.Reserve)
            {
                LastMoveRating += RATING_OPENRESERVE;
                cardToBeMoved = Reserve.State[move.SourceIndex.Value];
            }

            // Reward any move to tableau
            if (move.Target == Location.Tableau)
            {
                LastMoveRating += RATING_TABLEAU;
                var targetTableau = Deal.Tableaus[move.TargetIndex.Value];
                var stack = targetTableau.Stack.Reverse();

                // Punish buring foundation target
                for (var i = 0; i < stack.Count(); i++)
                {
                    if (Foundation.CanPush(stack.ElementAt(i)))
                    {
                        LastMoveRating += RATING_BURYFOUNDATIONTARGET * (i + 1);
                    }
                }

                if (targetTableau.IsEmpty)
                {
                    // Avoid moving the single card of a tableau to an empty one
                    if (move.Source == Location.Tableau && Deal.Tableaus[move.SourceIndex.Value].Stack.Count == 1)
                    {
                        LastMoveRating = -RATING_FOUNDATION;
                        return;
                    }

                    var followup = false;

                    // Reward a move to an empty tableau that can be followed by another move from reserve
                    foreach (var (_, card) in Reserve.Occupied)
                    {
                        if (card.IsBelow(cardToBeMoved))
                        {
                            LastMoveRating += RATING_CLOSEDTABLEAUFOLLOWUP + (int)card.Rank;
                            followup = true;
                        }
                    }

                    // Reward a move to an empty tableau that can be followed by another move from tableaus
                    foreach (var tableauTop in Deal.Tableaus.Select(t => t.Top))
                    {
                        if (tableauTop?.IsBelow(cardToBeMoved) ?? false)
                        {
                            LastMoveRating += RATING_CLOSEDTABLEAUFOLLOWUP + (int)tableauTop.Rank;
                            followup = true;
                        }
                    }

                    // punish filling a tableau slot with no immediate followup
                    if (!followup)
                    {
                        LastMoveRating += RATING_CLOSEDTABLEAU;
                    }
                }
            }

            if (move.Target == Location.Reserve)
            {
                LastMoveRating += RATING_RESERVE;
            }
        }

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Board other) => other == null
            ? false
            : Moves.SequenceEqual(other.Moves) && Deal == other.Deal;

        public override bool Equals(object obj) => obj is Board board && Equals(board);

        public override int GetHashCode()
        {
            var hc = Deal.GetHashCode();
            foreach (var move in Moves)
            {
                hc = HashCode.Combine(hc, move.GetHashCode());
            }
            return hc;
        }

        public static bool operator ==(Board a, Board b) => Equals(a, b);

        public static bool operator !=(Board a, Board b) => !(a == b);
        #endregion
    }
}