using System;
using System.Linq;
using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class TableausTests
    {
        [Fact]
        public void EmptyTableauCount_returns_empty_tableau_count()
        {
            var ts = new Tableaus(
                new Tableau(),
                new Tableau("KC"),
                new Tableau("KD"),
                new Tableau("KH"),
                new Tableau("KS"),
                new Tableau("QC"),
                new Tableau("QD"),
                new Tableau());
            Assert.Equal(2, ts.EmptyTableauCount);
        }

        [Fact]
        public void Clone_clones_object()
        {
            var ts = new Tableaus(
                new Tableau(),
                new Tableau("KC"),
                new Tableau("KD"),
                new Tableau("KH"),
                new Tableau("KS"),
                new Tableau("QC"),
                new Tableau("QD"),
                new Tableau());
            var clone = ts.Clone();

            Assert.Equal(ts.EmptyTableauCount, clone.EmptyTableauCount);
            Assert.Equal(Card.Null, clone[0].Top);
            Assert.Equal(Card.Get("KC"), clone[1].Top);
            Assert.Equal(Card.Get("KD"), clone[2].Top);
            Assert.Equal(Card.Get("KH"), clone[3].Top);
            Assert.Equal(Card.Get("KS"), clone[4].Top);
            Assert.Equal(Card.Get("QC"), clone[5].Top);
            Assert.Equal(Card.Get("QD"), clone[6].Top);
            Assert.Equal(Card.Null, clone[7].Top);
        }

        [Fact]
        public void ToString_returns_string_representation()
            => Assert.Equal(
                $"01 02 03 04 05 06 07 08{Environment.NewLine}-- -- -- -- -- -- -- --{Environment.NewLine}   KC KD KH KS QC QD   {Environment.NewLine}      9C               ",
                    new Tableaus(new Tableau(),
                    new Tableau("KC"),
                    new Tableau("KD 9C"),
                    new Tableau("KH"),
                    new Tableau("KS"),
                    new Tableau("QC"),
                    new Tableau("QD"),
                    new Tableau("")).ToString());

        [Fact]
        public void AllCards_returns_all_cards()
        {
            var t0 = new Tableau("AC");
            var t1 = new Tableau("AD");
            var t3 = new Tableau("AH AS");
            var ts = new Tableaus(t0, t1, new Tableau(), t3);
            var allCards = ts.AllCards().ToList();

            // Assert
            Assert.Equal(4, allCards.Count);
            Assert.Equal(Card.Get("AC"), allCards[0]);
            Assert.Equal(Card.Get("AD"), allCards[1]);
            Assert.Equal(Card.Get("AH"), allCards[2]);
            Assert.Equal(Card.Get("AS"), allCards[3]);

            Assert.Empty(new Tableaus().AllCards());
        }
    }
}