using Xunit;

namespace FreeCellSolver.Test
{
    public class FoundationTests
    {
        [Fact]
        public void State_is_initialized_to_empty_foundation()
        {
            var foundation = new Foundation();

            foreach (var suit in Suits.Values)
            {
                Assert.Equal(-1, foundation[suit]);
            }
        }

        [Theory]
        [InlineData(Rank.Ace, -1, true)]  // Ace to empty
        [InlineData(Rank.R2, -1, false)]  // Two to empty
        [InlineData(Rank.R2, 1, true)]    // Two to ace
        public void CanPush_returns_whether_card_can_be_pushed_or_not(Rank rankToPush, int currentTop, bool expectedCanPush)
        {
            var suit = Suit.Spades;
            var foundation = new Foundation();

            for (var r = 0; r < currentTop; r++)
            {
                foundation.Push(Card.Get(suit, (Rank)r));
            }

            Assert.Equal(expectedCanPush, foundation.CanPush(Card.Get(suit, rankToPush)));
        }

        [Fact]
        public void PushTest()
        {
            var foundation = new Foundation();

            foreach (var suit in Suits.Values)
            {
                Assert.Equal(-1, foundation[suit]);

                foreach (var rank in Ranks.Values)
                {
                    var card = Card.Get(suit, rank);
                    foundation.Push(card);

                    Assert.Equal((int)rank, foundation[suit]);
                    Assert.Equal((int)rank, foundation[suit]);
                }
            }
        }
    }
}
