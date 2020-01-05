using System;
using Xunit;

namespace FreeCellSolver.Test
{
    public class FoundationTests
    {
        [Fact]
        public void State_is_initialized_to_empty_foundation()
        {
            var f = new Foundation();

            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(-1, f[(Suit)i]);
            }
        }

        [Fact]
        public void Indexer_returns_value_of_specified_suit()
        {
            var f = new Foundation(5, -1, -1, -1);
            Assert.Equal(5, f[Suit.Clubs]);
        }

        [Fact]
        public void IsComplete_returns_whether_foundation_is_complete()
        {
            // Complete
            Assert.True(new Foundation(12, 12, 12, 12).IsComplete);
            // Missing King of Spades
            Assert.False(new Foundation(12, 12, 12, 11).IsComplete);
        }

        [Fact]
        public void CanPush_returns_whether_card_can_be_pushed()
        {
            var f = new Foundation(-1, -1, -1, -1);

            Assert.True(f.CanPush(Card.Get("AC")));
            Assert.False(f.CanPush(Card.Get("2C")));
        }

        [Fact]
        public void Push_pushes_card_to_foundation_slot()
        {
            var f = new Foundation(-1, -1, -1, -1);

            Assert.Equal(-1, f[Suit.Clubs]);
            f.Push(Card.Get("AC"));
            Assert.Equal(0, f[Suit.Clubs]);
        }

        [Fact]
        public void Clone_clones_object()
        {
            var f = new Foundation(0, 3, -1, 2);
            var clone = f.Clone();

            Assert.Equal(0, clone[Suit.Clubs]);
            Assert.Equal(3, clone[Suit.Diamonds]);
            Assert.Equal(-1, clone[Suit.Hearts]);
            Assert.Equal(2, clone[Suit.Spades]);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal($"CC DD HH SS{Environment.NewLine}AC 4D -- KS", new Foundation(0, 3, -1, 12).ToString());
    }
}
