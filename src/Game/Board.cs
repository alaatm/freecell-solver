using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game.Extensions;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game
{
    public sealed class Board : IEquatable<Board>, IComparable<Board>
    {
        private int _hashcode = 0;

        internal int _cost = 0;
        internal int _manualMoveCount = 0;

        public int AutoMoveCount { get; private set; }
        public int MovesEstimated { get; private set; }
        public int MoveCount => _manualMoveCount + AutoMoveCount;

        public List<Move> AutoMoves { get; private set; }
        public Move LastMove { get; private set; }
        public Board Prev { get; private set; }

        public Reserve Reserve { get; }
        public Foundation Foundation { get; }
        public Tableaus Tableaus { get; }

        public bool IsSolved => MovesEstimated == 0;

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

            _manualMoveCount = copy._manualMoveCount;
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
                if (tableaus[t].CanMove(foundation))
                {
                    _moves.Add(Move.Get(MoveType.TableauToFoundation, t));
                    Debug.Assert(!foundation.CanAutoPlay(tableaus[t].Top));
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
                    var emptyTarget = tableau.IsEmpty;

                    // Skip move to empty if we've already made a similar move to another empty tableau
                    if (alreadyMovedToEmpty && emptyTarget)
                    {
                        continue;
                    }

                    if (reserve.CanMove(r, tableau))
                    {
                        var move = Move.Get(MoveType.ReserveToTableau, r, t);
                        if (!move.IsReverseOf(lastMove))
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
                    var emptyTarget = targetTableau.IsEmpty;
                    int moveSize;

                    // Skip move to empty when we've already made a similar move to another empty tableau.
                    // -or- when no moves available
                    if ((emptyTarget && alreadyMovedToEmpty) || (moveSize = tableau.CountMovable(targetTableau)) == 0)
                    {
                        continue;
                    }

                    // Left shift Reserve.FreeCount+1 on number of non-target empty tableaus to get max move size
                    // i.e. 0 free, 2 empty tableaus that are not the target:
                    // maxMoveSize is 0+1 << 2 = 4
                    var maxMoveSize = freeCountPlusOne << (emptyTarget ? emptyTableauCount - 1 : emptyTableauCount);

                    // Skip move if moving entire column to empty one
                    if (emptyTarget && (moveSize = Math.Min(moveSize, maxMoveSize)) != tableauSize)
                    {
                        var move = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                        if (!move.IsReverseOf(lastMove))
                        {
                            alreadyMovedToEmpty = true;
                            _moves.Add(move);
                        }
                    }
                    else if (!emptyTarget && maxMoveSize >= moveSize)
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

            copy._manualMoveCount++;
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

        public void ComputeCost()
        {
            Debug.Assert(_cost == 0);

            var foundation = Foundation;
            var tableaus = Tableaus;

            var colorDiff = Math.Abs(
                foundation[Suits.Clubs] + foundation[Suits.Spades] -
                foundation[Suits.Diamonds] - foundation[Suits.Hearts]);

            var suitsFound = 0;
            var numBuried = 0;
            var totalUnsortedSize = 0;
            for (var i = 0; i < 8; i++)
            {
                var t = tableaus[i];
                var size = t.Size;
                var unsortedSize = size - t.SortedSize;
                totalUnsortedSize += unsortedSize;

                if (suitsFound < 4)
                {
                    // Count depth of buried cards next in line to go to foundation
                    // only in the unsorted portion of the stack.
                    for (var j = unsortedSize - 1; j >= 0; j--)
                    {
                        if (foundation.CanPush(t[j]))
                        {
                            numBuried += size - j - 1;
                            suitsFound++;
                        }
                    }
                }
            }

            _cost =
                (MovesEstimated * 2)        // Less cards at foundation is costly by a factor of 2
                + totalUnsortedSize         // Unsored tableaues are a disadvantage
                + (4 - Reserve.FreeCount)   // Fewer free cells is a disadvantage
                + colorDiff                 // Greater color variance at foundation is a disadvantage
                + numBuried;                // Deeply buried cards within the unsorted portion of the col that are next to be placed at foundation is a disadvantage
        }

        public Board Clone() => new(this);

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

        public int CompareTo(Board other)
            => ((_cost << 8) | _manualMoveCount) - ((other._cost << 8) | other._manualMoveCount);

        #region Equality overrides and overloads
        public bool Equals(Board other)
        {
            Debug.Assert(other is not null);

            if (!Foundation.Equals(other.Foundation))
            {
                return false;
            }

            if (!Reserve.Equals(other.Reserve))
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

        static readonly Random _rnd = new();
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