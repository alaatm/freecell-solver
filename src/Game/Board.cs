using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game.Extensions;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Board : IEquatable<Board>
    {
        private int _hashcode = 0;

        public int ManualMoveCount { get; private set; }
        public int AutoMoveCount { get; private set; }
        public int MovesEstimated { get; private set; }
        public int MoveCount => ManualMoveCount + AutoMoveCount;

        public List<Move> AutoMoves { get; private set; }
        public Move LastMove { get; private set; }
        public Board Prev { get; private set; }

        public Reserve Reserve { get; }
        public Foundation Foundation { get; }
        public Tableaus Tableaus { get; }

        public bool IsSolved => MovesEstimated == 0;

        public int Cost { get; private set; }

        public Board(Tableaus tableaus) : this(new Reserve(), new Foundation(), tableaus) { }

        public Board(Reserve reserve, Foundation foundation, Tableaus tableaus)
        {
            Tableaus = tableaus.Clone();
            Reserve = reserve.Clone();
            Foundation = foundation.Clone();
            MovesEstimated = 52 - (foundation[Suits.Clubs] + foundation[Suits.Diamonds] + foundation[Suits.Hearts] + foundation[Suits.Spades]);
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
        }

        public static Board FromDealNum(int dealNum) => BoardExtensions.FromDealNum(dealNum);

        [ThreadStatic] static List<Move> _moves;

        public List<Move> GetValidMoves()
        {
            var tableaus = Tableaus;
            var reserve = Reserve;
            var foundation = Foundation;
            var lastMove = LastMove;

            (_moves ??= new List<Move>()).Clear();

            var freeCountPlusOne = reserve.FreeCount + 1;
            var emptyTableauCount = tableaus.EmptyTableauCount;

            // 1. Reserve -> Foundation
            for (var r = 0; r < 4; r++)
            {
                if (reserve.CanMove(r, foundation))
                {
                    _moves.Add(Move.Get(MoveType.ReserveToFoundation, r));
                    Debug.Assert(!foundation.CanAutoPlay(reserve[r]));
                }
            }

            // 2. Tableau -> Foundation
            for (var t = 0; t < 8; t++)
            {
                var tableau = tableaus[t];
                if (tableau.CanMove(foundation))
                {
                    _moves.Add(Move.Get(MoveType.TableauToFoundation, t));
                    Debug.Assert(!foundation.CanAutoPlay(tableau.Top));
                }
            }

            // 3. Reserve -> Tableau
            for (var r = 0; r < 4; r++)
            {
                if (reserve.GetValue(r) == Card.Nil)
                {
                    continue;
                }

                var alreadyMovedToEmpty = false;
                for (var t = 0; t < 8; t++)
                {
                    var tableau = tableaus[t];

                    if (reserve.CanMove(r, tableau))
                    {
                        var move = Move.Get(MoveType.ReserveToTableau, r, t);
                        var emptyTarget = tableau.IsEmpty;
                        // Skip move to empty if we've already made a similar
                        // move to another empty tableau or the move is a reverse of last move
                        // i.e. skip when move.IsReverseOf(lastMove) || (emptyTarget && alreadyMovedToEmpty) is true
                        if (!move.IsReverseOf(lastMove) && (!emptyTarget || !alreadyMovedToEmpty))
                        {
                            _moves.Add(move);
                            alreadyMovedToEmpty = emptyTarget || alreadyMovedToEmpty;
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

                    // Left shift Reserve.FreeCount+1 on number of non-target empty tableaus to get max move size
                    // i.e. 0 free, 2 empty tableaus that are not the target:
                    // maxMoveSize is 0+1 << 2 = 4
                    var maxMoveSize = freeCountPlusOne << (emptyTarget ? emptyTableauCount - 1 : emptyTableauCount);

                    if (emptyTarget)
                    {
                        // Skip move to empty when we've already made a similar
                        // move to another empty tableau.
                        if (!alreadyMovedToEmpty)
                        {
                            // For empty targets, add all possible moves from a tableau to an empty one except adding an entire sorted tableau
                            // i.e. maxMoveSize = 3 and given the following t1:
                            // JS TH 9S  (Size = 3, SortedSize = 3)
                            // Adds 2 moves:
                            // TH 9S -> t2
                            // 9S    -> t2
                            do
                            {
                                if (maxMoveSize >= moveSize && /* Skip move if moving entire column to empty one. */ tableauSize != moveSize)
                                {
                                    var move = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                                    if (!move.IsReverseOf(lastMove))
                                    {
                                        alreadyMovedToEmpty = true;
                                        _moves.Add(move);
                                    }
                                }
                            } while (--moveSize > 0);
                        }
                    }
                    else if (maxMoveSize >= moveSize)
                    {
                        var move = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                        if (!move.IsReverseOf(lastMove))
                        {
                            _moves.Add(move);
                        }
                    }
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
                        _moves.Add(move);
                    }
                }
            }

            return _moves;
        }

        public Board ExecuteMove(Move move)
        {
            var copy = ExecuteMoveNoAutoPlay(move);
            copy.AutoPlay();
            return copy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Board ExecuteMoveNoAutoPlay(Move move)
        {
            var copy = Clone();

            copy.ManualMoveCount++;
            copy.LastMove = move;
            copy.Prev = this;

            copy.ExecuteMoveCore(move);

            return copy;
        }

        internal void RootAutoPlay()
        {
            Debug.Assert(Prev == null);

            Prev = Clone();
            AutoPlay();
            if (AutoMoves == null)
            {
                Prev = null;
            }
        }

        private void AutoPlay()
        {
            Debug.Assert(Prev != null);

            var reserve = Reserve;
            var foundation = Foundation;
            var tableaus = Tableaus;

            bool found;

            do
            {
                found = false;

                // 1. Reserve -> Foundation
                for (var r = 0; r < 4; r++)
                {
                    var card = reserve[r];
                    if (card != Card.Null && foundation.CanAutoPlay(card))
                    {
                        var move = Move.Get(MoveType.ReserveToFoundation, r);

                        AutoMoves ??= new List<Move>(4);
                        AutoMoveCount++;
                        AutoMoves.Add(move);
                        ExecuteMoveCore(move);
                        found = true;
                    }
                }

                // 2. Tableau -> Foundation
                for (var t = 0; t < 8; t++)
                {
                    var card = tableaus[t].Top;
                    if (card != Card.Null && foundation.CanAutoPlay(card))
                    {
                        var move = Move.Get(MoveType.TableauToFoundation, t);

                        AutoMoves ??= new List<Move>(4);
                        AutoMoveCount++;
                        AutoMoves.Add(move);
                        ExecuteMoveCore(move);
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

        public int ComputeCost(bool factorPastMoves)
        {
            var foundation = Foundation;
            var tableaus = Tableaus;

            var colorDiff = Math.Abs(
                foundation[Suits.Clubs] + foundation[Suits.Spades] -
                foundation[Suits.Diamonds] - foundation[Suits.Hearts]);

            var unsortedSize = 0;
            for (var i = 0; i < 8; i++)
            {
                var t = tableaus[i];
                unsortedSize += t.Size - t.SortedSize;
            }

            var cost = MovesEstimated + unsortedSize + (4 - Reserve.FreeCount) + colorDiff;
            if (factorPastMoves)
            {
                cost += ManualMoveCount;
            }

            Cost = cost;
            return cost;
        }

        public Board Clone() => new Board(this);

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
            Debug.Assert(other is not null);

            if (!Reserve.Equals(other.Reserve))
            {
                return false;
            }

            if (!Foundation.Equals(other.Foundation))
            {
                return false;
            }

            var tableaus = Tableaus;
            var otherTableaus = other.Tableaus;

            for (var i = 0; i < 8; i++)
            {
                if (!tableaus[i].EqualsFast(otherTableaus[i]))
                {
                    return false;
                }
            }

            for (var i = 0; i < 8; i++)
            {
                if (!tableaus[i].EqualsSlow(otherTableaus[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) => obj is Board other && Equals(other);

        public override int GetHashCode()
        {
            // Board is immutable so its perfectly fine to cache the hashcode.
            if (_hashcode == 0)
            {
                var tableaus = Tableaus;
                var reserve = Reserve;

                var r0 = reserve.GetValue(0);
                var r1 = reserve.GetValue(1);
                var r2 = reserve.GetValue(2);
                var r3 = reserve.GetValue(3);
                if (r0 != Card.Nil) _hashcode += _reserveRand[r0];
                if (r1 != Card.Nil) _hashcode += _reserveRand[r1];
                if (r2 != Card.Nil) _hashcode += _reserveRand[r2];
                if (r3 != Card.Nil) _hashcode += _reserveRand[r3];

                for (var i = 0; i < 8; i++)
                {
                    var t = tableaus[i];
                    var size = t.Size;
                    var sortedSize = t.SortedSize;
                    var unsortedSize = size - sortedSize;

                    _hashcode += _tableauUnsortedRand[unsortedSize] << i;
                    _hashcode += _tableauSortedRand[sortedSize] << i;
                    if (size > 0)
                    {
                        _hashcode += _tableauTopRand[t.Top.RawValue] << i;
                    }
                }
            }

            return _hashcode;
        }

        static readonly Random _rnd = new Random();
        static readonly int[] _reserveRand = new int[52];
        static readonly int[] _tableauUnsortedRand = new int[8];
        static readonly int[] _tableauSortedRand = new int[14];
        static readonly int[] _tableauTopRand = new int[52];

        static Board()
        {
            InitHashRand(52, _reserveRand);
            InitHashRand(8, _tableauUnsortedRand);
            InitHashRand(14, _tableauSortedRand);
            InitHashRand(52, _tableauTopRand);
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