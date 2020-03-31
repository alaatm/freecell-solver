using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class CardTests
    {
        [Fact]
        public void Can_get_card_by_rawValue()
        {
            for (sbyte i = 0; i < 52; i++)
            {
                var card = Card.Get(i);
                Assert.Equal(i & 3, card.Suit);
                Assert.Equal(i >> 2, card.Rank);
            }
        }

        [Fact]
        public void Can_get_card_by_string()
        {
            const string ranks = "A23456789TJQK";
            const string suits = "CDHS";

            foreach (var r in ranks)
            {
                foreach (var s in suits)
                {
                    var card = Card.Get($"{r}{s}");
                    Assert.Equal(suits.IndexOf(s), card.Suit);
                    Assert.Equal(ranks.IndexOf(r), card.Rank);
                }
            }
        }

        [Fact]
        public void Can_get_card_by_suit_and_rank()
        {
            for (var r = Ranks.Ace; r <= Ranks.Rk; r++)
            {
                for (var s = Suits.Clubs; s <= Suits.Spades; s++)
                {
                    var card = Card.Get(s, r);
                    Assert.Equal(s, card.Suit);
                    Assert.Equal(r, card.Rank);
                }
            }
        }

        [Theory]
        [InlineData("3S", "4S", false)]
        [InlineData("3S", "3H", false)]
        [InlineData("3S", "4H", true)]
        public void IsBelow_returns_whether_card_can_go_below_specfied_tableau_top(string check, string top, bool expectedIsBelow)
            => Assert.Equal(expectedIsBelow, Card.Get(check).IsBelow(Card.Get(top)));

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal("AS", Card.Get("AS").ToString());
    }
}
