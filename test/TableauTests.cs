using System;
using System.Linq;
using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class TableauTests
    {
        [Theory]
        [InlineData("", 0, 0)]
        [InlineData("KD", 1, 1)]
        [InlineData("KD 5H 4S 2D", 4, 1)]
        [InlineData("KD 5H 4S", 3, 2)]
        [InlineData("6C 5H 4S", 3, 3)]
        public void Size_props_are_properly_initialized(string tableau, int expectedSize, int expectedSortedSize)
        {
            // Arrange & act
            var t = Tableau.Create(tableau);

            // Assert
            Assert.Equal(expectedSize, t.Size);
            Assert.Equal(expectedSortedSize, t.SortedSize);
        }

        [Fact]
        public void IsEmpty_returns_whether_tableau_is_empty()
        {
            Assert.True(Tableau.Create().IsEmpty);
            Assert.False(Tableau.Create("AS").IsEmpty);
        }

        [Fact]
        public void Indexer_returns_cards_starting_from_bottom()
        {
            var t = Tableau.Create("5H 9S 2D");
            Assert.Equal(Card.Get("2D"), t[2]);
            Assert.Equal(Card.Get("5H"), t[0]);
        }

        [Fact]
        public void Top_returns_top_card_or_null_if_empty()
        {
            var t = Tableau.Create("5H 9S 2D");
            Assert.Equal(Card.Get("2D"), t.Top);

            t = Tableau.Create("");
            Assert.Equal(Card.Null, t.Top);
        }

        [Fact]
        public void Top_is_tracked_when_pushing()
        {
            var t = Tableau.Create("5H");
            Assert.Equal(Card.Get("5H"), t.Top);

            t.Push(Card.Get("4S"));
            Assert.Equal(Card.Get("4S"), t.Top);
        }

        [Fact]
        public void Top_is_tracked_when_popping()
        {
            var t = Tableau.Create("5H 4S");
            Assert.Equal(Card.Get("4S"), t.Top);

            t.Pop();
            Assert.Equal(Card.Get("5H"), t.Top);

            t.Pop();
            Assert.Equal(Card.Null, t.Top);
        }

        [Theory]
        [InlineData("", "5H", true)]
        [InlineData("6S", "5H", true)]
        [InlineData("4H", "5H", false)]
        public void CanPush_returns_whether_card_can_be_placed_on_top(string top, string card, bool canPush)
            => Assert.Equal(canPush, Tableau.Create(top).CanPush(Card.Get(card)));

        [Fact]
        public void CanPop_returns_whether_top_can_be_popped()
        {
            // Can't pop when empty
            Assert.False(Tableau.Create("").CanPop());
            Assert.True(Tableau.Create("5H").CanPop());
        }

        [Fact]
        public void CanMove_returns_whether_top_can_be_moved_to_reserve()
        {
            // Can't move when empty
            Assert.False(Tableau.Create().CanMove(Reserve.Create(), out var idx));
            Assert.Equal(-1, idx);

            // Can't move when non-empty but full reserve
            Assert.False(Tableau.Create("5H").CanMove(Reserve.Create("AC", "AD", "AH", "AS"), out idx));
            Assert.Equal(-1, idx);

            // Can move to reserve slot #2 (index=1)
            Assert.True(Tableau.Create("5H").CanMove(Reserve.Create("AD", null, "AH", "AS"), out idx));
            Assert.Equal(1, idx);
        }

        [Fact]
        public void CanMove_returns_whether_top_can_be_moved_to_foundation()
        {
            // Can't move when empty
            Assert.False(Tableau.Create().CanMove(Foundation.Create()));

            // Can't move when non-empty but no valid foundation target
            Assert.False(Tableau.Create("5H").CanMove(Foundation.Create()));

            // Can move to foundation on top of 4H
            Assert.True(Tableau.Create("5H").CanMove(Foundation.Create(Ranks.Nil, Ranks.Nil, Ranks.R4, Ranks.Nil)));
        }

        [Fact]
        public void Push_pushes_card_to_top_and_maintains_size_and_sortedSize()
        {
            var t = Tableau.Create("8S 2H 9C 8H");
            Assert.Equal(4, t.Size);
            Assert.Equal(2, t.SortedSize);

            t.Push(Card.Get("7C"));
            Assert.Equal(Card.Get("7C"), t.Top);
            Assert.Equal(5, t.Size);
            Assert.Equal(3, t.SortedSize);
        }

        [Fact]
        public void Push_tests()
        {
            // Arrange
            var cards = "8D 7C 6H 5S 4H 3C 2D";
            var cardsArr = cards.Split(' ').Select(c => Card.Get(c)).ToArray();

            var t = Tableau.Create("");
            Assert.Equal(0, t.Size);
            Assert.Equal(0, t.SortedSize);
            Assert.Null(t.Top);

            // Act & assert
            for (var i = 0; i < cardsArr.Length; i++)
            {
                t.Push(cardsArr[i]);

                Assert.Equal(cardsArr[i], t.Top);
                Assert.Equal(i + 1, t.Size);
                Assert.Equal(i + 1, t.SortedSize);
            }
        }

        [Fact]
        public void Pop_pops_top_and_maintains_size_and_sortedSize()
        {
            // Arrange
            var t = Tableau.Create("8S 2H 9C 8H");
            Assert.Equal(4, t.Size);
            Assert.Equal(2, t.SortedSize);

            // Act
            t.Pop();

            // Assert
            Assert.Equal(Card.Get("9C"), t.Top);
            Assert.Equal(3, t.Size);
            Assert.Equal(1, t.SortedSize);
        }

        [Fact]
        public void Pop_updates_stack_when_col_is_sorted_after_pop_operation()
        {
            // Arrange
            var t = Tableau.Create("TH 9C 7C");

            // Act
            t.Pop();

            // Assert
            Assert.Equal(2, t.Size);
            Assert.Equal(2, t.SortedSize);
            Assert.Equal(Card.Get("9C"), t.Top);
            Assert.Equal(Card.Get("9C"), t[1]);
            Assert.Equal(Card.Get("TH"), t[0]);
        }

        [Fact]
        public void Pop_tests()
        {
            // Arrange
            var cards = "8D 7C 6H 5S 4H 3C 2D";
            var cardsArr = cards.Split(' ').Select(c => Card.Get(c)).ToArray();

            var t = Tableau.Create(cards);
            Assert.Equal(7, t.Size);
            Assert.Equal(7, t.SortedSize);
            Assert.Equal(cardsArr[^1], t.Top);

            // Act & assert
            for (var i = cardsArr.Length - 1; i >= 0; i--)
            {
                Assert.Equal(cardsArr[i], t.Pop());
                if (i > 0)
                {
                    Assert.Equal(cardsArr[i - 1], t.Top);
                }
                else
                {
                    Assert.Null(t.Top);
                }
                Assert.Equal(i, t.Size);
                Assert.Equal(i, t.SortedSize);
            }
        }

        [Fact]
        public void Move_moves_cards_from_tableau_to_another_when_requestedSize_equals_1()
        {
            var t1 = Tableau.Create("8S 2H 9C 8H");
            var t2 = Tableau.Create("");

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
            var t1 = Tableau.Create("8S 2H 9C 8H");
            var t2 = Tableau.Create("");

            t1.Move(t2, 2);

            Assert.Equal(Card.Get("2H"), t1.Top);
            Assert.Equal(2, t1.Size);
            Assert.Equal(1, t1.SortedSize);

            Assert.Equal(Card.Get("8H"), t2.Top);
            Assert.Equal(2, t2.Size);
            Assert.Equal(2, t2.SortedSize);

            Assert.Equal(Card.Get("8H"), t2[1]);
            Assert.Equal(Card.Get("9C"), t2[0]);
        }

        [Fact]
        public void Move_moves_cards_from_tableau_to_another_and_maintains_top1()
        {
            var t1 = Tableau.Create("9C 8H");
            var t2 = Tableau.Create("TH");

            t1.Move(t2, 2);

            Assert.Equal(Card.Null, t1.Top);
            Assert.Equal(0, t1.Size);
            Assert.Equal(0, t1.SortedSize);

            Assert.Equal(Card.Get("8H"), t2.Top);
            Assert.Equal(3, t2.Size);
            Assert.Equal(3, t2.SortedSize);

            Assert.Equal(Card.Get("8H"), t2[2]);
            Assert.Equal(Card.Get("9C"), t2[1]);
            Assert.Equal(Card.Get("TH"), t2[0]);
        }

        [Fact]
        public void Move_moves_cards_from_tableau_to_another_and_maintains_top2()
        {
            var t1 = Tableau.Create("TD 9C 8H");
            var t2 = Tableau.Create("TH");

            t1.Move(t2, 2);

            Assert.Equal(Card.Get("TD"), t1.Top);
            Assert.Equal(1, t1.Size);
            Assert.Equal(1, t1.SortedSize);

            Assert.Equal(Card.Get("8H"), t2.Top);
            Assert.Equal(3, t2.Size);
            Assert.Equal(3, t2.SortedSize);

            Assert.Equal(Card.Get("8H"), t2[2]);
            Assert.Equal(Card.Get("9C"), t2[1]);
            Assert.Equal(Card.Get("TH"), t2[0]);
        }

        [Fact]
        public void Move_updates_stack_when_col_is_sorted_after_move_operation()
        {
            // Arrange
            var t1 = Tableau.Create("TS 4D 4S 3D 9D 8C");
            var t2 = Tableau.Create("TC");

            // Act
            t1.Move(t2, 2);

            // Assert
            Assert.Equal(4, t1.Size);
            Assert.Equal(2, t1.SortedSize);
            Assert.Equal(Card.Get("3D"), t1.Top);
            Assert.Equal(Card.Get("3D"), t1[3]);
            Assert.Equal(Card.Get("4S"), t1[2]);
            Assert.Equal(Card.Get("4D"), t1[1]);
            Assert.Equal(Card.Get("TS"), t1[0]);
        }

        [Fact]
        public void Move_moves_card_to_reserve()
        {
            var t = Tableau.Create("8S 2H 9C 8H");
            var r = Reserve.Create();

            t.Move(r, 0);

            Assert.Equal(Card.Get("9C"), t.Top);
            Assert.Equal(3, t.Size);
            Assert.Equal(1, t.SortedSize);

            Assert.Equal(Card.Get("8H"), r[0]);
        }

        [Fact]
        public void Move_moves_card_to_foundation()
        {
            var t = Tableau.Create("8S 2H 9C AH");
            var f = Foundation.Create();

            t.Move(f);

            Assert.Equal(Card.Get("9C"), t.Top);
            Assert.Equal(3, t.Size);
            Assert.Equal(1, t.SortedSize);

            Assert.Equal(Ranks.R2, f[Suits.Hearts]);
        }

        [Theory]
        [InlineData("", "", 0)]
        [InlineData("", "3H", 0)]
        [InlineData("3H", "", 1)]
        [InlineData("3H", "5S", 0)]
        [InlineData("3H", "4S", 1)]
        [InlineData("3H", "4H", 0)]
        [InlineData("AH 5H 4S", "2C", 0)]
        [InlineData("AH 5H 4S", "5D", 1)]
        [InlineData("AH 5H 4S", "6S", 2)]
        [InlineData("AH 5H 4S", "6H", 0)]
        [InlineData("2H QS JD TC 9H 8S 7D 6C 5H 4S 3D 2S AD", "KH", 12)]
        public void CountMovable_returns_num_of_cards_that_can_be_moved_to_target(string stack, string targetTop, int expectedMovableCount)
        {
            var t1 = Tableau.Create(stack);
            var t2 = Tableau.Create(targetTop);
            Assert.Equal(expectedMovableCount, t1.CountMovable(t2));
        }

        [Fact]
        public void Clone_clones_object()
        {
            var t = Tableau.Create("8S 2H 9C 8H");
            var clone = t.Clone();

            Assert.True(t.Equals(clone));
            Assert.Equal(Card.Get("8H"), clone.Top);
            Assert.Equal(Card.Get("8H"), clone[3]);
            Assert.Equal(Card.Get("9C"), clone[2]);
            Assert.Equal(Card.Get("2H"), clone[1]);
            Assert.Equal(Card.Get("8S"), clone[0]);
            Assert.Equal(4, clone.Size);
            Assert.Equal(2, clone.SortedSize);

            Assert.Same(t.Top, clone.Top);
            Assert.NotSame(t, clone);
            var fi = typeof(Tableau).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotSame(fi.GetValue(t), fi.GetValue(clone));
        }

        [Fact]
        public void Clone_clones_empty_tableau()
        {
            var t = Tableau.Create("");
            var clone = t.Clone();

            Assert.Equal(Card.Null, clone.Top);
            Assert.Equal(0, clone.Size);
            Assert.Equal(0, clone.SortedSize);
        }

        [Fact]
        public void Equals_only_checks_occupied_slots_when_checking_equality()
        {
            // Arrange
            var t1 = Tableau.Create("5C 8C 2H 9D");
            var t2 = Tableau.Create("5C 8C 2H KC");

            t1.Move(Tableau.Create(), 1);
            t2.Move(Tableau.Create(), 1);

            // Act
            var equals = t1.Equals(t2);

            // Assert
            Assert.True(equals);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal($"8S{Environment.NewLine}2H{Environment.NewLine}9C{Environment.NewLine}8H", Tableau.Create("8S 2H 9C 8H").ToString());

        [Fact]
        public void AllCards_returns_all_cards()
        {
            var t = Tableau.Create("KS QS");
            var allCards = t.AllCards().ToList();

            // Assert
            Assert.Equal(2, allCards.Count);
            Assert.Equal(Card.Get("KS"), allCards[0]);
            Assert.Equal(Card.Get("QS"), allCards[1]);

            Assert.Empty(Tableau.Create().AllCards());
        }
    }
}