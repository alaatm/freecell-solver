using Xunit;

namespace FreeCellSolver.Test
{
    public class CardTests
    {
        [Fact]
        public void Can_ctor_using_string()
        {
            var card = Card.Get("4H");

            Assert.Equal(Suit.Hearts, card.Suit);
            Assert.Equal(Rank.R4, card.Rank);
            Assert.Equal("4H", card.ToString());
        }

        [Fact]
        public void Can_ctor_using_explicit_values()
        {
            var card = Card.Get(Suit.Hearts, Rank.R4);

            Assert.Equal(Suit.Hearts, card.Suit);
            Assert.Equal(Rank.R4, card.Rank);
            Assert.Equal("4H", card.ToString());
        }

        [Theory]
        [InlineData("3S", "2S", true)]   // 3 of spades -> 2 of spades
        [InlineData("3H", "2S", false)]  // 3 of hearts -> 2 of spades
        [InlineData("3S", "AS", false)]  // 3 of spades -> ace of spades
        public void IsAbove_returns_whether_card_is_above_another_for_foundation_stack(string check, string against, bool expectedIsAbove)
        {
            var cardToCheck = Card.Get(check);
            Assert.Equal(expectedIsAbove, cardToCheck.IsAbove(Card.Get(against)));
        }

        [Theory]
        [InlineData("3S", "2S", false)]   // 3 of spades -> 2 of spades
        [InlineData("3S", "2H", false)]   // 3 of spades -> 2 of hearts
        [InlineData("3S", "3C", false)]   // 3 of spades -> 3 of clubs
        [InlineData("3S", "3H", false)]   // 3 of spades -> 3 of hearts
        [InlineData("3S", "4S", false)]   // 3 of spades -> 4 of clubs
        [InlineData("3S", "4H", true)]    // 3 of spades -> 5 of hearts
        [InlineData("3S", "4D", true)]    // 3 of spades -> 5 of diamonds
        public void IsBelow_returns_whether_card_is_can_stack_above_tableau_top_card(string check, string against, bool expectedIsBelow)
        {
            var cardToCheck = Card.Get(check);
            Assert.Equal(expectedIsBelow, cardToCheck.IsBelow(Card.Get(against)));
        }

        [Fact]
        public void EqualityTest()
        {
            var ks1 = Card.Get("KS");
            var ks2 = Card.Get("KS");

            Assert.True(ks1 == ks2);
            Assert.True(ks1.Equals(ks2));
            Assert.True(object.Equals(ks1, ks2));
        }
    }
}
