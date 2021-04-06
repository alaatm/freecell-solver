﻿using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game.Extensions;

namespace FreeCellSolver.Game
{
    public sealed class Board : IEquatable<Board>, IComparable<Board>
    {
        private int _hashcode;

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
            Tableaus = tableaus.CloneX(),
            Reserve = reserve.Clone(),
            Foundation = foundation.Clone(),
            MovesEstimated = 52 - (foundation[Suits.Clubs] + foundation[Suits.Diamonds] + foundation[Suits.Hearts] + foundation[Suits.Spades]),
        };

        public static Board FromDealNum(int dealNum) => BoardExtensions.FromDealNum(dealNum);

        [ThreadStatic] private static List<Move> _moves;

        public List<Move> GetValidMoves()
        {
            var tableaus = Tableaus;
            var reserve = Reserve;
            var foundation = Foundation;
            var lastMove = LastMove;

            (_moves ??= new List<Move>()).Clear();

            var freeCountPlusOne = reserve.FreeCount + 1;
            var emptyTableauCount = tableaus.EmptyCount();

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
                            _moves.Add(Move.Get(MoveType.ReserveToTableau, r, t));
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
                        var move = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                        if (!move.IsReverseOfTT(lastMove))
                        {
                            alreadyMovedToEmpty = true;
                            _moves.Add(move);
                        }
                    }
                    else if (!emptyTarget && maxMoveSize >= moveSize)
                    {
                        var move = Move.Get(MoveType.TableauToTableau, t1, t2, moveSize);
                        if (!move.IsReverseOfTT(lastMove))
                        {
                            _moves.Add(move);
                        }
                    }
                }
            }

            // 5. Tableau -> Reserve
            for (var t = 0; t < 8; t++)
            {
                if (AllowTableauToReserve(t) && tableaus[t].CanMove(reserve, out var r))
                {
                    _moves.Add(Move.Get(MoveType.TableauToReserve, t, r));
                }
            }

            return _moves;
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

            return copy;
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
                    // only in the unsorted portion of each tableau.
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

            _manualMoveCount = _manualMoveCount,
            AutoMoveCount = AutoMoveCount,
            MovesEstimated = MovesEstimated,
            LastMove = LastMove,
        };

        // Any change in this function must also be reflected in GetAutoMoves()
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
                    MovesEstimated--;
                    Tableaus[move.From].Move(Foundation);
                    break;
                case MoveType.TableauToReserve:
                    Tableaus[move.From].Move(Reserve, move.To);
                    break;
                case MoveType.TableauToTableau:
                    Tableaus[move.From].Move(Tableaus[move.To], move.Size);
                    Debug.Assert(move.Size <= ((Reserve.FreeCount + 1) << (Tableaus.EmptyCount() - (Tableaus[move.To].IsEmpty ? 1 : 0))));
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

        internal void RootAutoPlay()
        {
            Debug.Assert(Prev == null);

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
            // Block if last move is TtR unless the last move source is same as either the source
            // or dest of the proposed move.
            //
            //     r
            //     ↑
            // 00 01 02 03 04 05 06 07
            //
            // i.e. if last move t1-->r, then we block any TtT move where target or source is not 1.
            // so we only allow tx-->t1 or t1-->tx
            //
            // We do this because we want to stay with the same tableau discovering more promising positions.
            !(LastMove.Type == MoveType.TableauToReserve && LastMove.From != t1 && LastMove.From != t2);

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

            return _hashcode;
        }

        static readonly Random _rnd = new();
        static readonly int[] _reserveRand = new int[52];
        static readonly int[] _tableauUnsortedRand = new int[7];
        static readonly int[] _tableauSortedRand = new int[13];
        static readonly int[] _tableauTopRand = new int[52];

        static Board()
        {
            InitHashRand(52, _reserveRand);
            InitHashRand(7, _tableauUnsortedRand);
            InitHashRand(13, _tableauSortedRand);
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