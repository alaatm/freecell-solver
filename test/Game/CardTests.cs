using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class CardTests
    {
        [Fact]
        public void Can_get_card_by_rawValue()
        {
            for (var i = 4; i < 56; i++)
            {
                var card = Card.Get(i);
                Assert.Equal(i & 3, card.Suit);
                Assert.Equal(i >> 2, card.Rank);
                Assert.Equal(card.Suit == Suits.Hearts || card.Suit == Suits.Diamonds ? Colors.Red : Colors.Black, card.Color);
            }
        }

        [Fact]
        public void Can_get_card_by_string()
        {
            const string ranks = "A23456789TJQK";
            const string suits = "HCDS";

            foreach (var r in ranks)
            {
                foreach (var s in suits)
                {
                    var card = Card.Get($"{r}{s}");
                    Assert.Equal(suits.IndexOf(s), card.Suit);
                    Assert.Equal(ranks.IndexOf(r) + 1, card.Rank);
                    Assert.Equal(s == 'H' || s == 'D' ? Colors.Red : Colors.Black, card.Color);
                }
            }
        }

        [Fact]
        public void Can_get_card_by_suit_and_rank()
        {
            for (var r = Ranks.Ace; r <= Ranks.Rk; r++)
            {
                for (var s = Suits.Hearts; s <= Suits.Spades; s++)
                {
                    var card = Card.Get(s, r);
                    Assert.Equal(s, card.Suit);
                    Assert.Equal(r, card.Rank);
                    Assert.Equal(s == Suits.Hearts || s == Suits.Diamonds ? Colors.Red : Colors.Black, card.Color);
                }
            }
        }

        [Fact]
        public void All_returns_all_cards()
        {
            var cards = Card.All();

            Assert.Equal(52, cards.Length);

            var rank = 1;
            var suit = 0;
            for (var i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                Assert.Equal(i + 4, card.RawValue);
                Assert.Equal(rank, card.Rank);
                Assert.Equal(suit, card.Suit);

                if (++suit > 3)
                {
                    rank++;
                    suit = 0;
                }
            }
        }

        [Theory]
        [InlineData("3C", "2C", false)]
        [InlineData("3C", "2D", false)]
        [InlineData("3C", "2H", false)]
        [InlineData("3C", "2S", false)]
        [InlineData("3C", "3C", false)]
        [InlineData("3C", "3D", false)]
        [InlineData("3C", "3H", false)]
        [InlineData("3C", "3S", false)]
        [InlineData("3C", "4C", false)]
        [InlineData("3C", "4D", true)]
        [InlineData("3C", "4H", true)]
        [InlineData("3C", "4S", false)]

        [InlineData("3D", "2C", false)]
        [InlineData("3D", "2D", false)]
        [InlineData("3D", "2H", false)]
        [InlineData("3D", "2S", false)]
        [InlineData("3D", "3C", false)]
        [InlineData("3D", "3D", false)]
        [InlineData("3D", "3H", false)]
        [InlineData("3D", "3S", false)]
        [InlineData("3D", "4C", true)]
        [InlineData("3D", "4D", false)]
        [InlineData("3D", "4H", false)]
        [InlineData("3D", "4S", true)]

        [InlineData("3H", "2C", false)]
        [InlineData("3H", "2D", false)]
        [InlineData("3H", "2H", false)]
        [InlineData("3H", "2S", false)]
        [InlineData("3H", "3C", false)]
        [InlineData("3H", "3D", false)]
        [InlineData("3H", "3H", false)]
        [InlineData("3H", "3S", false)]
        [InlineData("3H", "4C", true)]
        [InlineData("3H", "4D", false)]
        [InlineData("3H", "4H", false)]
        [InlineData("3H", "4S", true)]

        [InlineData("3S", "2C", false)]
        [InlineData("3S", "2D", false)]
        [InlineData("3S", "2H", false)]
        [InlineData("3S", "2S", false)]
        [InlineData("3S", "3C", false)]
        [InlineData("3S", "3D", false)]
        [InlineData("3S", "3H", false)]
        [InlineData("3S", "3S", false)]
        [InlineData("3S", "4C", false)]
        [InlineData("3S", "4D", true)]
        [InlineData("3S", "4H", true)]
        [InlineData("3S", "4S", false)]
        public void IsBelow_returns_whether_card_can_go_below_another(string check, string top, bool expectedIsBelow)
            => Assert.Equal(expectedIsBelow, Card.Get(check).IsBelow(Card.Get(top)));

        [Fact]
        public void GetHashCode_tests()
        {
            Assert.Equal(
                Card.Get("AC").GetHashCode(),
                Card.Get("AC").GetHashCode());

            Assert.Equal(((Card)default).GetHashCode(), ((Card)default).GetHashCode());

            Assert.NotEqual(
                Card.Get("AC").GetHashCode(),
                Card.Get("AD").GetHashCode());
        }

        [Fact]
        public void Equality_tests()
        {
            Assert.True(
                Card.Get("AC") ==
                Card.Get("AC"));

            Assert.True(default == ((Card)default));

            Assert.True(
                Card.Get("AC") !=
                Card.Get("AD"));
        }

        [Theory]
        [InlineData("", "--")]
        [InlineData("AS", "AS")]
        public void ToString_returns_string_representation(string card, string expected)
            => Assert.Equal(expected, Card.Get(card).ToString());
    }
}
