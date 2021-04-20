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
            var moves = b.GetValidMoves().ToArray();
            var ttrMoves = moves.Where(p => p.Type == MoveType.TableauToReserve).ToList();

            // Assert
            Assert.Equal(4, ttrMoves.Count);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 4, 1, 1), ttrMoves[0]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 5, 1, 1), ttrMoves[1]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 6, 1, 1), ttrMoves[2]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 7, 1, 1), ttrMoves[3]);
        }

        [Fact]
        public void GetValidMoves_doesnt_block_TtR_moves_if_last_move_was_not_TtR()
        {
            /*
             * aa bb cc dd
             * -- -- -- --       
             *                          
             * 00 01 02 03 04 05 06 07  
             * -- -- -- -- -- -- -- --  
             * 7S TC 5D QC 8C 2C JD 3C  
             * JS 9H AS AH QH AC AD JH  
             * 7H 4D KC TS 3D 6D KS 4C  
             * TH JC QS 6H 5C 2D 2S 8D  
             * KH 3H QD 2H 9C KD 3S 5H  
             * 9D 5S 6S 4H TD 6C 7D 7C  
             * 9S 4S 8S 8H             
             *              
             */

            // Arrange
            var b = Board.FromDealNum(984);
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 0, 4, 1));

            // Act
            var moves = b.GetValidMoves().ToArray();
            var ttrMoves = moves.Where(p => p.Type == MoveType.TableauToReserve).ToList();

            // Assert
            Assert.Equal(8, ttrMoves.Count);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 0, 0, 1), ttrMoves[0]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 1, 0, 1), ttrMoves[1]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 2, 0, 1), ttrMoves[2]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 3, 0, 1), ttrMoves[3]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 4, 0, 1), ttrMoves[4]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 5, 0, 1), ttrMoves[5]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 6, 0, 1), ttrMoves[6]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 7, 0, 1), ttrMoves[7]);
        }

        [Fact]
        public void GetValidMoves_blocks_non_meaningfull_RtT_moves_when_last_move_is_TtT()
        {
            /*
             * HH CC DD SS
             * 4H 4C 4D 4S
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
            var moves = b.GetValidMoves().ToArray();
            var rttMoves = moves.Where(p => p.Type == MoveType.ReserveToTableau).ToList();

            // Assert
            Assert.Equal(2, rttMoves.Count);
            Assert.Equal(Move.Get(MoveType.ReserveToTableau, 0, 1, 1), rttMoves[0]);
            Assert.Equal(Move.Get(MoveType.ReserveToTableau, 1, 6, 1), rttMoves[1]);
        }

        [Fact]
        public void GetValidMoves_blocks_reverse_moves_TtT_regardless_when_they_took_place_in_move_history()
        {
            /*
             * HH CC DD SS
             * 2H 4C 6D 8S
             *
             * aa bb cc dd
             * KS 6H KD JD
             *                                    
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * 9S    QS JS    JH 5C
             * QD    QC 8C    9D 6C
             * 7D    TS 3H    9H QH
             * TC    7C 7H    8H 8D
             * JC    KH KC    4H 5H
             * TH                TD
             * 9C                  
             */

            // Arrange
            var r = Reserve.Create("KS", "6H", "KD", "JD");
            var f = Foundation.Create(Ranks.R2, Ranks.R4, Ranks.R6, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC JC TH 9C");
            var t2 = Tableau.Create("QS QC TS 7C KH");
            var t3 = Tableau.Create("JS 8C 3H 7H KC");
            var t5 = Tableau.Create("JH 9D 9H 8H 4H");
            var t6 = Tableau.Create("5C 6C QH 8D 5H TD");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, tRest, t5, t6, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 0, 6));
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 2, 1));

            // Act
            var moves = b.GetValidMoves().ToArray();
            var tttMoves = moves.Where(p => p.Type == MoveType.TableauToTableau).ToList();

            // Assert
            Assert.Empty(tttMoves.Where(m => m.From == 6 && m.To == 0));
        }

        [Fact]
        public void GetValidMoves_unblocks_reverse_moves_TtT_when_original_tableaus_had_any_manual_moves()
        {
            /*
             * HH CC DD SS
             * 2H 4C 6D 8S
             *
             * aa bb cc dd
             * KS 6H KD JD
             *                                    
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * 9S    QS JS    JH 5C
             * QD    QC 8C    9D 6C
             * 7D    TS 3H    9H QH
             * TC    7C 7H    8H 8D
             * JC    KH KC    4H 5H
             * TH                TD
             * 9C                  
             */

            // Arrange
            var r = Reserve.Create("KS", "6H", "KD", "JD");
            var f = Foundation.Create(Ranks.R2, Ranks.R4, Ranks.R6, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC JC TH 9C");
            var t2 = Tableau.Create("QS QC TS 7C KH");
            var t3 = Tableau.Create("JS 8C 3H 7H KC");
            var t5 = Tableau.Create("JH 9D 9H 8H 4H");
            var t6 = Tableau.Create("5C 6C QH 8D 5H TD");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, tRest, t5, t6, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 0, 6));
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 0, 1));
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 6, 7));

            // Act
            var moves = b.GetValidMoves().ToArray();
            var tttMoves = moves.Where(p => p.Type == MoveType.TableauToTableau).ToList();

            // Assert
            Assert.Single(tttMoves.Where(m => m.From == 6 && m.To == 0));
        }

        [Fact]
        public void GetValidMoves_doesnt_block_TtT_move_when_column_sortSize_changes_after_move()
        {
            /*
             * HH CC DD SS
             * 6H 6C AD 4S
             * 
             * 00 01 02 03
             * QH 5D KH KD
             * 
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * KC 2D QC QD 6S 8H TH JD
             * 7D JH    JS QS 8S 9C TS
             * TC             KS 7C 9H
             * 9D             6D 3D
             * 8C             5S 7S
             * 7H             4D JC
             *                   TD
             *                   9S
             *                   8D
            */

            var b = Board.Create(
                    Reserve.Create("QH", "5D", "KH", "KD"),
                    Foundation.Create(Ranks.R6, Ranks.R6, Ranks.Ace, Ranks.R4),
                    Tableaus.Create(
                            Tableau.Create("KC 7D TC 9D 8C 7H"),
                            Tableau.Create("2D JH"),
                            Tableau.Create("QC"),
                            Tableau.Create("QD JS"),
                            Tableau.Create("6S QS"),
                            Tableau.Create("8H 8S KS 6D 5S 4D"),
                            Tableau.Create("TH 9C 7C 3D 7S JC TD 9S 8D"),
                            Tableau.Create("JD TS 9H")
                    )
            );
            Assert.True(b.IsValid());
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 1, 2));

            // Act
            var moves = b.GetValidMoves().ToArray();
            var tttMoves = moves.Where(p => p.Type == MoveType.TableauToTableau).ToList();

            // Assert
            Assert.Single(tttMoves.Where(m => m.From == 2 && m.To == 1 && m.Size == 1));
        }
    }
}