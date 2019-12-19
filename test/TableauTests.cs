using Xunit;

namespace FreeCellSolver.Test
{
    public class TableauTests
    {
        [Theory]
        [InlineData("", 0)]
        [InlineData("AH", 1)]
        [InlineData("AH5C", 1)]
        [InlineData("5H4S", 2)]
        [InlineData("AH5H4S", 2)]
        [InlineData("KHQSJDTC9H8S7D6C5H4S3D2SAD", 13)]
        [InlineData("2HQSJDTC9H8S7D6C5H4S3D2SAD", 12)]
        [InlineData("2HQSJDTC9H8S7D6C5H4S3D2S", 11)]
        [InlineData("JD7H8CKCQHAH", 1)]
        public void SortedSize_returns_sorted_size(string cards, int expectedSize)
        {
            var tableau = new Tableau(cards);
            Assert.Equal(expectedSize, tableau.SortedSize);
        }

        [Theory]
        [InlineData("", "", 0)]
        [InlineData("", "AH", 0)]
        [InlineData("AH", "", 1)]
        [InlineData("AH", "5S", 0)]
        [InlineData("AH", "2S", 1)]
        [InlineData("AH5H4S", "QS6C2C", 0)]
        [InlineData("AH5H4S", "QS6C5D", 1)]
        [InlineData("AH5H4S", "QS6S", 2)]
        [InlineData("2HQSJDTC9H8S7D6C5H4S3D2SAD", "KH", 12)]
        public void CountMovable_returns_num_of_cards_that_can_be_moved(string sourceCards, string destCards, int expectedMovableCount)
        {
            var sourceTableau = new Tableau(sourceCards);
            var destTableau = new Tableau(destCards);
            Assert.Equal(expectedMovableCount, sourceTableau.CountMovable(destTableau));
        }

        [Theory]
        [InlineData("AH", "", 1, true, false, 0, 1)]
        [InlineData("AH", "2S", 1, true, false, 0, 2)]
        [InlineData("AH5H4S", "QS7C5D", 1, false, false, 1, 2)]
        [InlineData("AH5H4S", "QS6S", 2, false, false, 1, 3)]
        [InlineData("2HQSJDTC9H8S7D6C5H4S3D2SAD", "KH", 12, false, false, 1, 13)]
        [InlineData("2HQSJDTC9H8S7D6C5H4S3D2SAD", "8C", 7, false, false, 5, 8)]
        public void Move_moves_cards_between_tableau(string sourceCards, string destCards, int requestedCount, bool expectedSrcEmpty, bool expectedDestEmpy, int expectedSrcSortedCount, int expectedDestSortedCount)
        {
            var sourceTableau = new Tableau(sourceCards);
            var destTableau = new Tableau(destCards);

            sourceTableau.Move(destTableau, requestedCount);

            Assert.Equal(expectedSrcEmpty, sourceTableau.IsEmpty);
            Assert.Equal(expectedDestEmpy, destTableau.IsEmpty);

            Assert.Equal(expectedSrcSortedCount, sourceTableau.SortedSize);
            Assert.Equal(expectedDestSortedCount, destTableau.SortedSize);
        }
    }
}