using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    public class Board : IEquatable<Board>
    {
        public int MovesSinceFoundation { get; private set; }
        public int MoveCount { get; private set; }
        public List<Move> AutoMoves { get; private set; }
        public Move LastMove { get; private set; }
        public Board Prev { get; private set; }

        public Reserve Reserve { get; private set; }
        public Foundation Foundation { get; private set; }
        public Tableaus Tableaus { get; private set; }
        public bool IsSolved => Foundation.IsComplete;

        public int LastMoveRating { get; private set; }

        public Board(Tableaus tableaus) : this(new Reserve(), new Foundation(), tableaus) { }

        public Board(Reserve reserve, Foundation foundation, Tableaus tableaus)
        {
            Tableaus = tableaus.Clone();
            Reserve = reserve.Clone();
            Foundation = foundation.Clone();
        }

        private Board(Board copy)
        {
            Tableaus = copy.Tableaus.Clone();
            Reserve = copy.Reserve.Clone();
            Foundation = copy.Foundation.Clone();

            MovesSinceFoundation = copy.MovesSinceFoundation;
            MoveCount = copy.MoveCount;
            LastMove = copy.LastMove;
            Prev = copy.Prev;
        }

        public static Board FromDealNum(int dealNum) => BoardExtensions.FromDealNum(dealNum);

        public static Board FromString(string deal) => BoardExtensions.FromString(deal);

        public List<Move> GetValidMoves(out bool foundationFound)
        {
            var tableaus = Tableaus;
            var reserve = Reserve;
            var foundation = Foundation;
            var lastMove = LastMove;

            var moves = new List<Move>();
            foundationFound = false;

            var freeCount = Reserve.FreeCount + 1;
            var emptyTableauCount = Tableaus.EmptyTableauCount;

            // 1. Reserve -> Foundation
            for (var r = 0; r < 4; r++)
            {
                if (reserve.CanMove(r, foundation, out var f))
                {
                    moves.Add(Move.Get(MoveType.ReserveToFoundation, r, f));
                    foundationFound = true;
                    Debug.Assert(!foundation.CanAutoPlay(reserve[r]));
                }
            }

            // 2. Tableau -> Foundation
            for (var t = 0; t < 8; t++)
            {
                var tableau = tableaus[t];
                if (tableau.CanMove(foundation, out var f))
                {
                    moves.Add(Move.Get(MoveType.TableauToFoundation, t, f));
                    foundationFound = true;
                    Debug.Assert(!foundation.CanAutoPlay(tableau[0]));
                }
            }

            // 3. Reserve -> Tableau
            for (var r = 0; r < 4; r++)
            {
                if (reserve[r] == null)
                {
                    continue;
                }

                var alreadyMovedToEmpty = false;
                for (var t = 0; t < 8; t++)
                {
                    var tableau = tableaus[t];
                    var emptyTarget = tableau.IsEmpty;

                    if (reserve.CanMove(r, tableau))
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
                var tableau = tableaus[t1];
                var tableauSize = tableau.Size;

                if (tableauSize == 0)
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

                    var targetTableau = tableaus[t2];
                    var emptyTarget = targetTableau.IsEmpty;
                    var moveSize = tableau.CountMovable(targetTableau);
                    // Left shift freeCount+1 on number of non-target empty tableaus to get max move size
                    // i.e. 0 free, 2 empty tableaus that are not the target:
                    // maxAllowedMoveSize is 0+1 << 2 = 4
                    var maxAllowedMoveSize = freeCount << (emptyTarget ? emptyTableauCount - 1 : emptyTableauCount);
                    var canMove = true;
                    /* No need to get target top when moveSize is 1 since we won't be calling IsBelow() */
                    var targetTop = moveSize > 1 ? targetTableau.Top : null;

                    var didMove = false;

                    while (moveSize > 0)
                    {
                        // Do not move an entire column to an empty one
                        var uselessMove = tableauSize == moveSize && emptyTarget;

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
                                    didMove = true;
                                }
                            }
                        }

                        if (--moveSize > 0)
                        {
                            canMove = emptyTarget || tableau[moveSize - 1].IsBelow(targetTop);
                        }
                    }

                    alreadyMovedToEmpty = emptyTarget && didMove ? true : alreadyMovedToEmpty;
                }
            }

            // 5. Tableau -> Reserve
            for (var t = 0; t < 8; t++)
            {
                if (tableaus[t].CanMove(reserve, out var r))
                {
                    var move = Move.Get(MoveType.TableauToReserve, t, r);
                    if (!move.IsReverseOf(lastMove))
                    {
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        public void ExecuteMove(Move move, Board prev, bool autoPlay = true /* This flag is just to make testing easier. Should always be true*/)
        {
            MoveCount++;
            LastMove = move;
            Prev = prev;

            ExecuteMoveCore(move);
            if (autoPlay)
            {
                AutoPlay();
            }
        }

        public void AutoPlay()
        {
            var reserve = Reserve;
            var foundation = Foundation;
            var tableaus = Tableaus;

            var found = false;

            do
            {
                found = false;

                // 1. Reserve -> Foundation
                for (var r = 0; r < 4; r++)
                {
                    var card = reserve[r];
                    if (card != null && foundation.CanAutoPlay(card))
                    {
                        var move = Move.Get(MoveType.ReserveToFoundation, r, (int)card.Suit);
                        if (AutoMoves == null) AutoMoves = new List<Move>(10);
                        AutoMoves.Add(move);
                        ExecuteMoveCore(move);
                        LastMoveRating += 25;
                        found = true;
                    }
                }

                // 2. Tableau -> Foundation
                for (var t = 0; t < 8; t++)
                {
                    var card = tableaus[t].Top;
                    if (card != null && foundation.CanAutoPlay(card))
                    {
                        var move = Move.Get(MoveType.TableauToFoundation, t, (int)card.Suit);
                        if (AutoMoves == null) AutoMoves = new List<Move>(10);
                        AutoMoves.Add(move);
                        ExecuteMoveCore(move);
                        LastMoveRating += 25;
                        found = true;
                    }
                }
            } while (found);
        }

        private void ExecuteMoveCore(Move move)
        {
            switch (move.Type)
            {
                case MoveType.TableauToFoundation:
                    MovesSinceFoundation = 0;
                    Tableaus[move.From].Move(Foundation);
                    break;
                case MoveType.TableauToReserve:
                    MovesSinceFoundation++;
                    Tableaus[move.From].Move(Reserve, move.To);
                    break;
                case MoveType.TableauToTableau:
                    MovesSinceFoundation++;
                    Tableaus[move.From].Move(Tableaus[move.To], move.Size);
                    Debug.Assert(move.Size <= ((Reserve.FreeCount + 1) << (Tableaus.EmptyTableauCount - (Tableaus[move.To].IsEmpty ? 1 : 0))));
                    break;
                case MoveType.ReserveToFoundation:
                    MovesSinceFoundation = 0;
                    Reserve.Move(move.From, Foundation);
                    break;
                case MoveType.ReserveToTableau:
                    MovesSinceFoundation++;
                    Reserve.Move(move.From, Tableaus[move.To]);
                    break;
            }

            // Assert count and uniqueness
            Debug.Assert(AllCards.Count() == 52 && new HashSet<Card>(AllCards).Count == 52);
        }

        public void Undo()
        {
            var lastMove = LastMove;

            if (lastMove == null)
            {
                return;
            }

            var prev = Prev;

            MoveCount--;
            Prev = prev.Prev;
            LastMove = prev.LastMove;

            switch (lastMove.Type)
            {
                case MoveType.TableauToFoundation:
                    Foundation.Undo(lastMove, this);
                    break;
                case MoveType.TableauToReserve:
                    Reserve.Undo(lastMove, this);
                    break;
                case MoveType.TableauToTableau:
                    Tableaus[lastMove.To].Undo(lastMove, this);
                    break;
                case MoveType.ReserveToFoundation:
                    Foundation.Undo(lastMove, this);
                    break;
                case MoveType.ReserveToTableau:
                    Tableaus[lastMove.To].Undo(lastMove, this);
                    break;
            }

            // Assert count and uniqueness
            Debug.Assert(AllCards.Count() == 52 && new HashSet<Card>(AllCards).Count == 52);
        }

        public void RateMove(Move move)
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

            var tableaus = Tableaus;
            var reserve = Reserve;
            var foundation = Foundation;

            // Reward move to foundation
            if (move.Type == MoveType.TableauToFoundation || move.Type == MoveType.ReserveToFoundation)
            {
                LastMoveRating += RATING_FOUNDATION;
            }

            if (move.Type == MoveType.TableauToFoundation || move.Type == MoveType.TableauToReserve || move.Type == MoveType.TableauToTableau)
            {
                var sourceTableau = tableaus[move.From];
                var sourceTableauSize = sourceTableau.Size;
                cardToBeMoved = sourceTableau.Top;

                // Reward emptying tableau slot
                if (sourceTableauSize == move.Size)
                {
                    LastMoveRating += RATING_OPENTABLEAU;
                }

                // Reward unburing foundation targets
                for (var i = move.Size; i < sourceTableauSize; i++)
                {
                    if (foundation.CanPush(sourceTableau[i]))
                    {
                        LastMoveRating += Math.Max(1, RATING_FREEFOUNDATIONTARGET - ((i - move.Size) * 3));
                    }
                }

                // Reward a newly discovered tableau-to-tableau move
                var cardToBeTop = sourceTableauSize > move.Size ? sourceTableau[move.Size] : null;
                if (tableaus.CanReceive(cardToBeTop, move.From))
                {
                    LastMoveRating += RATING_FREETABLEAUTARGET;
                }
            }

            // Reward opening reserve slot
            if (move.Type == MoveType.ReserveToFoundation || move.Type == MoveType.ReserveToTableau)
            {
                LastMoveRating += RATING_OPENRESERVE;
                cardToBeMoved = reserve[move.From];
            }

            if (move.Type == MoveType.ReserveToTableau || move.Type == MoveType.TableauToTableau)
            {
                // Reward any move to tableau
                LastMoveRating += RATING_TABLEAU + /* Reward more for moving sorted stacks */ move.Size - 1;
                var targetTableau = tableaus[move.To];
                var targetTableauSize = targetTableau.Size;

                // Punish buring foundation target, penalty is higher on bottom cards
                for (var i = 0; i < targetTableauSize; i++)
                {
                    if (foundation.CanPush(targetTableau[i]))
                    {
                        LastMoveRating += RATING_BURYFOUNDATIONTARGET * (targetTableauSize + move.Size - i - 1);
                    }
                }

                if (targetTableauSize == 0)
                {
                    var followup = false;

                    // Reward a move to an empty tableau that can be followed by another move from reserve
                    for (var i = 0; i < 4; i++)
                    {
                        var card = reserve[i];
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
                        var card = tableaus[i].Top;
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

        public Board Clone() => new Board(this);

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
            foreach (var move in GetMoves())
            {
                replayBoard.ExecuteMove(move, null);
                replayBoard.ToImage().Save(Path.Join(path, $"{i++}.jpg"));
            }
        }

        private IEnumerable<Move> GetMoves()
        {
            if (Prev == null)
            {
                yield break;
            }

            var prev = this;
            var moves = new Stack<Move>();

            do
            {
                moves.Push(prev.LastMove);
                prev = prev.Prev;
            } while (prev.Prev != null);

            foreach (var move in moves)
            {
                yield return move;
            }
        }


        #region Equality overrides and overloads
        public bool Equals(Board other)
        {
            if (other == null)
            {
                return false;
            }

            var tableaus = Tableaus;
            var reserve = Reserve;
            var foundation = Foundation;

            var otherTableaus = other.Tableaus;
            var otherReserve = other.Reserve;
            var otherFoundation = other.Foundation;

            for (var i = 0; i < 4; i++)
            {
                // TODO: This actually might report inequal where GetHashCode() return same value
                // when we have same cards at reserve but at different positions. Chance is very low
                // but consider making both methods return persistent values.
                //
                // i.e.
                // Reserve1 [ 4C, --, --, 5H ]
                // Reserve2 [ 4C, 5H, --, -- ]
                // and everything else is same on both boards.
                //
                // GetHashCode() for both boards --> returns same value
                // Equals()                      --> returns false
                //
                // For above case, we want to report equality.
                if (reserve[i]?.RawValue != otherReserve[i]?.RawValue)
                {
                    return false;
                }
                if (foundation[(Suit)i] != otherFoundation[(Suit)i])
                {
                    return false;
                }
            }

            for (var i = 0; i < 8; i++)
            {
                var t1 = tableaus[i];
                var size1 = t1.Size;
                var sortedSize1 = t1.SortedSize;
                var top1 = size1 > 0 ? t1[0] : null;

                var t2 = otherTableaus[i];
                var size2 = t2.Size;
                var sortedSize2 = t2.SortedSize;
                var top2 = size2 > 0 ? t2[0] : null;

                if (top1 != top2 || size1 != size2 || sortedSize1 != sortedSize2)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) => obj is Board board && Equals(board);

        public override int GetHashCode()
        {
            var tableaus = Tableaus;
            var reserve = Reserve;

            unchecked
            {
                var hash = 0;

                for (var i = 0; i < 4; i++)
                {
                    var r = reserve[i];
                    if (r != null)
                    {
                        hash += _reserveRand[r.RawValue];
                    }
                }

                for (var i = 0; i < 8; i++)
                {
                    var t = tableaus[i];
                    var size = t.Size;
                    var sortedSize = t.SortedSize;
                    var unsortedSize = size - sortedSize;

                    hash += _tableauUnsortedRand[i][unsortedSize];
                    hash += _tableauSortedRand[i][sortedSize];
                    if (size > 0)
                    {
                        hash += _tableauTopRand[i][t[0].RawValue];
                    }
                }

                return hash;
            }
        }

        static readonly Random _rnd = new Random();
        static readonly int[] _reserveRand = new int[52];
        static readonly int[][] _tableauUnsortedRand = new int[8][];
        static readonly int[][] _tableauSortedRand = new int[8][];
        static readonly int[][] _tableauTopRand = new int[8][];

        static Board()
        {
            InitHashRand(52, _reserveRand);

            for (var i = 0; i < 8; i++)
            {
                _tableauUnsortedRand[i] = new int[8];
                InitHashRand(8, _tableauUnsortedRand[i]);

                _tableauSortedRand[i] = new int[14];
                InitHashRand(14, _tableauSortedRand[i]);

                _tableauTopRand[i] = new int[52];
                InitHashRand(52, _tableauTopRand[i]);
            }
        }

        static void InitHashRand(int count, int[] rand)
        {
            for (var i = 0; i < count; i++)
            {
                rand[i] = _rnd.Next();
            }
        }

        public static bool operator ==(Board a, Board b) => Equals(a, b);

        public static bool operator !=(Board a, Board b) => !(a == b);
        #endregion

        // Used only for post moves asserts
        internal IEnumerable<Card> AllCards => Foundation.AllCards().Concat(Reserve.AllCards()).Concat(Tableaus.AllCards());
    }
}