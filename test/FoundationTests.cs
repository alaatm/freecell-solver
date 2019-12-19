using Xunit;

namespace FreeCellSolver.Test
{
    public class FoundationTests
    {
        [Fact]
        public void State_is_initialized_to_empty_foundation()
        {
            var foundation = new Foundation();

            foreach (var suit in Suits.All())
            {
                Assert.Equal(-1, foundation[suit]);
            }
        }

        [Theory]
        [InlineData(Rank.Ace, -1, true)]  // Ace to empty
        [InlineData(Rank.R2, -1, false)]  // Two to empty
        [InlineData(Rank.R2, 1, true)]   // Two to ace
        public void CanPush_returns_whether_card_can_be_pushed_or_not(Rank rankToPush, int currentTop, bool expectedCanPush)
        {
            var suit = Suit.Spades;
            var foundation = new Foundation();

            for (var r = 0; r < currentTop; r++)
            {
                foundation.Push(new Card(suit, (Rank)r));
            }

            Assert.Equal(expectedCanPush, foundation.CanPush(new Card(suit, rankToPush)));
        }

        [Fact]
        public void PushTest()
        {
            var foundation = new Foundation();
            var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };

            foreach (var suit in suits)
            {
                Assert.Equal(-1, foundation[suit]);

                for (var r = 0; r < 13; r++)
                {
                    var card = new Card(suit, (Rank)r);
                    foundation.Push(card);

                    Assert.Equal(r, foundation[suit]);
                    Assert.Equal(r, foundation[suit]);
                }
            }
        }
    }
}
