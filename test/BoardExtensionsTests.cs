using System.IO;
using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;
using Xunit;

namespace FreeCellSolver.Test
{
    public class BoardExtensionsTests
    {
        static readonly Board _b1 = new(Tableaus.Create(
            new Tableau("JD KD 2S 4C 3S 6D 6S"),
            new Tableau("2D KC KS 5C TD 8S 9C"),
            new Tableau("9H 9S 9D TS 4S 8D 2H"),
            new Tableau("JC 5S QD QH TH QS 6H"),
            new Tableau("5D AD JS 4H 8H 6C"),
            new Tableau("7H QC AS AC 2C 3D"),
            new Tableau("7C KH AH 4D JH 8C"),
            new Tableau("5H 3H 3C 7S 7D TC")
        ));

        static readonly Board _b2 = new(Tableaus.Create(
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
        public void AsJson_returns_json_string() => Assert.Equal(
            "[[41,49,7,12,11,21,23,],[5,48,51,16,37,31,32,],[34,35,33,39,15,29,6,],[40,19,45,46,38,47,22,],[17,1,43,14,30,20,],[26,44,3,0,4,9,],[24,50,2,13,42,28,],[18,10,8,27,25,36,],];",
            Board.FromDealNum(1).AsJson());

        [Fact]
        public void EmitCSharpCode_emits_cscode()
        {
            var writer = new StringWriter();
            Board
                .FromDealNum(1)
                .ExecuteMove(Move.Get(MoveType.TableauToReserve, 5, 0))
                .ExecuteMove(Move.Get(MoveType.TableauToReserve, 5, 1))
                .EmitCSharpCode(writer);
            Assert.Equal(
@"/*
CC DD HH SS
2C -- -- AS

00 01 02 03
3D -- -- --

00 01 02 03 04 05 06 07
-- -- -- -- -- -- -- --
JD 2D 9H JC 5D 7H 7C 5H
KD KC 9S 5S AD QC KH 3H
2S KS 9D QD JS    AH 3C
4C 5C TS QH 4H    4D 7S
3S TD 4S TH 8H    JH 7D
6D 8S 8D QS 6C    8C TC
6S 9C 2H 6H            
*/

var b = new Board(
	new Reserve(""3D""),
	new Foundation(Ranks.R2, Ranks.Nil, Ranks.Nil, Ranks.Ace),
	Tableaus.Create(
		new Tableau(""JD KD 2S 4C 3S 6D 6S""),
		new Tableau(""2D KC KS 5C TD 8S 9C""),
		new Tableau(""9H 9S 9D TS 4S 8D 2H""),
		new Tableau(""JC 5S QD QH TH QS 6H""),
		new Tableau(""5D AD JS 4H 8H 6C""),
		new Tableau(""7H QC""),
		new Tableau(""7C KH AH 4D JH 8C""),
		new Tableau(""5H 3H 3C 7S 7D TC"")
	)
);
if (!b.IsValid()) { throw new Exception(); }",
            writer.ToString());
        }
    }
}
