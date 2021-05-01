using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game.Extensions;
using FreeCellSolver.Buffers;

namespace FreeCellSolver.Game
{
    public sealed class Board : IEquatable<Board>, IComparable<Board>
    {
        private int _hashcode;
        private ForbiddenMoves _forbiddenMoves;

        internal int _cost;
        internal int _manualMoveCount;

        public int AutoMoveCount { get; private set; }
        public int MovesEstimated { get; private set; }
        public int MoveCount => _manualMoveCount + AutoMoveCount;

        public Move LastMove { get; private set; }
        public Board Prev { get; private set; }

        public Reserve Reserve { get; private init; }
        public Foundation Foundation { get; private init; }
        public Tableau[] Tableaus { get; private init; }

        public bool IsSolved => MovesEstimated == 0;

        private Board() { }

        public static Board Create(Reserve reserve, Foundation foundation, params Tableau[] tableaus) => new()
        {
            Reserve = reserve.Clone(),
            Foundation = foundation.Clone(),
            Tableaus = tableaus.CloneX(),
            MovesEstimated = 52 - (foundation[Suits.Hearts] + foundation[Suits.Clubs] + foundation[Suits.Diamonds] + foundation[Suits.Spades]),
        };

        public static Board FromDealNum(int dealNum) => new()
        {
            Reserve = Reserve.Create(),
            Foundation = Foundation.Create(),
            Tableaus = BoardExtensions.FromDealNum(dealNum),
            MovesEstimated = 52,
        };

        [ThreadStatic] private static Move[] _moves;

        public ReadOnlySpan<Move> GetValidMoves()
        {
            var tableaus = Tableaus;
            var reserve = Reserve;
            var foundation = Foundation;

            var moveCount = 0;
            _moves ??= new Move[64];

            var freeCountPlusOne = reserve.FreeCount + 1;
            var emptyTableauCount = tableaus.EmptyCount();

            // 1. Reserve -> Foundation
            for (var r = 0; r < 4; r++)
            {
                if (reserve.CanMove(r, foundation))
                {
                    _moves[moveCount++] = Move.Get(MoveType.ReserveToFoundation, r);
                    Debug.Assert(!foundation.CanAutoPlay(reserve[r]));
                }
            }

            // 2. Tableau -> Foundation
            for (var t = 0; t < 8; t++)
            {
                if (tableaus[t].CanMove(foundation))
                {
                    _moves[moveCount++] = Move.Get(MoveType.TableauToFoundation, t);
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
                    if (AllowReserveToTableau(r, t))
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
                            _moves[moveCount++] = Move.Get(MoveType.ReserveToTableau, r, t);
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
                    if (t1 == t2 || !AllowTableauToTableau(t1, t2))
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
                        alreadyMovedToEmpty = true;
                        _moves[moveCount++] = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                    }
                    else if (!emptyTarget && maxMoveSize >= moveSize)
                    {
                        _moves[moveCount++] = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                    }
                }
            }

            // 5. Tableau -> Reserve
            for (var t = 0; t < 8; t++)
            {
                if (AllowTableauToReserve(t) && tableaus[t].CanMove(reserve, out var r))
                {
                    _moves[moveCount++] = Move.Get(MoveType.TableauToReserve, t, r);
                }
            }

            return _moves.AsSpan().Slice(0, moveCount);
        }

        public Board ExecuteMove(Move move) => ExecuteMove(move, true);

        public Board ExecuteMove(Move move, bool autoPlay)
        {
            var copy = Clone();

            copy._manualMoveCount++;
            copy.LastMove = move;
            copy.Prev = this;

            copy.ExecuteMoveCore(move);
            if (autoPlay)
            {
                copy.AutoPlay();
            }

            Debug.Assert(copy._hashcode == 0);
            Debug.Assert(copy._cost == 0);
            return copy;
        }

