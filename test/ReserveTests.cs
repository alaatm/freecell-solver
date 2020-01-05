using System;
using FreeCellSolver.Extensions;
using Xunit;

namespace FreeCellSolver.Test
{
    public class ReserveTests
    {
        [Fact]
        public void State_is_initialized_to_empty_reserve()
        {
            var r = new Reserve();

            for (var i = 0; i < 4; i++)
            {
                Assert.Null(r[i]);
            }
        }

        [Fact]
        public void Indexer_returns_value_of_specified_slot()
        {
            var r = new Reserve(1, -1, 2, -1);
            Assert.Equal(2, r[2].RawValue);
        }

        [Fact]
        public void FreeCount_is_properly_initialized()
        {
            // empty reserve
            Assert.Equal(4, new Reserve().FreeCount);
            // 2 spots occupied, 2 free
            Assert.Equal(2, new Reserve(1, -1, 2, -1).FreeCount);
        }

        [Fact]
        public void CanInsert_returns_whether_card_can_be_inserted_along_with_target_index()
        {
            // Index 2 free
            var r = new Reserve(0, 1, -1, 2);
            Assert.True(r.CanInsert(out var index));
            Assert.Equal(2, index);

            // Full
            r = new Reserve(0, 1, 5, 2);
            Assert.False(r.CanInsert(out index));
            Assert.Equal(-1, index);
        }

        [Fact]
        public void CanMove_returns_whether_card_can_be_moved_to_tableau()
        {
            // Move 5H to empty tableau
            var r = new Reserve(Card.Get("5H").RawValue, -1, -1, -1);
            var t = new Tableau("");
            Assert.True(r.CanMove(0, t));

            // Move 5H below 6S
            t = new Tableau("6C");
            Assert.True(r.CanMove(0, t));

            // Can't move 5H below 9C
            t = new Tableau("9C");
            Assert.False(r.CanMove(0, t));

            // Can't move empty slot
            r = new Reserve(-1, -1, -1, -1);
            Assert.False(r.CanMove(0, t));
        }

        [Fact]
        public void CanMove_returns_whether_card_can_be_moved_to_foundation_or_not()
        {
            // Move AC to empty foundation
            var r = new Reserve(Card.Get("AC").RawValue, -1, -1, -1);
            var f = new Foundation();
            Assert.True(r.CanMove(0, f, out var idx));
            Assert.Equal(Suit.Clubs, (Suit)idx);

            // Can't move 2C to empty foundation
            r = new Reserve(Card.Get("2C").RawValue, -1, -1, -1);
            Assert.False(r.CanMove(0, f, out idx));
            Assert.Equal(-1, idx);

            // Can't move empty slot
            r = new Reserve(-1, -1, -1, -1);
            Assert.False(r.CanMove(0, f, out idx));
            Assert.Equal(-1, idx);
        }

        [Fact]
        public void Insert_inserts_card_and_maintains_freeCount()
        {
            var r = new Reserve();

            r.Insert(0, Card.Get("3H"));
            Assert.Equal(Card.Get("3H"), r[0]);
            Assert.Equal(3, r.FreeCount);

            r.Insert(1, Card.Get("5D"));
            Assert.Equal(Card.Get("5D"), r[1]);
            Assert.Equal(2, r.FreeCount);
        }

        [Fact]
        public void Move_moves_card_to_tableau()
        {
            var t = new Tableau("5H");
            var r = new Reserve(Card.Get("4S").RawValue, -1, -1, -1);

            r.Move(0, t);

            Assert.Null(r[0]);
            Assert.Equal(4, r.FreeCount);
            Assert.Equal(2, t.Size);
            Assert.Equal(2, t.SortedSize);
            Assert.Equal(Card.Get("4S"), t.Top);
        }

        [Fact]
        public void Move_moves_card_to_foundation()
        {
            var card = Cards.AceOfClubs;
            var f = new Foundation();
            var r = new Reserve(Card.Get("AC").RawValue, -1, -1, -1);

            r.Move(0, f);

            Assert.Null(r[0]);
            Assert.Equal(4, r.FreeCount);
            Assert.Equal(0, f[Suit.Clubs]);
        }

        [Fact]
        public void Clone_clones_object()
        {
            var r = new Reserve(0, 3, -1, 2);
            var clone = r.Clone();

            Assert.Equal(r.FreeCount, clone.FreeCount);
            Assert.Equal(0, clone[0].RawValue);
            Assert.Equal(3, clone[1].RawValue);
            Assert.Null(clone[2]);
            Assert.Equal(2, clone[3].RawValue);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal($"01 02 03 04{Environment.NewLine}-- 5H -- KD", new Reserve(-1, Card.Get("5H").RawValue, -1, Card.Get("KD").RawValue).ToString());
    }
}
