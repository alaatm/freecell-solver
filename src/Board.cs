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
        public List<Move> Moves { get; private set; } = new List<Move>();
        public Reserve Reserve { get; private set; }
        public Foundation Foundation { get; private set; }
        public Tableaus Tableaus { get; private set; }
        public bool IsSolved => Foundation.IsComplete;

        public int LastMoveRating { get; private set; }

        public int MaxAllowedMoveSize => Reserve.FreeCount + Tableaus.EmptyTableauCount + 1;

        private Board() { }

        public Board(Tableaus tableaus) : this(tableaus, null, null) { }

        public Board(Tableaus tableaus, Reserve reserve, Foundation foundation)
        {
            Tableaus = tableaus.Clone();
            Reserve = reserve?.Clone() ?? new Reserve();
            Foundation = foundation?.Clone() ?? new Foundation();
        }

        public (List<Move> moves, bool foundationFound) GetValidMoves(bool haltWhenFoundationFound)
        {
            var moves = new List<Move>();
            var reserveToFoundationFound = false;
            var tableauToFoundationFound = false;

            // 1. Reserve -> Foundation
            for (var r = 0; r < 4; r++)
            {
                var card = Reserve[r];
                if (card != null)
                {
                    if (Foundation.CanPush(card))
                    {
                        moves.Add(Move.Get(MoveType.ReserveToFoundation, r));
                        reserveToFoundationFound = true;
                    }
                }
            }

            // 2. Tableau -> Foundation
            for (var t = 0; t < 8; t++)
            {
                var tableau = Tableaus[t];
                if (tableau.IsEmpty)
                {
                    continue;
                }

                if (Foundation.CanPush(tableau.Top))
                {
                    moves.Add(Move.Get(MoveType.TableauToFoundation, t));
                    tableauToFoundationFound = true;
                }
            }

            // 3. Reserve -> Tableau
            for (var r = 0; r < 4 && (!haltWhenFoundationFound || (haltWhenFoundationFound && !reserveToFoundationFound)); r++)
            {
                var card = Reserve[r];
                if (card != null)
                {
                    for (var t = 0; t < 8; t++)
                    {
                        var tableau = Tableaus[t];
                        if (Reserve.CanMove(card, tableau))
                        {
                            moves.Add(Move.Get(MoveType.ReserveToTableau, r, t));
                        }
                    }
                }
            }

            // 4. Tableau -> Tableau
            for (var t1 = 0; t1 < 8 && (!haltWhenFoundationFound || (haltWhenFoundationFound && !tableauToFoundationFound)); t1++)
            {
                var tableau = Tableaus[t1];
                if (tableau.IsEmpty)
                {
                    continue;
                }

                for (var t2 = 0; t2 < 8; t2++)
                {
                    if (t1 != t2)
                    {
                        var targetTableau = Tableaus[t2];
                        var moveSize = tableau.CountMovable(targetTableau);
                        var maxAllowedMoves = moveSize == 1 ? 1 : MaxAllowedMoveSize - (targetTableau.IsEmpty ? 1 : 0);

                        if (moveSize > 0 && maxAllowedMoves >= moveSize)
                        {
                            moves.Add(Move.Get(MoveType.TableauToTableau, t1, t2, moveSize));
                        }
                    }
                }
            }

            // 5. Tableau -> Reserve
            for (var t = 0; t < 8 && (!haltWhenFoundationFound || (haltWhenFoundationFound && !tableauToFoundationFound)); t++)
            {
                var tableau = Tableaus[t];
                if (tableau.IsEmpty)
                {
                    continue;
                }

                var (canInsert, r) = Reserve.CanInsert(tableau.Top);
                if (canInsert)
                {
                    moves.Add(Move.Get(MoveType.TableauToReserve, t, r));
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

        public bool ExecuteMove(Move move, bool rate = false)
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
                    Tableaus[move.From].Move(Foundation);
                    break;
                case MoveType.TableauToReserve:
                    Tableaus[move.From].Move(Reserve, move.To);
                    break;
                case MoveType.TableauToTableau:
                    Tableaus[move.From].Move(Tableaus[move.To], move.Size);
                    break;
                case MoveType.ReserveToFoundation:
                    var card = Reserve[move.From];
                    Reserve.Move(card, Foundation);
                    break;
                case MoveType.ReserveToTableau:
                    card = Reserve[move.From];
                    Reserve.Move(card, Tableaus[move.To]);
                    break;
            }

            Debug.Assert(
                Tableaus.CardCount
                + Reserve.OccupiedCount
                + Foundation.CountPlaced == 52);

            return true;
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
                var sourceTableau = Tableaus[move.From];
                cardToBeMoved = sourceTableau.Top;

                // Reward emptying tableau slot
                if (sourceTableau.Size == move.Size)
                {
                    LastMoveRating += RATING_OPENTABLEAU;
                }

                // Reward unburing foundation targets
                for (var i = move.Size; i < sourceTableau.Size; i++)
                {
                    if (Foundation.CanPush(sourceTableau[i]))
                    {
                        LastMoveRating += Math.Max(1, RATING_FREEFOUNDATIONTARGET - ((i - move.Size) * 3));
                    }
                }

                // Reward a newly discovered tableau-to-tableau move
                var cardToBeTop = sourceTableau.Size > move.Size ? sourceTableau[move.Size] : null;
                if (Tableaus.CanReceive(cardToBeTop, sourceTableau))
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
                LastMoveRating += RATING_TABLEAU + /* Reward more for moving sorted stacks */ move.Size - 1;
                var targetTableau = Tableaus[move.To];

                // Punish buring foundation target, penalty is higher on bottom cards
                for (var i = 0; i < targetTableau.Size; i++)
                {
                    if (Foundation.CanPush(targetTableau[i]))
                    {
                        LastMoveRating += RATING_BURYFOUNDATIONTARGET * (targetTableau.Size + move.Size - i - 1);
                    }
                }

                if (targetTableau.IsEmpty)
                {
                    // Do not move the single card/sorted stack of a tableau to an empty one
                    if (move.Type == MoveType.TableauToTableau && Tableaus[move.From].Size == move.Size)
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
                    for (var i = 0; i < 8; i++)
                    {
                        var card = Tableaus[i].Top;
                        if (card?.IsBelow(cardToBeMoved) ?? false)
                        {
                            LastMoveRating += RATING_CLOSEDTABLEAUFOLLOWUP + (int)card.Rank;
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

        public Board Clone()
        {
            var board = new Board(Tableaus, Reserve, Foundation);
            board.Moves = Moves.ToList();
            return board;
        }

        public void PrintMoves(string path, Tableaus originalDeal)
        {
            var replayBoard = new Board(originalDeal);
            replayBoard.ToImage().Save(Path.Join(path, "0.jpg"));

            var i = 1;
            foreach (var move in Moves)
            {
                replayBoard.ExecuteMove(move);
                replayBoard.ToImage().Save(Path.Join(path, $"{i++}.jpg"));
            }
        }

        #region Equality overrides and overloads
        public bool Equals([AllowNull] Board other) => other == null
            ? false
            : Moves.SequenceEqual(other.Moves) && Tableaus == other.Tableaus;

        public override bool Equals(object obj) => obj is Board board && Equals(board);

        public override int GetHashCode()
        {
            var hc = Tableaus.GetHashCode();
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