        public void ComputeCost()
        {
            Debug.Assert(_cost == 0);

            var foundation = Foundation;
            var tableaus = Tableaus;

            var hearts = foundation[Suits.Hearts];
            var clubs = foundation[Suits.Clubs];
            var diamonds = foundation[Suits.Diamonds];
            var spades = foundation[Suits.Spades];

            var colorDiff = Math.Abs(
                clubs + spades -
                diamonds - hearts);

            // Convert to card's rawValue and add 1 to the rank so we can
            // check if tableaus' cards can be inserted to foundation
            hearts = (byte)((hearts + 1) << 2);
            clubs = (byte)(((clubs + 1) << 2) | 1);
            diamonds = (byte)(((diamonds + 1) << 2) | 2);
            spades = (byte)(((spades + 1) << 2) | 3);

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
                    // only in the unsorted portion of each tableau.
                    for (var j = 0; j < unsortedSize; j++)
                    {
                        var card = t.GetValue(j);

                        if ((card == clubs) ||
                            (card == diamonds) ||
                            (card == hearts) ||
                            (card == spades))
                        {
                            numBuried += size - j - 1;
                            suitsFound++;
                        }
                    }
                }
            }

            _cost =
                (MovesEstimated * 2)       // Less cards at foundation is costly by a factor of 2
                + totalUnsortedSize        // Unsorted tableaus are a disadvantage
                + (4 - Reserve.FreeCount)  // Fewer free cells is a disadvantage
                + colorDiff                // Greater color variance at foundation is a disadvantage
                + numBuried;               // Deeply buried cards, which are next in line, within the unsorted portion of tableaus is a disadvantage
        }

        public Board Clone() => new()
        {
            Tableaus = Tableaus.CloneX(),
            Reserve = Reserve.Clone(),
            Foundation = Foundation.Clone(),

            _forbiddenMoves = _forbiddenMoves,
            _manualMoveCount = _manualMoveCount,
            AutoMoveCount = AutoMoveCount,
            MovesEstimated = MovesEstimated,
            LastMove = LastMove,
        };

        // Any change in this function must also be reflected in GetAutoMoves()
        private void AutoPlay()
        {
            Debug.Assert(Prev is not null);

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
                        AutoMoveCount++;
                        ExecuteMoveCore(Move.Get(MoveType.ReserveToFoundation, r));
                        found = true;
                    }
                }

                // 2. Tableau -> Foundation
                for (var t = 0; t < 8; t++)
                {
                    var card = tableaus[t].Top;
                    if (card != Card.Null && foundation.CanAutoPlay(card))
                    {
                        AutoMoveCount++;
                        ExecuteMoveCore(Move.Get(MoveType.TableauToFoundation, t));
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
                    LiftMoveRestrictions(move.From);
                    MovesEstimated--;
                    Tableaus[move.From].Move(Foundation);
                    break;
                case MoveType.TableauToReserve:
                    LiftMoveRestrictions(move.From);
                    Tableaus[move.From].Move(Reserve, move.To);
                    break;
                case MoveType.TableauToTableau:
                    LiftMoveRestrictions(move.From);
                    LiftMoveRestrictions(move.To);
                    if (Tableaus[move.From].Move(Tableaus[move.To], move.Size))
                    {
                        // Only add a move restriction when the source tableau is still sorted to prevent the same move in reverse in the future.
                        // This move restriction will be lifted if either the source or target tableaus receive any adjustments in the future.
                        AddMoveRestriction(move.To, move.From);
                    }
                    Debug.Assert(move.Size <= ((Reserve.FreeCount + 1) << (Tableaus.EmptyCount() - (Tableaus[move.To].IsEmpty ? 1 : 0))));
                    break;
                case MoveType.ReserveToFoundation:
                    MovesEstimated--;
                    Reserve.Move(move.From, Foundation);
                    break;
                case MoveType.ReserveToTableau:
                    LiftMoveRestrictions(move.To);
                    Reserve.Move(move.From, Tableaus[move.To]);
                    break;
            }

            // Assert count and uniqueness
            Debug.Assert(AllCards.Count() == 52 && new HashSet<Card>(AllCards).Count == 52);
        }

        internal void RootAutoPlay()
        {
            Debug.Assert(Prev is null);

            Prev = Clone();
            AutoPlay();
            if (AutoMoveCount == 0)
            {
                Prev = null;
            }
        }

        internal IEnumerable<Move> GetMoves()
        {
            var moves = new Stack<Move>();
            this.Traverse(b =>
            {
                foreach (var autoMove in GetAutoMoves(b).Reverse())
                {
                    moves.Push(autoMove);
                }
                if (b.LastMove.Type != MoveType.None)
                {
                    moves.Push(b.LastMove);
                }
            });

            return moves;

            static IEnumerable<Move> GetAutoMoves(Board board)
            {
                if (board.Prev is null)
                {
                    return Enumerable.Empty<Move>();
                }

                var clone = board.Prev.ExecuteMove(board.LastMove, false);
                var reserve = clone.Reserve;
                var foundation = clone.Foundation;
                var tableaus = clone.Tableaus;

                var autoMoves = new List<Move>();

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
                            autoMoves.Add(move);
                            clone.ExecuteMoveCore(move);
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
                            autoMoves.Add(move);
                            clone.ExecuteMoveCore(move);
                            found = true;
                        }
                    }
                } while (found);

                return autoMoves;
            }
        }

        private bool AllowReserveToTableau(int r, int t) =>
            // Block if last move is TtT unless the proposed move target is same as either source
            // or dest of last move.
            //
            // 00 01 02 03 04 05 06 07
            //     ↓        ↑
            //     →→→→→→→→→→
            //
            // i.e. if last move was t1-->t4, then we block any RtT move where target tableau is not 1 or 4.
            // so we only allow r-->t1 or r-->t4
            //
            // We do this because we want to stay with the same tableaus that originated the move to discover
            // more promising positions.
            !(LastMove.Type == MoveType.TableauToTableau && t != LastMove.From && t != LastMove.To) &&
            // Block reverse move
            !(LastMove.Type == MoveType.TableauToReserve && r == LastMove.To && t == LastMove.From);

        private bool AllowTableauToTableau(int t1, int t2) =>
            // Block reverse move
            !_forbiddenMoves.Contains(t1, t2);

        private bool AllowTableauToReserve(int t) =>
            // Block if last move is also TtR unless the last move source is greater or equal to
            // the proposed move source.
            //
            //              r
            //              ↑
            // 00 01 02 03 04 05 06 07
            //
            // i.e. if last move t4-->r, then we block any TtR move where source is less than 4.
            // so we only allow t4-->r, t5-->r, t6-->r and t7-->r
            //
            // We do this because blocked moves are guaranteed to be generated from other board states.
            !(LastMove.Type == MoveType.TableauToReserve && LastMove.From > t) &&
            // Block reverse move
            !(LastMove.Type == MoveType.ReserveToTableau && LastMove.To == t);

        private void AddMoveRestriction(int from, int to)
            => _forbiddenMoves.Add(from, to);

        private void LiftMoveRestrictions(int t)
            => _forbiddenMoves.Remove(t);

        struct ForbiddenMoves
        {
            private Arr04 _items;
            private byte _size;

            public void Add(int from, int to)
                => _items[_size++] = (byte)((from << 4) | to);

            public void Remove(int t)
                => _items.Remove((byte)t, ref _size);

            public bool Contains(int t1, int t2)
                => _items.IndexOf((byte)((t1 << 4) | t2), _size) != -1;
        }

        #region overrides and interface impl functions
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Foundation.ToString());
            sb.AppendLine();
            sb.AppendLine(Reserve.ToString());
            sb.AppendLine();
            sb.Append(Tableaus.Dump());

            return sb.ToString();
        }

        public int CompareTo(Board other)
            => ((_cost << 8) | _manualMoveCount) - ((other._cost << 8) | other._manualMoveCount);

        public bool Equals(Board other)
        {
            Debug.Assert(other is not null);
            //Debug.Assert(_hashcode != 0 && other._hashcode != 0); // Should always true when running, but not when running tests

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
                if (!tableaus[i].Equals(otherTableaus[i]))
                {
                    return false;
                }
            }

            Debug.Assert(_hashcode == other._hashcode);
            return true;
        }

        public override bool Equals(object obj) => obj is Board other && Equals(other);

        public override int GetHashCode()
        {
            // Board is immutable so its perfectly fine to cache the hashcode.
            if (_hashcode != 0)
            {
                return _hashcode;
            }

            var tableaus = Tableaus;
            var reserve = Reserve;

            var r0 = reserve.GetValue(0);
            var r1 = reserve.GetValue(1);
            var r2 = reserve.GetValue(2);
            var r3 = reserve.GetValue(3);
            if (r0 != Card.Nil) { _hashcode += _reserveRand[r0]; }
            if (r1 != Card.Nil) { _hashcode += _reserveRand[r1]; }
            if (r2 != Card.Nil) { _hashcode += _reserveRand[r2]; }
            if (r3 != Card.Nil) { _hashcode += _reserveRand[r3]; }

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

            return _hashcode;
        }

        static readonly Random _rnd = new();
        static readonly int[] _reserveRand = new int[56];
        static readonly int[] _tableauUnsortedRand = new int[7];
        static readonly int[] _tableauSortedRand = new int[13];
        static readonly int[] _tableauTopRand = new int[56];

        static Board()
        {
            InitHashRand(56, _reserveRand);
            InitHashRand(7, _tableauUnsortedRand);
            InitHashRand(13, _tableauSortedRand);
            InitHashRand(56, _tableauTopRand);
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