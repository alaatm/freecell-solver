using System;
using Xunit;

namespace FreeCellSolver.Test
{
    public class TableausTests
    {
        [Fact]
        public void EmptyTableauCount_returns_empty_tableau_count()
        {
            var ts = new Tableaus(new Tableau(""), new Tableau("KC"), new Tableau("KD"), new Tableau("KH"), new Tableau("KS"), new Tableau("QC"), new Tableau("QD"), new Tableau(""));
            Assert.Equal(2, ts.EmptyTableauCount);
        }

        [Fact]
        public void CanReceive_returns_whether_card_can_be_placed_on_top_of_any_tableau()
        {
            // Excluding col 0, card can be received at col 7 because its empty.
            var ts = new Tableaus(new Tableau(""), new Tableau("KC"), new Tableau("KD"), new Tableau("KH"), new Tableau("KS"), new Tableau("QC"), new Tableau("QD"), new Tableau(""));
            Assert.True(ts.CanReceive(Card.Get("5H"), 0));

            // Excluding col 0, card can be received at col 1 below 6S
            ts = new Tableaus(new Tableau(""), new Tableau("6S"), new Tableau("KD"), new Tableau("KH"), new Tableau("KS"), new Tableau("QC"), new Tableau("QD"), new Tableau("QH"));
            Assert.True(ts.CanReceive(Card.Get("5H"), 0));

            // Excluding col 0, card cannot be placed anywhere
            ts = new Tableaus(new Tableau(""), new Tableau("6S"), new Tableau("KD"), new Tableau("KH"), new Tableau("KS"), new Tableau("QC"), new Tableau("QD"), new Tableau("QH"));
            Assert.False(ts.CanReceive(Card.Get("TH"), 0));

            // Cannot receive null card
            ts = new Tableaus(new Tableau(""), new Tableau("6S"), new Tableau("KD"), new Tableau("KH"), new Tableau("KS"), new Tableau("QC"), new Tableau("QD"), new Tableau("QH"));
            Assert.False(ts.CanReceive(null, 0));
        }

        [Fact]
        public void Clone_clones_object()
        {
            var ts = new Tableaus(new Tableau(""), new Tableau("KC"), new Tableau("KD"), new Tableau("KH"), new Tableau("KS"), new Tableau("QC"), new Tableau("QD"), new Tableau(""));
            var clone = ts.Clone();

            Assert.Equal(ts.EmptyTableauCount, clone.EmptyTableauCount);
            Assert.Null(clone[0].Top);
            Assert.Equal(Card.Get("KC"), clone[1].Top);
            Assert.Equal(Card.Get("KD"), clone[2].Top);
            Assert.Equal(Card.Get("KH"), clone[3].Top);
            Assert.Equal(Card.Get("KS"), clone[4].Top);
            Assert.Equal(Card.Get("QC"), clone[5].Top);
            Assert.Equal(Card.Get("QD"), clone[6].Top);
            Assert.Null(clone[7].Top);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal(
                $"01 02 03 04 05 06 07 08{Environment.NewLine}-- -- -- -- -- -- -- --{Environment.NewLine}   KC KD KH KS QC QD   {Environment.NewLine}      9C               ",
                new Tableaus(new Tableau(""), new Tableau("KC"), new Tableau("KD9C"), new Tableau("KH"), new Tableau("KS"), new Tableau("QC"), new Tableau("QD"), new Tableau("")).ToString());
    }
}