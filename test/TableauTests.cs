using System;
using System.Linq;
using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class TableauTests
    {
        [Fact]
        public void Size_is_properly_initialized()
        {
            Assert.Equal(0, new Tableau("").Size);
            Assert.Equal(4, new Tableau("KD5H4S2D").Size);
        }

        [Fact]
        public void SortedSize_is_properly_initialized()
        {
            Assert.Equal(0, new Tableau("").SortedSize);
            Assert.Equal(1, new Tableau("KD").SortedSize);
            // 5H and 4S are not at top
            Assert.Equal(1, new Tableau("KD5H4S2D").SortedSize);
            // 5H and 4S are at top
            Assert.Equal(2, new Tableau("KD5H4S").SortedSize);
            // All sorted
            Assert.Equal(3, new Tableau("6C5H4S").SortedSize);
        }

        [Fact]
        public void IsEmpty_returns_whether_tableau_is_empty()
        {
            Assert.True(new Tableau("").IsEmpty);
            Assert.False(new Tableau("AS").IsEmpty);
        }

        [Fact]
        public void Indexer_returns_cards_starting_from_top()
        {
            var t = new Tableau("5H9S2D");
            Assert.Equal(Card.Get("2D"), t[0]);
            Assert.Equal(Card.Get("5H"), t[2]);
        }

        [Fact]
        public void Top_returns_top_card_or_null_if_empty()
        {
            var t = new Tableau("5H9S2D");
            Assert.Equal(Card.Get("2D"), t.Top);

            t = new Tableau("");
            Assert.Null(t.Top);
        }

        [Fact]
        public void Top_is_tracked_when_pushing()
        {
            var t = new Tableau("5H");
            Assert.Equal(Card.Get("5H"), t.Top);

            t.Push(Card.Get("4S"));
            Assert.Equal(Card.Get("4S"), t.Top);
        }

        [Fact]
        public void Top_is_tracked_when_popping()
        {
            var t = new Tableau("5H4S");
            Assert.Equal(Card.Get("4S"), t.Top);

            t.Pop();
            Assert.Equal(Card.Get("5H"), t.Top);

            t.Pop();
            Assert.Null(t.Top);
        }

        [Theory]
        [InlineData("", "5H", true)]
        [InlineData("6S", "5H", true)]
        [InlineData("4H", "5H", false)]
        public void CanPush_returns_whether_card_can_be_placed_on_top(string top, string card, bool canPush)
            => Assert.Equal(canPush, new Tableau(top).CanPush(Card.Get(card)));

        [Fact]
        public void CanPop_returns_whether_top_can_be_popped()
        {
            // Can't pop when empty
            Assert.False(new Tableau("").CanPop());
            Assert.True(new Tableau("5H").CanPop());
        }

        [Fact]
        public void CanMove_returns_whether_top_can_be_moved_to_reserve()
        {
            // Can't move when empty
            Assert.False(new Tableau("").CanMove(new Reserve(), out _));

            // Can't move when non-empty but full reserve
            Assert.False(new Tableau("5H").CanMove(new Reserve(Card.Get(0).RawValue, Card.Get(1).RawValue, Card.Get(2).RawValue, Card.Get(3).RawValue), out _));

            // Can move to reserve slot #2 (index=1)
            Assert.True(new Tableau("5H").CanMove(new Reserve(Card.Get(1).RawValue, Card.Empty, Card.Get(2).RawValue, Card.Get(3).RawValue), out var idx));
            Assert.Equal(1, idx);
        }

        [Fact]
        public void CanMove_returns_whether_top_can_be_moved_to_foundation()
        {
            // Can't move when empty
            Assert.False(new Tableau("").CanMove(new Foundation(), out _));

            // Can't move when non-empty but no valid foundation target
            Assert.False(new Tableau("5H").CanMove(new Foundation(), out _));

            // Can move to foundation on top of 4H
            Assert.True(new Tableau("5H").CanMove(new Foundation(Card.Empty, Card.Empty, Ranks.R4, Card.Empty), out var idx));
            Assert.Equal(Suits.Hearts, idx);
        }

        [Fact]
        public void Push_pushes_card_to_top_and_maintains_size_and_sortedSize()
        {
            var t = new Tableau("8S2H9C8H");
            Assert.Equal(4, t.Size);
            Assert.Equal(2, t.SortedSize);

            t.Push(Card.Get("7C"));
            Assert.Equal(Card.Get("7C"), t.Top);
            Assert.Equal(5, t.Size);
            Assert.Equal(3, t.SortedSize);
        }

        [Fact]
        public void Pop_pops_top_and_maintains_size_and_sortedSize()
        {
            var t = new Tableau("8S2H9C8H");
            Assert.Equal(4, t.Size);
            Assert.Equal(2, t.SortedSize);

            t.Pop();
            Assert.Equal(Card.Get("9C"), t.Top);
            Assert.Equal(3, t.Size);
            Assert.Equal(1, t.SortedSize);
        }

        [Fact]
        public void Move_moves_cards_from_tableau_to_another_when_requestedSize_equals_1()
        {
            var t1 = new Tableau("8S2H9C8H");
            var t2 = new Tableau("");

            t1.Move(t2, 1);

            Assert.Equal(Card.Get("9C"), t1.Top);
            Assert.Equal(3, t1.Size);
            Assert.Equal(1, t1.SortedSize);

            Assert.Equal(Card.Get("8H"), t2.Top);
            Assert.Equal(1, t2.Size);
            Assert.Equal(1, t2.SortedSize);

            Assert.Equal(Card.Get("8H"), t2[0]);
        }

        [Fact]
        public void Move_moves_cards_from_tableau_to_another_when_requestedSize_above_1()
        {
            var t1 = new Tableau("8S2H9C8H");
            var t2 = new Tableau("");

            t1.Move(t2, 2);

            Assert.Equal(Card.Get("2H"), t1.Top);
            Assert.Equal(2, t1.Size);
            Assert.Equal(1, t1.SortedSize);

            Assert.Equal(Card.Get("8H"), t2.Top);
            Assert.Equal(2, t2.Size);
            Assert.Equal(2, t2.SortedSize);

            Assert.Equal(Card.Get("8H"), t2[0]);
            Assert.Equal(Card.Get("9C"), t2[1]);
        }

        [Fact]
        public void Move_moves_cards_from_tableau_to_another_and_maintains_top1()
        {
            var t1 = new Tableau("9C8H");
            var t2 = new Tableau("TH");

            t1.Move(t2, 2);

            Assert.Null(t1.Top);
            Assert.Equal(0, t1.Size);
            Assert.Equal(0, t1.SortedSize);

            Assert.Equal(Card.Get("8H"), t2.Top);
            Assert.Equal(3, t2.Size);
            Assert.Equal(3, t2.SortedSize);

            Assert.Equal(Card.Get("8H"), t2[0]);
            Assert.Equal(Card.Get("9C"), t2[1]);
            Assert.Equal(Card.Get("TH"), t2[2]);
        }

        [Fact]
        public void Move_moves_cards_from_tableau_to_another_and_maintains_top2()
        {
            var t1 = new Tableau("TD9C8H");
            var t2 = new Tableau("TH");

            t1.Move(t2, 2);

            Assert.Equal(Card.Get("TD"), t1.Top);
            Assert.Equal(1, t1.Size);
            Assert.Equal(1, t1.SortedSize);

            Assert.Equal(Card.Get("8H"), t2.Top);
            Assert.Equal(3, t2.Size);
            Assert.Equal(3, t2.SortedSize);

            Assert.Equal(Card.Get("8H"), t2[0]);
            Assert.Equal(Card.Get("9C"), t2[1]);
            Assert.Equal(Card.Get("TH"), t2[2]);
        }

        [Fact]
        public void Move_moves_card_to_reserve()
        {
            var t = new Tableau("8S2H9C8H");
            var r = new Reserve();

            t.Move(r, 0);

            Assert.Equal(Card.Get("9C"), t.Top);
            Assert.Equal(3, t.Size);
            Assert.Equal(1, t.SortedSize);

            Assert.Equal(Card.Get("8H"), r[0]);
        }

        [Fact]
        public void Move_moves_card_to_foundation()
        {
            var t = new Tableau("8S2H9CAH");
            var f = new Foundation();

            t.Move(f);

            Assert.Equal(Card.Get("9C"), t.Top);
            Assert.Equal(3, t.Size);
            Assert.Equal(1, t.SortedSize);

            Assert.Equal(Ranks.Ace, f[Suits.Hearts]);
        }

        [Theory]
        [InlineData("", "", 0)]
        [InlineData("", "3H", 0)]
        [InlineData("3H", "", 1)]
        [InlineData("3H", "5S", 0)]
        [InlineData("3H", "4S", 1)]
        [InlineData("3H", "4H", 0)]
        [InlineData("AH5H4S", "2C", 0)]
        [InlineData("AH5H4S", "5D", 1)]
        [InlineData("AH5H4S", "6S", 2)]
        [InlineData("AH5H4S", "6H", 0)]
        [InlineData("2HQSJDTC9H8S7D6C5H4S3D2SAD", "KH", 12)]
        public void CountMovable_returns_num_of_cards_that_can_be_moved_to_target(string stack, string targetTop, int expectedMovableCount)
        {
            var t1 = new Tableau(stack);
            var t2 = new Tableau(targetTop);
            Assert.Equal(expectedMovableCount, t1.CountMovable(t2));
        }

        [Fact]
        public void Clone_clones_object()
        {
            var t = new Tableau("8S2H9C8H");
            var clone = t.Clone();

            Assert.Equal(Card.Get("8H"), clone.Top);
            Assert.Equal(Card.Get("8H"), clone[0]);
            Assert.Equal(Card.Get("9C"), clone[1]);
            Assert.Equal(Card.Get("2H"), clone[2]);
            Assert.Equal(Card.Get("8S"), clone[3]);
            Assert.Equal(4, clone.Size);
            Assert.Equal(2, clone.SortedSize);
        }

        [Fact]
        public void Clone_clones_empty_tableau()
        {
            var t = new Tableau("");
            var clone = t.Clone();

            Assert.Null(clone.Top);
            Assert.Equal(0, clone.Size);
            Assert.Equal(0, clone.SortedSize);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal($"8S{Environment.NewLine}2H{Environment.NewLine}9C{Environment.NewLine}8H", new Tableau("8S2H9C8H").ToString());

        [Fact]
        public void AllCards_returns_all_cards()
        {
            var t = new Tableau("KSQS");
            var allCards = t.AllCards().ToList();

            // Assert
            Assert.Equal(2, allCards.Count);
            Assert.Equal(Card.Get("KS"), allCards[0]);
            Assert.Equal(Card.Get("QS"), allCards[1]);

            Assert.Empty(new Tableau().AllCards());
        }
    }
}