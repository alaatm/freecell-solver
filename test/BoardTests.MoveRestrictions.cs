using System.Linq;
using Xunit;
using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;

namespace FreeCellSolver.Test
{
    public class BoardTests_MoveRestrictions
    {
        [Fact]
        public void GetValidMoves_blocks_moves_that_are_guaranteed_to_be_generated_from_other_board_states_when_last_and_proposed_moves_are_TtR()
        {
            /*
             * aa bb cc dd
             * -- -- -- --       ←←←←←←←←
             *                          ↑
             * 00 01 02 03 04 05 06 07  ↑
             * -- -- -- -- -- -- -- --  ↑
             * 7S TC 5D QC 8C 2C JD 3C  ↑
             * JS 9H AS AH QH AC AD JH  ↑
             * 7H 4D KC TS 3D 6D KS 4C  ↑
             * TH JC QS 6H 5C 2D 2S 8D  ↑
             * KH 3H QD 2H 9C KD 3S 5H  ↑
             * 9D 5S 6S 4H TD 6C 7D 7C  ↑
             * 9S 4S 8S 8H  ↓           ↑
             *              →→→→→→→→→→→→→
             */

            // Arrange
            var b = Board.FromDealNum(984);
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 4, 0, 1));

            // Act
            var ttrMoves = b.GetValidMoves().Where(p => p.Type == MoveType.TableauToReserve).ToList();

            // Assert
            Assert.Equal(4, ttrMoves.Count);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 4, 1, 1), ttrMoves[0]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 5, 1, 1), ttrMoves[1]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 6, 1, 1), ttrMoves[2]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 7, 1, 1), ttrMoves[3]);
        }

        [Fact]
        public void GetValidMoves_blocks_non_meaningfull_TtT_moves_when_last_move_is_TtR()
        {
            /*
             *    CC DD HH SS
             *    5C 5D 5H 5S
             *   
             *    aa bb cc dd
             * →→ -- -- -- --
             * ↑                       
             * ↑  00 01 02 03 04 05 06 07
             * ↑  -- -- -- -- -- -- -- --
             * ↑  KS KH KD 6C 6D 6H 6S QD
             * ↑  JC JS JH JD QC QS QH TD
             * ↑  7D 7H 9H 9D TC TS TH 7C
             * ↑  8D 8H 8S 8C 9C 9S 7S
             * ←← KC
             */

            // Arrange
            var b = Board.Create(Reserve.Create(), Foundation.Create(Ranks.R5, Ranks.R5, Ranks.R5, Ranks.R5), Tableaus.Create(
                Tableau.Create("KS JC 7D 8D KC"),
                Tableau.Create("KH JS 7H 8H"),
                Tableau.Create("KD JH 9H 8S"),
                Tableau.Create("6C JD 9D 8C"),
                Tableau.Create("6D QC TC 9C"),
                Tableau.Create("6H QS TS 9S"),
                Tableau.Create("6S QH TH 7S"),
                Tableau.Create("QD TD 7C")));
            Assert.True(b.IsValid());
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0, 1), false);

            // Act
            var tttMoves = b.GetValidMoves().Where(p => p.Type == MoveType.TableauToTableau).ToList();

            // Assert
            Assert.Equal(4, tttMoves.Count);
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 0, 4, 1), tttMoves[0]);
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 0, 5, 1), tttMoves[1]);
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 6, 0, 1), tttMoves[2]);
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 7, 0, 1), tttMoves[3]);
        }

        [Fact]
        public void GetValidMoves_blocks_non_meaningfull_RtT_moves_when_last_move_is_TtT()
        {
            /*
             * CC DD HH SS
             * 4C 4D 4H 4S
             *
             * aa bb cc dd
             * 6D 9S -- --
             *                        
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * KS KH KD 6C 5C 5S 6S 5H
             * JC JS JH JD QC 6H QH 5D
             * 7D 7H 9H 9D 9C QS TH KC
             * 8D 8H 8S 8C TD TS 7S QD
             *     ↑              ↓ TC
             *     ↑              ↓ 7C    
             *     ←←←←←←←←←←←←←←←← 
             */

            // Arrange
            var b = Board.Create(Reserve.Create("6D", "9S"), Foundation.Create(Ranks.R4, Ranks.R4, Ranks.R4, Ranks.R4), Tableaus.Create(
                Tableau.Create("KS JC 7D 8D"),
                Tableau.Create("KH JS 7H 8H"),
                Tableau.Create("KD JH 9H 8S"),
                Tableau.Create("6C JD 9D 8C"),
                Tableau.Create("5C QC 9C TD"),
                Tableau.Create("5S 6H QS TS"),
                Tableau.Create("6S QH TH 7S"),
                Tableau.Create("5H 5D KC QD TC 7C")));
            Assert.True(b.IsValid());
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 6, 1, 1), false);

            // Act
            var rttMoves = b.GetValidMoves().Where(p => p.Type == MoveType.ReserveToTableau).ToList();

            // Assert
            Assert.Equal(2, rttMoves.Count);
            Assert.Equal(Move.Get(MoveType.ReserveToTableau, 0, 1, 1), rttMoves[0]);
            Assert.Equal(Move.Get(MoveType.ReserveToTableau, 1, 6, 1), rttMoves[1]);
        }
    }
}