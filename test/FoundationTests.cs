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
        public void CanPush_returns_whether_card_can_be_pushed()
        {
            var f = new Foundation();

            Assert.True(f.CanPush(Card.Get("AC")));
            Assert.False(f.CanPush(Card.Get("2C")));
        }

        [Theory]
        [MemberData(nameof(CanAutoPlay_testsData))]
        public void CanAutoPlay_tests(short clubs, short diamonds, short hearts, short spades, Card card, bool expectedCanAutoPlay)
        {
            var f = new Foundation(clubs, diamonds, hearts, spades);
            Assert.Equal(expectedCanAutoPlay, f.CanAutoPlay(card));
        }

        public static IEnumerable<object[]> CanAutoPlay_testsData()
        {
            // false when card cannot be pushed
            yield return new object[] { -1, -1, -1, -1, Card.Get("KC"), false };
            // true when card can be pushed and is below RANK 3
            yield return new object[] { -1, -1, -1, -1, Card.Get("AC"), true };
            yield return new object[] { 0, -1, -1, -1, Card.Get("2C"), true };
            // true when card can be pushed, above or equal RANK 3 and ALL oposite color cards of lower rank are at foundation
            yield return new object[] { 1, 1, 1, -1, Card.Get("3C"), true };  // Alow 3C -> 2D and 2H are at foundation
            yield return new object[] { 1, 1, -1, 1, Card.Get("3D"), true };  // Alow 3D -> 2C and 2S are at foundation
            // false when card can be pushed, above or equal RANK 3 and ANY oposite color cards of lower rank are NOT at foundation
            yield return new object[] { 1, 0, 1, -1, Card.Get("3C"), false }; // Deny 3C -> 2D not at foundation
            yield return new object[] { 1, 1, 0, -1, Card.Get("3C"), false }; // Deny 3C -> 2H not at foundation
            yield return new object[] { 1, 0, 0, -1, Card.Get("3C"), false }; // Deny 3C -> Neither 2D or 2H are at foundation
            yield return new object[] { 0, 1, -1, 1, Card.Get("3D"), false }; // Deny 3D -> 2C not at foundation
            yield return new object[] { 1, 1, -1, 0, Card.Get("3D"), false }; // Deny 3D -> 2S not at foundation
            yield return new object[] { 0, 1, -1, 0, Card.Get("3D"), false }; // Deny 3D -> Neither 2C or 2S are at foundation
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

        [Fact]
        public void AllCards_returns_all_cards()
        {
            var f = new Foundation(-1, 0, 1, 2);
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
