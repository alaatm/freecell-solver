using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game.Extensions;

namespace FreeCellSolver.Game
{
    public sealed class Board : IEquatable<Board>
    {
        public int ManualMoveCount { get; private set; }
        public int AutoMoveCount { get; private set; }
        public int MovesEstimated { get; private set; }
        public int MinMovesToGoal => ManualMoveCount + MovesEstimated;
        public int MoveCount => ManualMoveCount + AutoMoveCount;

        public List<Move> AutoMoves { get; private set; }
        public Move LastMove { get; private set; }
        public Board Prev { get; private set; }

        public Reserve Reserve { get; private set; }
        public Foundation Foundation { get; private set; }
        public Tableaus Tableaus { get; private set; }
        public bool IsSolved => MovesEstimated == 0;

        public int Cost { get; private set; }

        public Board(Tableaus tableaus) : this(new Reserve(), new Foundation(), tableaus) { }

        public Board(Reserve reserve, Foundation foundation, Tableaus tableaus)
        {
            Tableaus = tableaus.Clone();
            Reserve = reserve.Clone();
            Foundation = foundation.Clone();
            MovesEstimated = 52 - (foundation[Suits.CLUBS] + foundation[Suits.DIAMONDS] + foundation[Suits.HEARTS] + foundation[Suits.SPADES] + 4);
        }

        private Board(Board copy)
        {
            Tableaus = copy.Tableaus.Clone();
            Reserve = copy.Reserve.Clone();
            Foundation = copy.Foundation.Clone();

            ManualMoveCount = copy.ManualMoveCount;
            AutoMoveCount = copy.AutoMoveCount;
            MovesEstimated = copy.MovesEstimated;
            LastMove = copy.LastMove;
            Prev = copy.Prev;
        }

        public static Board FromDealNum(int dealNum) => BoardExtensions.FromDealNum(dealNum);

        public static Board FromString(string deal) => BoardExtensions.FromString(deal);

