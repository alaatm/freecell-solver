using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using FreeCellSolver.Game;

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
                Assert.Equal(Ranks.Ace, f[i]);
            }
        }

        [Fact]
        public void Indexer_returns_next_rank_of_specified_suit()
        {
            var f = new Foundation(Ranks.R6, Ranks.Nil, Ranks.Nil, Ranks.Nil);
            Assert.Equal(Ranks.R7, f[Suits.Clubs]);
            Assert.Equal(Ranks.Ace, f[Suits.Diamonds]);
            Assert.Equal(Ranks.Ace, f[Suits.Hearts]);
            Assert.Equal(Ranks.Ace, f[Suits.Spades]);
        }

        [Fact]
        public void CanPush_returns_whether_card_can_be_pushed()
        {
            var f = new Foundation();

            Assert.True(f.CanPush(Card.Get("AC")));
            Assert.False(f.CanPush(Card.Get("2C")));
        }

        [Theory]
        [MemberData(nameof(CanAutoPlay_testsData))]
        public void CanAutoPlay_tests(int clubs, int diamonds, int hearts, int spades, Card card, bool expectedCanAutoPlay)
        {
            var f = new Foundation(clubs, diamonds, hearts, spades);
            Assert.Equal(expectedCanAutoPlay, f.CanAutoPlay(card));
        }

        public static IEnumerable<object[]> CanAutoPlay_testsData()
        {
            // false when card cannot be pushed
            yield return new object[] { Ranks.Nil, Ranks.Nil, Ranks.Nil, Ranks.Nil, Card.Get("KC"), false };
            // true when card can be pushed and is below RANK 3
            yield return new object[] { Ranks.Nil, Ranks.Nil, Ranks.Nil, Ranks.Nil, Card.Get("AC"), true };
            yield return new object[] { Ranks.Ace, Ranks.Nil, Ranks.Nil, Ranks.Nil, Card.Get("2C"), true };
            // true when card can be pushed, above or equal RANK 3 and ALL oposite color cards of lower rank are at foundation
            yield return new object[] { Ranks.R2, Ranks.R2, Ranks.R2, Ranks.Nil, Card.Get("3C"), true };  // Alow 3C -> 2D and 2H are at foundation
            yield return new object[] { Ranks.R2, Ranks.R2, Ranks.Nil, Ranks.R2, Card.Get("3D"), true };  // Alow 3D -> 2C and 2S are at foundation
            // false when card can be pushed, above or equal RANK 3 and ANY oposite color cards of lower rank are NOT at foundation
            yield return new object[] { Ranks.R2, Ranks.Ace, Ranks.R2, Ranks.Nil, Card.Get("3C"), false }; // Deny 3C -> 2D not at foundation
            yield return new object[] { Ranks.R2, Ranks.R2, Ranks.Ace, Ranks.Nil, Card.Get("3C"), false }; // Deny 3C -> 2H not at foundation
            yield return new object[] { Ranks.R2, Ranks.Ace, Ranks.Ace, Ranks.Nil, Card.Get("3C"), false }; // Deny 3C -> Neither 2D or 2H are at foundation
            yield return new object[] { Ranks.Ace, Ranks.R2, Ranks.Nil, Ranks.R2, Card.Get("3D"), false }; // Deny 3D -> 2C not at foundation
            yield return new object[] { Ranks.R2, Ranks.R2, Ranks.Nil, Ranks.Ace, Card.Get("3D"), false }; // Deny 3D -> 2S not at foundation
            yield return new object[] { Ranks.Ace, Ranks.R2, Ranks.Nil, Ranks.Ace, Card.Get("3D"), false }; // Deny 3D -> Neither 2C or 2S are at foundation
        }

        [Fact]
        public void Push_pushes_card_to_foundation_slot()
        {
            var f = new Foundation(Ranks.Nil, Ranks.Nil, Ranks.Nil, Ranks.Nil);

            Assert.Equal(Ranks.Ace, f[Suits.Clubs]);
            f.Push(Card.Get("AC"));
            Assert.Equal(Ranks.R2, f[Suits.Clubs]);
        }

        [Fact]
        public void Clone_clones_object()
        {
            var f = new Foundation(Ranks.Ace, Ranks.R4, Ranks.Nil, Ranks.R3);
            var clone = f.Clone();

            Assert.Equal(Ranks.R2, clone[Suits.Clubs]);
            Assert.Equal(Ranks.R5, clone[Suits.Diamonds]);
            Assert.Equal(Ranks.Ace, clone[Suits.Hearts]);
            Assert.Equal(Ranks.R4, clone[Suits.Spades]);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal($"CC DD HH SS{Environment.NewLine}AC 4D -- KS", new Foundation(Ranks.Ace, Ranks.R4, Ranks.Nil, Ranks.Rk).ToString());

        [Fact]
        public void AllCards_returns_all_cards()
        {
            var f = new Foundation(Ranks.Nil, Ranks.Ace, Ranks.R2, Ranks.R3);
            var allCards = f.AllCards().ToList();

            // Assert
            Assert.Equal(6, allCards.Count);
            Assert.Equal(Card.Get("AD"), allCards[0]);
            Assert.Equal(Card.Get("AH"), allCards[1]);
            Assert.Equal(Card.Get("2H"), allCards[2]);
            Assert.Equal(Card.Get("AS"), allCards[3]);
            Assert.Equal(Card.Get("2S"), allCards[4]);
            Assert.Equal(Card.Get("3S"), allCards[5]);

            Assert.Empty(new Foundation().AllCards());
        }
    }
}
