using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public class Board : IEquatable<Board>
    {
        private Deal _originalDeal;

        public List<Move> Moves { get; private set; } = new List<Move>();
        public Reserve Reserve { get; private set; } = new Reserve();
        public Foundation Foundation { get; private set; } = new Foundation();
        public Deal Deal { get; private set; }
        public bool IsSolved => Foundation.IsComplete;

        public int LastMoveRating { get; private set; }

        public Board(Deal deal)
        {
            _originalDeal = deal.Clone();
            Deal = deal;
        }

        public (List<string> moves, bool foundationFound) GetValidMoves(bool haltWhenFoundationFound)
        {
            var moves = new List<string>();
            var reserveToFoundationFound = false;
            var tableauToFoundationFound = false;

            // 1. Reserve -> Foundation
            for (var i = 0; i < 4; i++)
            {
                var card = Reserve[i];
                if (card != null)
                {
                    if (Foundation.CanPush(card))
                    {
                        moves.Add($"{"abcd"[i]}h");
                        reserveToFoundationFound = true;
                    }
                }
            }

            // 2. Tableau -> Foundation
            for (var i = 0; i < Deal.Tableaus.Count; i++)
            {
                var tableau = Deal.Tableaus[i];
                if (tableau.IsEmpty)
                {
                    continue;
                }

                if (Foundation.CanPush(tableau.Top))
                {
                    moves.Add($"{i}h");
                    tableauToFoundationFound = true;
                }
            }

            // 3. Reserve -> Tableau
            for (var i = 0; i < 4 && (!haltWhenFoundationFound || (haltWhenFoundationFound && !reserveToFoundationFound)); i++)
            {
                var card = Reserve[i];
                if (card != null)
                {
                    for (var t = 0; t < Deal.Tableaus.Count; t++)
                    {
                        var tableau = Deal.Tableaus[t];
                        if (Reserve.CanMove(card, tableau))
                        {
                            moves.Add($"{"abcd"[i]}{t}");
                        }
                    }
                }
            }

            // 4. Tableau -> Tableau
            for (var i = 0; i < Deal.Tableaus.Count && (!haltWhenFoundationFound || (haltWhenFoundationFound && !tableauToFoundationFound)); i++)
            {
                var tableau = Deal.Tableaus[i];
                if (tableau.IsEmpty)
                {
                    continue;
                }

                for (var t = 0; t < Deal.Tableaus.Count; t++)
                {
                    var targetTableau = Deal.Tableaus[t];
                    if (targetTableau.IsEmpty || tableau.Top.IsBelow(targetTableau.Top))
                    {
                        moves.Add($"{i}{t}");
                    }
                }
            }

            // 5. Tableau -> Reserve
            for (var i = 0; i < Deal.Tableaus.Count && (!haltWhenFoundationFound || (haltWhenFoundationFound && !tableauToFoundationFound)); i++)
            {
                var tableau = Deal.Tableaus[i];
                if (tableau.IsEmpty)
                {
                    continue;
                }

                var (canInsert, index) = Reserve.CanInsert(tableau.Top);
                if (canInsert)
                {
                    moves.Add($"{i}{"abcd"[index]}");
                }
            }

            return (moves, reserveToFoundationFound || tableauToFoundationFound);
        }

        public bool ShouldMove(Move move)
        {
            // Do not move if this is an exact opposite of the previous move
            if (Moves.Count > 0 && Moves[Moves.Count - 1].IsReverseOf(move))
            {
                return false;
            }

            return true;
        }

        public bool Move(Move move, bool rate = false)
        {
            Moves.Add(move);

            if (rate)
            {
                if (!RateMove(move))
                {
                    return false;
                }
            }

            switch (move.Type)
            {
                case MoveType.TableauToFoundation:
                    Deal.Tableaus[move.From].Move(Foundation);
                    break;
                case MoveType.TableauToReserve:
                    Deal.Tableaus[move.From].Move(Reserve, move.To);
                    break;
                case MoveType.TableauToTableau:
                    Deal.Tableaus[move.From].Move(Deal.Tableaus[move.To], 1);
                    break;
                case MoveType.ReserveToFoundation:
                    var card = Reserve[move.From];
                    Reserve.Move(card, Foundation);
                    break;
                case MoveType.ReserveToTableau:
                    card = Reserve[move.From];
                    Reserve.Move(card, Deal.Tableaus[move.To]);
                    break;
            }

            Debug.Assert(
                Deal.Tableaus.Sum(t => t.Size)
                + Reserve.OccupiedCount
                + Foundation.CountPlaced == 52);

            return true;
        }

        public Board Clone()
        {
            var board = new Board(Deal.Clone());
            board._originalDeal = _originalDeal.Clone();
            board.Moves = Moves.ToList();
            board.Reserve = Reserve.Clone();
            board.Foundation = Foundation.Clone();
            return board;
        }

        public void PrintMoves(string path)
        {
            var replayBoard = new Board(_originalDeal);
            replayBoard.ToImage().Save(Path.Join(path, "0.jpg"));

            var i = 1;
            foreach (var move in Moves)
            {
                replayBoard.Move(move);
                replayBoard.ToImage().Save(Path.Join(path, $"{i++}.jpg"));
            }
        }

        private bool RateMove(Move move)
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
            if (move.Type == MoveType.TableauToFoundation || move.Type == MoveType.ReserveToFoundation)
            {
                LastMoveRating += RATING_FOUNDATION;
            }

            if (move.Type == MoveType.TableauToFoundation || move.Type == MoveType.TableauToReserve || move.Type == MoveType.TableauToTableau)
            {
                var sourceTableau = Deal.Tableaus[move.From];
                cardToBeMoved = sourceTableau.Top;

                // Reward emptying tableau slot
                if (sourceTableau.Size == 1)
                {
                    LastMoveRating += RATING_OPENTABLEAU;
                }

                // Reward unburing foundation targets
                for (var i = 1; i < sourceTableau.Size; i++)
                {
                    if (Foundation.CanPush(sourceTableau[i]))
                    {
                        LastMoveRating += Math.Max(1, RATING_FREEFOUNDATIONTARGET - ((i - 1) * 3));
                    }
                }

                // Reward a newly discovered tableau-to-tableau move
                var cardToBeTop = sourceTableau.Size > 1 ? sourceTableau[1] : null;
                if (cardToBeTop != null && Deal.Tableaus.Any(t => t.CanPush(cardToBeTop)))
                {
                    LastMoveRating += RATING_FREETABLEAUTARGET;
                }
            }

            // Reward opening reserve slot
            if (move.Type == MoveType.ReserveToFoundation || move.Type == MoveType.ReserveToTableau)
            {
                LastMoveRating += RATING_OPENRESERVE;
                cardToBeMoved = Reserve[move.From];
            }

            if (move.Type == MoveType.ReserveToTableau || move.Type == MoveType.TableauToTableau)
            {
                // Reward any move to tableau
                LastMoveRating += RATING_TABLEAU;
                var targetTableau = Deal.Tableaus[move.To];

                // Punish buring foundation target
                for (var i = 0; i < targetTableau.Size; i++)
                {
                    if (Foundation.CanPush(targetTableau[i]))
                    {
                        LastMoveRating += RATING_BURYFOUNDATIONTARGET * (targetTableau.Size - i); // make the penalty higher on bottom cards
                    }
                }

                if (targetTableau.IsEmpty)
                {
                    // Do not move the single card of a tableau to an empty one
                    if (move.Type == MoveType.TableauToTableau && Deal.Tableaus[move.From].Size == 1)
                    {
                        LastMoveRating = -RATING_FOUNDATION;
                        return false;
                    }

                    var followup = false;

                    // Reward a move to an empty tableau that can be followed by another move from reserve
                    for (var i = 0; i < 4; i++)
                    {
                        var card = Reserve[i];
                        if (card != null)
                        {
                            if (card.IsBelow(cardToBeMoved))
                            {
                                LastMoveRating += RATING_CLOSEDTABLEAUFOLLOWUP + (int)card.Rank;
                                followup = true;
                            }
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

            // Punish filling a reserve spot
            if (move.Type == MoveType.TableauToReserve)
            {
                LastMoveRating += RATING_RESERVE;
            }

            return true;
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