        public List<Move> GetValidMoves()
        {
            var tableaus = Tableaus;
            var reserve = Reserve;
            var foundation = Foundation;
            var lastMove = LastMove;

            var moves = new List<Move>();

            var freeCount = Reserve.FreeCount + 1;
            var emptyTableauCount = Tableaus.EmptyTableauCount;

            // 1. Reserve -> Foundation
            for (var r = 0; r < 4; r++)
            {
                if (reserve.CanMove(r, foundation, out var f))
                {
                    moves.Add(Move.Get(MoveType.ReserveToFoundation, r, f));
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
                    Debug.Assert(!foundation.CanAutoPlay(tableau.Top));
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
                    var moveSize = tableau.CountMovable(targetTableau);
                    if (moveSize == 0)
                    {
                        continue;
                    }

                    var emptyTarget = targetTableau.IsEmpty;

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

        public void ExecuteMove(Move move, Board prev, bool autoPlay = true /* This flag is used for printing of moves as we don't want to execute auto plays otherwise should always be true*/)
        {
            ManualMoveCount++;
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

            Board original = null;
            if (Prev == null)
            {
                // Record original so that we can set it to Prev if this is root
                // before we execute a move below
                original = Clone();
            }

            var autoFound = false;
            bool found;

            do
            {
                found = false;

                // 1. Reserve -> Foundation
                for (var r = 0; r < 4; r++)
                {
                    var card = reserve[r];
                    if (card != null && foundation.CanAutoPlay(card))
                    {
                        var move = Move.Get(MoveType.ReserveToFoundation, r, card.Suit);

                        if (AutoMoves == null)
                        {
                            AutoMoves = new List<Move>(4);
                        }

                        AutoMoveCount++;
                        AutoMoves.Add(move);
                        ExecuteMoveCore(move);
                        found = autoFound = true;
                    }
                }

                // 2. Tableau -> Foundation
                for (var t = 0; t < 8; t++)
                {
                    var card = tableaus[t].Top;
                    if (card != null && foundation.CanAutoPlay(card))
                    {
                        var move = Move.Get(MoveType.TableauToFoundation, t, card.Suit);

                        if (AutoMoves == null)
                        {
                            AutoMoves = new List<Move>(4);
                        }

                        AutoMoveCount++;
                        AutoMoves.Add(move);
                        ExecuteMoveCore(move);
                        found = autoFound = true;
                    }
                }
            } while (found);

            if (Prev == null && autoFound)
            {
                // This is the root, we need to insert a copy just before this one
                // with the original state
                Prev = original;
            }
        }

        private void ExecuteMoveCore(Move move)
        {
            switch (move.Type)
            {
                case MoveType.TableauToFoundation:
                    MovesEstimated--;
                    Tableaus[move.From].Move(Foundation);
                    break;
                case MoveType.TableauToReserve:
                    Tableaus[move.From].Move(Reserve, move.To);
                    break;
                case MoveType.TableauToTableau:
                    Tableaus[move.From].Move(Tableaus[move.To], move.Size);
                    Debug.Assert(move.Size <= ((Reserve.FreeCount + 1) << (Tableaus.EmptyTableauCount - (Tableaus[move.To].IsEmpty ? 1 : 0))));
                    break;
                case MoveType.ReserveToFoundation:
                    MovesEstimated--;
                    Reserve.Move(move.From, Foundation);
                    break;
                case MoveType.ReserveToTableau:
                    Reserve.Move(move.From, Tableaus[move.To]);
                    break;
            }

            // Assert count and uniqueness
            Debug.Assert(AllCards.Count() == 52 && new HashSet<Card>(AllCards).Count == 52);
        }

        public void ComputeCost()
        {
            var foundation = Foundation;
            var tableaus = Tableaus;

            var fClubs = foundation[Suits.CLUBS] + 1;
            var fDiamonds = foundation[Suits.DIAMONDS] + 1;
            var fHearts = foundation[Suits.HEARTS] + 1;
            var fSpades = foundation[Suits.SPADES] + 1;

            var colorDiff = Math.Abs(fClubs + fSpades - fDiamonds - fHearts);

            var unsortedSize = 0;
            for (var i = 0; i < 8; i++)
            {
                var t = tableaus[i];
                unsortedSize += t.Size - t.SortedSize;
            }

            Cost = MovesEstimated + unsortedSize + (4 - Reserve.FreeCount) + colorDiff;
        }

        public IEnumerable<Move> GetMoves()
        {
            var moves = new Stack<Move>();
            Traverse(b =>
            {
                foreach (var autoMove in b.AutoMoves?.AsEnumerable().Reverse() ?? Enumerable.Empty<Move>())
                {
                    moves.Push(autoMove);
                }
                if (b.LastMove != null)
                {
                    moves.Push(b.LastMove);
                }
            });

            return moves;
        }

        public void Traverse(Action<Board> visit)
        {
            var prev = this;

            while (prev != null)
            {
                visit(prev);
                prev = prev.Prev;
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

            var reserveSum = 0;
            var otherReserveSum = 0;
            for (var i = 0; i < 4; i++)
            {
                reserveSum += reserve[i]?.RawValue ?? 0;
                otherReserveSum += otherReserve[i]?.RawValue ?? 0;
                if (foundation[i] != otherFoundation[i])
                {
                    return false;
                }
            }

            // Note as long as we have same cards in reserve, regardless of order then we consider them to be equal
            if (reserveSum != otherReserveSum)
            {
                return false;
            }

            for (var i = 0; i < 8; i++)
            {
                var t1 = tableaus[i];
                var size1 = t1.Size;
                var sortedSize1 = t1.SortedSize;
                var top1 = size1 > 0 ? t1.Top : null;

                var t2 = otherTableaus[i];
                var size2 = t2.Size;
                var sortedSize2 = t2.SortedSize;
                var top2 = size2 > 0 ? t2.Top : null;

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
                        hash += _tableauTopRand[i][t.Top.RawValue];
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