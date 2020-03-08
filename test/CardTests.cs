using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class CardTests
    {
        [Fact]
        public void Can_get_card_by_rawValue()
        {
            for (short i = 0; i < 52; i++)
            {
                var card = Card.Get(i);
                Assert.Equal(i & 3, card.Suit);
                Assert.Equal(i >> 2, card.Rank);
            }
        }

        [Fact]
        public void Can_get_card_by_string()
        {
            Assert.Equal("CDHS", Card.SUITS);
            Assert.Equal("A23456789TJQK", Card.RANKS);

            foreach (var r in Card.RANKS)
            {
                foreach (var s in Card.SUITS)
                {
                    var card = Card.Get($"{r}{s}");
                    Assert.Equal(Card.SUITS.IndexOf(s), card.Suit);
                    Assert.Equal(Card.RANKS.IndexOf(r), card.Rank);
                }
            }
        }

        [Fact]
        public void Can_get_card_by_suit_and_rank()
        {
            for (var r = Ranks.ACE; r <= Ranks.RK; r++)
            {
                for (var s = Suits.CLUBS; s <= Suits.SPADES; s++)
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
