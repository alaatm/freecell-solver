using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public class Board : IEquatable<Board>
    {
        // Used only for post moves asserts
        private IEnumerable<Card> _allCards => Foundation.AllCards().Concat(Reserve.AllCards()).Concat(Tableaus.AllCards());

        private int _emptyTableauCount;
        private int _maxAllowedMoveSize => Reserve.FreeCount + _emptyTableauCount + 1;

        public int MovesSinceFoundation { get; private set; }
        public List<Move> Moves { get; private set; } = new List<Move>();
        public Reserve Reserve { get; private set; }
        public Foundation Foundation { get; private set; }
        public Tableaus Tableaus { get; private set; }
        public bool IsSolved => Foundation.IsComplete;

        public int LastMoveRating { get; private set; }

        private Board() { }

        public Board(Tableaus tableaus) : this(tableaus, null, null) { }

        public Board(Tableaus tableaus, Reserve reserve, Foundation foundation) : this(tableaus, reserve, foundation, tableaus.EmptyTableauCount) { }

        private Board(Tableaus tableaus, Reserve reserve, Foundation foundation, int emptyTableauCount)
        {
            Tableaus = tableaus.Clone();
            Reserve = reserve?.Clone() ?? new Reserve();
            Foundation = foundation?.Clone() ?? new Foundation();
            _emptyTableauCount = emptyTableauCount;
        }

        public (List<Move> moves, bool foundationFound) GetValidMoves()
        {
            var lastMove = Moves.Count > 0 ? Moves[Moves.Count - 1] : null;

            var moves = new List<Move>();
            var foundationFound = false;

            // 1. Reserve -> Foundation
            for (var r = 0; r < 4; r++)
            {
                if (Reserve.CanMove(r, Foundation, out var f))
                {
                    moves.Add(Move.Get(MoveType.ReserveToFoundation, r, f));
                    foundationFound = true;
                }
            }

            // 2. Tableau -> Foundation
            for (var t = 0; t < 8; t++)
            {
                if (Tableaus[t].CanMove(Foundation, out var f))
                {
                    moves.Add(Move.Get(MoveType.TableauToFoundation, t, f));
                    foundationFound = true;
                }
            }

            // 3. Reserve -> Tableau
            for (var r = 0; r < 4; r++)
            {
                if (Reserve[r] == null)
                {
                    continue;
                }

                var alreadyMovedToEmpty = false;
                for (var t = 0; t < 8; t++)
                {
                    var tableau = Tableaus[t];
                    var emptyTarget = tableau.IsEmpty;

                    if (Reserve.CanMove(r, tableau))
                    {
                        var move = Move.Get(MoveType.ReserveToTableau, r, t);
                        if (!move.IsReverseOf(lastMove))
                        {
                            if (emptyTarget && alreadyMovedToEmpty)
                            {
                                // Skip move to empty when we've already made a similar
                                // move to another empty tableau
                            }
                            else
                            {
                                moves.Add(move);
                                alreadyMovedToEmpty = emptyTarget ? true : alreadyMovedToEmpty;
                            }
                        }
                    }
                }
            }

            // 4. Tableau -> Tableau
            for (var t1 = 0; t1 < 8; t1++)
            {
                var tableau = Tableaus[t1];
                if (tableau.IsEmpty)
                {
                    continue;
                }

                var alreadyMovedToEmpty = false;
                for (var t2 = 0; t2 < 8; t2++)
                {
                    if (t1 == t2)
                    {
                        continue;
                    }

                    var targetTableau = Tableaus[t2];
                    var emptyTarget = targetTableau.IsEmpty;
                    var moveSize = tableau.CountMovable(targetTableau);
                    var maxAllowedMoveSize = _maxAllowedMoveSize - (emptyTarget ? 1 : 0);
                    var canMove = true;
                    /* No need to get target top when moveSize is 1 since we won't be calling IsBelow() */
                    var targetTop = moveSize > 1 ? targetTableau.Top : null;

                    while (moveSize > 0)
                    {
                        // Do not move an entire column to an empty one
                        var uselessMove = tableau.Size == moveSize && emptyTarget;

                        if (canMove && maxAllowedMoveSize >= moveSize && !uselessMove)
                        {
                            var move = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                            if (!move.IsReverseOf(lastMove))
                            {
                                if (emptyTarget && alreadyMovedToEmpty)
                                {
                                    // Skip move to empty when we've already made a similar
                                    // move to another empty tableau
                                }
                                else
                                {
                                    moves.Add(move);
                                    alreadyMovedToEmpty = emptyTarget ? true : alreadyMovedToEmpty;
                                }
                            }
                        }

                        if (--moveSize > 0)
                        {
                            canMove = emptyTarget || tableau[moveSize - 1].IsBelow(targetTop);
                        }
                    }
                }
            }

            // 5. Tableau -> Reserve
            for (var t = 0; t < 8; t++)
            {
                if (Tableaus[t].CanMove(Reserve, out var r))
                {
                    var move = Move.Get(MoveType.TableauToReserve, t, r);
                    if (!move.IsReverseOf(lastMove))
                    {
                        moves.Add(move);
                    }
                }
            }

            return (moves, foundationFound);
        }

        public void ExecuteMove(Move move, bool rate = false)
        {
            Moves.Add(move);

            if (rate)
            {
                RateMove(move);
            }

            switch (move.Type)
            {
                case MoveType.TableauToFoundation:
                    MovesSinceFoundation = 0;
                    var t = Tableaus[move.From];
                    t.Move(Foundation);
                    _emptyTableauCount += t.IsEmpty ? 1 : 0;
                    break;
                case MoveType.TableauToReserve:
                    MovesSinceFoundation++;
                    t = Tableaus[move.From];
                    t.Move(Reserve, move.To);
                    _emptyTableauCount += t.IsEmpty ? 1 : 0;
                    break;
                case MoveType.TableauToTableau:
                    MovesSinceFoundation++;
                    var t1 = Tableaus[move.From];
                    var t2 = Tableaus[move.To];
                    t1.Move(t2, move.Size);
                    _emptyTableauCount += t1.IsEmpty ? 1 : 0;
                    _emptyTableauCount -= t2.Size == move.Size ? 1 : 0;

                    Debug.Assert(move.Size <= _maxAllowedMoveSize - (Tableaus[move.To].IsEmpty ? 1 : 0));
                    break;
                case MoveType.ReserveToFoundation:
                    MovesSinceFoundation = 0;
                    Reserve.Move(move.From, Foundation);
                    break;
                case MoveType.ReserveToTableau:
                    MovesSinceFoundation++;
                    t = Tableaus[move.To];
                    Reserve.Move(move.From, t);
                    _emptyTableauCount -= t.Size == 1 ? 1 : 0;
                    break;
            }

            // Assert count and uniqueness
            Debug.Assert(new HashSet<Card>(_allCards).Count == 52);
            Debug.Assert(_emptyTableauCount == Tableaus.EmptyTableauCount);
            Debug.Assert(_emptyTableauCount >= 0 && _emptyTableauCount <= 8);
        }

        public void UndoLastMove()
        {
            if (Moves.Count == 0)
            {
                return;
            }

            var lastMove = Moves[Moves.Count - 1];

            Moves.RemoveAt(Moves.Count - 1);

            switch (lastMove.Type)
            {
                case MoveType.TableauToFoundation:
                    Foundation.Undo(lastMove, this);
                    _emptyTableauCount -= Tableaus[lastMove.From].Size == 1 ? 1 : 0;
                    break;
                case MoveType.TableauToReserve:
                    Reserve.Undo(lastMove, this);
                    _emptyTableauCount -= Tableaus[lastMove.From].Size == 1 ? 1 : 0;
                    break;
                case MoveType.TableauToTableau:
                    Tableaus[lastMove.To].Undo(lastMove, this);
                    _emptyTableauCount -= Tableaus[lastMove.From].Size == lastMove.Size ? 1 : 0;
                    _emptyTableauCount += Tableaus[lastMove.To].IsEmpty ? 1 : 0;
                    break;
                case MoveType.ReserveToFoundation:
                    Foundation.Undo(lastMove, this);
                    break;
                case MoveType.ReserveToTableau:
                    Tableaus[lastMove.To].Undo(lastMove, this);
                    _emptyTableauCount += Tableaus[lastMove.To].IsEmpty ? 1 : 0;
                    break;
            }

            // Assert count and uniqueness
            Debug.Assert(new HashSet<Card>(_allCards).Count == 52);
            Debug.Assert(_emptyTableauCount == Tableaus.EmptyTableauCount);
            Debug.Assert(_emptyTableauCount >= 0 && _emptyTableauCount <= 8);
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
                if (Tableaus.CanReceive(cardToBeTop, move.From))
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
        }

        public Board Clone()
        {
            var board = new Board(Tableaus, Reserve, Foundation);
            board._emptyTableauCount = _emptyTableauCount;
            board.MovesSinceFoundation = MovesSinceFoundation;
            board.Moves = Moves.ToList();
            return board;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Foundation.ToString());
            sb.AppendLine();
            sb.AppendLine(Reserve.ToString());
            sb.AppendLine();
            sb.Append(Tableaus.ToString());

            return sb.ToString();
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
        // 2 boards are equal when their reserve, foundation and cascade piles are the same
        // regardless of how that state was reached (i.e. regardless of the set of executed moves)
        public bool Equals([AllowNull] Board other) => other == null
            ? false
            : Reserve == other.Reserve
                && Foundation == other.Foundation
                && Tableaus == other.Tableaus;

        public override bool Equals(object obj) => obj is Board board && Equals(board);

        public override int GetHashCode() => HashCode.Combine(Reserve, Foundation, Tableaus);

        public static bool operator ==(Board a, Board b) => Equals(a, b);

        public static bool operator !=(Board a, Board b) => !(a == b);
        #endregion
    }
}