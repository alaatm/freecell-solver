using System;
using System.Linq;
using FreeCellSolver.Game;
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
                Assert.Equal(Card.Null, r[i]);
            }
        }

        [Fact]
        public void Indexer_returns_value_of_specified_slot()
        {
            var r = new Reserve("AD", null, "AH");
            Assert.Equal(Card.Get("AD"), r[0]);
            Assert.Equal(Card.Null, r[1]);
            Assert.Equal(Card.Get("AH"), r[2]);
            Assert.Equal(Card.Null, r[3]);
        }

        [Fact]
        public void FreeCount_is_properly_initialized()
        {
            // empty reserve
            Assert.Equal(4, new Reserve().FreeCount);
            // 2 spots occupied, 2 free
            Assert.Equal(2, new Reserve("AD", null, "AH").FreeCount);
        }

        [Fact]
        public void CanInsert_returns_whether_card_can_be_inserted_along_with_target_index()
        {
            // Index 2 free
            var r = new Reserve("AC", "AD", null, "AH");
            Assert.True(r.CanInsert(out var index));
            Assert.Equal(2, index);

            // Full
            r = new Reserve("AC", "AD", "2D", "AH");
            Assert.False(r.CanInsert(out index));
            Assert.Equal(-1, index);
        }

        [Fact]
        public void CanMove_returns_whether_card_can_be_moved_to_tableau()
        {
            // Move 5H to empty tableau
            var r = new Reserve("5H");
            var t = new Tableau();
            Assert.True(r.CanMove(0, t));

            // Move 5H below 6S
            t = new Tableau("6C");
            Assert.True(r.CanMove(0, t));

            // Can't move 5H below 9C
            t = new Tableau("9C");
            Assert.False(r.CanMove(0, t));

            // Can't move empty slot
            r = new Reserve();
            Assert.False(r.CanMove(0, t));
        }

        [Fact]
        public void CanMove_returns_whether_card_can_be_moved_to_foundation_or_not()
        {
            // Move AC to empty foundation
            var r = new Reserve("AC");
            var f = new Foundation();
            Assert.True(r.CanMove(0, f));

            // Can't move 2C to empty foundation
            r = new Reserve("2C");
            Assert.False(r.CanMove(0, f));

            // Can't move empty slot
            r = new Reserve();
            Assert.False(r.CanMove(0, f));
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
            var r = new Reserve("4S");

            r.Move(0, t);

            Assert.Equal(Card.Null, r[0]);
            Assert.Equal(4, r.FreeCount);
            Assert.Equal(2, t.Size);
            Assert.Equal(2, t.SortedSize);
            Assert.Equal(Card.Get("4S"), t.Top);
        }

        [Fact]
        public void Move_moves_card_to_foundation()
        {
            var f = new Foundation();
            var r = new Reserve("AC");

            r.Move(0, f);

            Assert.Equal(Card.Null, r[0]);
            Assert.Equal(4, r.FreeCount);
            Assert.Equal(Ranks.R2, f[Suits.Clubs]);
        }

        [Fact]
        public void Clone_clones_object()
        {
            var r = new Reserve("AC", "AS", null, "AH");
            var clone = r.Clone();

            Assert.True(r.Equals(clone));
            Assert.Equal(r.FreeCount, clone.FreeCount);
            Assert.Equal(Card.Get("AC"), clone[0]);
            Assert.Equal(Card.Get("AS"), clone[1]);
            Assert.Equal(Card.Null, clone[2]);
            Assert.Equal(Card.Get("AH"), clone[3]);

            Assert.NotSame(r, clone);
            var fi = typeof(Reserve).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotSame(fi.GetValue(r), fi.GetValue(clone));
        }

        [Theory]
        [InlineData(",,,", ",,,", true)]
        [InlineData("AC,,,", "AC,,,", true)]
        [InlineData("AC,,,", ",AC,,", true)]
        [InlineData("AC,,,", ",,AC,", true)]
        [InlineData("AC,,,", ",,,AC", true)]

        [InlineData("AC,AD,,", "AD,AC,,", true)]
        [InlineData("AC,AD,,", "AD,,AC,", true)]
        [InlineData("AC,AD,,", "AD,,,AC", true)]
        [InlineData("AC,AD,,", ",AD,AC,", true)]
        [InlineData("AC,AD,,", ",AD,,AC", true)]
        [InlineData("AC,AD,,", ",,AD,AC", true)]

        [InlineData("AC,AD,,", "AC,AD,,", true)]
        [InlineData("AC,AD,,", "AC,,AD,", true)]
        [InlineData("AC,AD,,", "AC,,,AD", true)]
        [InlineData("AC,AD,,", ",AC,AD,", true)]
        [InlineData("AC,AD,,", ",AC,,AD", true)]
        [InlineData("AC,AD,,", ",,AC,AD", true)]

        [InlineData("AC,,,", ",,,", false)]
        [InlineData(",,,", "AC,,,", false)]

        [InlineData("AC,AD,,", "AC,,,", false)]
        [InlineData("AC,,,", "AC,AD,,", false)]
        public void Equality_Tests(string cards1, string cards2, bool expected)
        {
            // Arrange
            var c1 = cards1.Split(',');
            var r1 = new Reserve(c1[0], c1[1], c1[2], c1[3]);
            var c2 = cards2.Split(',');
            var r2 = new Reserve(c2[0], c2[1], c2[2], c2[3]);

            // Act
            var equal = r1.Equals(r2);

            // Assert
            Assert.Equal(expected, equal);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal($"01 02 03 04{Environment.NewLine}-- 5H -- KD", new Reserve(null, "5H", null, "KD").ToString());

        [Fact]
        public void AllCards_returns_all_cards()
        {
            var r = new Reserve(null, "KS", "QS");
            var allCards = r.AllCards().ToList();

            // Assert
            Assert.Equal(2, allCards.Count);
            Assert.Equal(Card.Get("KS"), allCards[0]);
            Assert.Equal(Card.Get("QS"), allCards[1]);

            Assert.Empty(new Reserve().AllCards());
        }
    }
}
