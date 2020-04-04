using System.IO;
using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;
using Xunit;

namespace FreeCellSolver.Test
{
    public class BoardExtensionsTests
    {
        static readonly Board _b1 = new Board(new Tableaus(
            new Tableau("JD KD 2S 4C 3S 6D 6S"),
            new Tableau("2D KC KS 5C TD 8S 9C"),
            new Tableau("9H 9S 9D TS 4S 8D 2H"),
            new Tableau("JC 5S QD QH TH QS 6H"),
            new Tableau("5D AD JS 4H 8H 6C"),
            new Tableau("7H QC AS AC 2C 3D"),
            new Tableau("7C KH AH 4D JH 8C"),
            new Tableau("5H 3H 3C 7S 7D TC")
        ));

        static readonly Board _b2 = new Board(new Tableaus(
            new Tableau("QD 4D TD 7S AH 3H AS"),
            new Tableau("QC JD JC 9D 9S AD 5S"),
            new Tableau("KC JS 8C KS TC 7H TH"),
            new Tableau("3C 6H 6C 7C 2S 3D JH"),
            new Tableau("4C QS 8S 6S 3S 5H"),
            new Tableau("2C 6D 4S 4H TS 8D"),
            new Tableau("KD 2D 5D AC 9H KH"),
            new Tableau("5C 9C QH 8H 2H 7D")
        ));

        [Fact]
        public void FromDealNum_returns_correct_result()
        {
            Assert.True(Board.FromDealNum(1) == _b1);
            Assert.True(Board.FromDealNum(2) == _b2);
        }

        [Fact]
        public void EmitCSharpCode_emits_cscode()
        {
            var writer = new StringWriter();
            Board.FromDealNum(1).EmitCSharpCode(writer);
            Assert.Equal(
@"/*
CC DD HH SS
-- -- -- --

01 02 03 04
-- -- -- --

01 02 03 04 05 06 07 08
-- -- -- -- -- -- -- --
JD 2D 9H JC 5D 7H 7C 5H
KD KC 9S 5S AD QC KH 3H
2S KS 9D QD JS AS AH 3C
4C 5C TS QH 4H AC 4D 7S
3S TD 4S TH 8H 2C JH 7D
6D 8S 8D QS 6C 3D 8C TC
6S 9C 2H 6H            
*/

var b = new Board(
	new Reserve(),
	new Foundation(),
	new Tableaus(
		new Tableau(""JD KD 2S 4C 3S 6D 6S""),
		new Tableau(""2D KC KS 5C TD 8S 9C""),
		new Tableau(""9H 9S 9D TS 4S 8D 2H""),
		new Tableau(""JC 5S QD QH TH QS 6H""),
		new Tableau(""5D AD JS 4H 8H 6C""),
		new Tableau(""7H QC AS AC 2C 3D""),
		new Tableau(""7C KH AH 4D JH 8C""),
		new Tableau(""5H 3H 3C 7S 7D TC"")
	)
);
if (!b.IsValid()) { throw new Exception(); }",
            writer.ToString());
        }
    }
}
