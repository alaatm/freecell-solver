using Xunit;

namespace FreeCellSolver.Test
{
    public class BoardTests
    {
        [Fact]
        public void Test()
        {
            var t0 = new Tableau("TD5C4H3S");
            var b = new Board(new Tableaus(t0, new Tableau(), new Tableau(), new Tableau(), new Tableau(), new Tableau(), new Tableau(), new Tableau()));

            var moves = b.GetValidMoves(out _, out _);
        }
    }
}