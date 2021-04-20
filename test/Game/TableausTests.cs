using System;
using System.Linq;
using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class TableausTests
    {
        [Fact]
        public void EmptyCount_returns_empty_tableau_count()
        {
            var ts = Tableaus.Create(
                Tableau.Create(),
                Tableau.Create("KC"),
                Tableau.Create("KD"),
                Tableau.Create("KH"),
                Tableau.Create("KS"),
                Tableau.Create("QC"),
                Tableau.Create("QD"),
                Tableau.Create());
            Assert.Equal(2, ts.EmptyCount());
        }

        [Fact]
        public void Clone_clones_object()
        {
            var ts = Tableaus.Create(
                Tableau.Create(),
                Tableau.Create("KC"),
                Tableau.Create("KD"),
                Tableau.Create("KH"),
                Tableau.Create("KS"),
                Tableau.Create("QC"),
                Tableau.Create("QD"),
                Tableau.Create());
            var clone = ts.CloneX();

            Assert.Equal(ts.EmptyCount(), clone.EmptyCount());
            Assert.Equal(Card.Null, clone[0].Top);
            Assert.Equal(Card.Get("KC"), clone[1].Top);
            Assert.Equal(Card.Get("KD"), clone[2].Top);
            Assert.Equal(Card.Get("KH"), clone[3].Top);
            Assert.Equal(Card.Get("KS"), clone[4].Top);
            Assert.Equal(Card.Get("QC"), clone[5].Top);
            Assert.Equal(Card.Get("QD"), clone[6].Top);
            Assert.Equal(Card.Null, clone[7].Top);

            Assert.NotSame(ts, clone);

            var fi = typeof(Tableau).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            for (var i = 0; i < 8; i++)
            {
                Assert.True(ts[i].Equals(clone[i]));
                Assert.NotSame(ts[i], clone[i]);
                Assert.Equal(ts[i].Top, clone[i].Top);
                Assert.NotSame(fi.GetValue(ts[i]), fi.GetValue(clone[i]));
            }
        }

        [Fact]
        public void Dump_returns_string_representation()
            => Assert.Equal(
                $"00 01 02 03 04 05 06 07{Environment.NewLine}-- -- -- -- -- -- -- --{Environment.NewLine}   KC KD KH KS QC QD   {Environment.NewLine}      9C               ",
                    Tableaus.Create(Tableau.Create(),
                    Tableau.Create("KC"),
                    Tableau.Create("KD 9C"),
                    Tableau.Create("KH"),
                    Tableau.Create("KS"),
                    Tableau.Create("QC"),
                    Tableau.Create("QD"),
                    Tableau.Create("")).Dump());

        [Fact]
        public void AllCards_returns_all_cards()
        {
            var t0 = Tableau.Create("AC");
            var t1 = Tableau.Create("AD");
            var t3 = Tableau.Create("AH AS");
            var ts = Tableaus.Create(t0, t1, Tableau.Create(), t3);
            var allCards = ts.AllCards().ToList();

            // Assert
            Assert.Equal(4, allCards.Count);
            Assert.Equal(Card.Get("AC"), allCards[0]);
            Assert.Equal(Card.Get("AD"), allCards[1]);
            Assert.Equal(Card.Get("AH"), allCards[2]);
            Assert.Equal(Card.Get("AS"), allCards[3]);

            Assert.Empty(Tableaus.Create().AllCards());
        }
    }
}