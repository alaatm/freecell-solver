using System.Linq;
using Xunit;
using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;

namespace FreeCellSolver.Test
{
    public class BoardTests
    {
        [Fact]
        public void IsSolved_returns_whether_board_is_solved()
        {
            Assert.True(Board.Create(Reserve.Create(), Foundation.Create(Ranks.Rk, Ranks.Rk, Ranks.Rk, Ranks.Rk), Tableaus.Create()).IsSolved);
            Assert.False(Board.FromDealNum(1).IsSolved);
        }

        [Fact]
        public void GetValidMoves_skips_similar_moves_from_reserve_to_multiple_empty_tableaus()
        {
            /*
             * CC DD HH SS
             * 4C 6D 2H 8S
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
             * JC    KH KC    4H TD   
             * TH                5H     
             * 9C                     
             */

            // Arrange
            var r = Reserve.Create("KS", "6H", "KD", "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC JC TH 9C");
            var t2 = Tableau.Create("QS QC TS 7C KH");
            var t3 = Tableau.Create("JS 8C 3H 7H KC");
            var t5 = Tableau.Create("JH 9D 9H 8H 4H");
            var t6 = Tableau.Create("5C 6C QH 8D TD 5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, tRest, t5, t6, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(4, moves.Where(m => m.Type == MoveType.ReserveToTableau).Count());
            Assert.Equal(4, moves.Where(m => m.Type == MoveType.ReserveToTableau && m.To == 1).Count());
        }

        [Fact]
        public void GetValidMoves_skips_similar_moves_from_tableau_to_multiple_empty_tableaus()
        {
            /*
             * CC DD HH SS
             * 4C 6D 2H 8S
             *
             * aa bb cc dd
             * KS 6H KD JD
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * 9S    QS JS    JH 5C   
             * TC    QC 8C    9D 6C   
             * KC    TS 3H    9H QH   
             * QD    7C 7H    8H 8D   
             * JC    KH 7D    4H TD   
             * TH                5H     
             * 9C                     
             * 
             * Max allowed: 4
             * Possible to move 5 from 0 to 1 but should only move 4, 3, 2, 1 stack(s)
             * Valid moves: 0->1(4), 2->1(1), 3->1(1), 5->1(1), 6->1(1)
             */

            // Arrange
            var r = Reserve.Create("KS", "6H", "KD", "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S TC KC QD JC TH 9C");
            var t2 = Tableau.Create("QS QC TS 7C KH");
            var t3 = Tableau.Create("JS 8C 3H 7H 7D");
            var t5 = Tableau.Create("JH 9D 9H 8H 4H");
            var t6 = Tableau.Create("5C 6C QH 8D TD 5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, tRest, t5, t6, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(5, moves.Where(m => m.Type == MoveType.TableauToTableau).Count());
            Assert.All(moves.Where(m => m.Type == MoveType.TableauToTableau), m => Assert.True(m.To == 1));
        }

        [Fact]
        public void GetValidMoves_skips_partial_moves_of_sorted_tableau_to_empty_one()
        {
            /*
             * CC DD HH SS
             * 4C 6D 2H 8S
             *
             * aa bb cc dd
             * KS 6H KD --
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * JC    QS JS 9S JH 5C   
             * TH    QC 8C TC 9D 6C   
             * 9C    TS 3H KC 9H QH   
             *       7C 7H JD 8H 8D   
             *       KH 7D    4H TD   
             *                   QD   
             *                   5H   
             * 
             * Max allowed: 4
             * Valid moves: 2->1(1), 3->1(1), 4->1(1), 5->1(1), 6->1(1)
             */

            // Arrange
            var r = Reserve.Create("KS", "6H", "KD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("JC TH 9C");
            var t2 = Tableau.Create("QS QC TS 7C KH");
            var t3 = Tableau.Create("JS 8C 3H 7H 7D");
            var t4 = Tableau.Create("9S TC KC JD");
            var t5 = Tableau.Create("JH 9D 9H 8H 4H");
            var t6 = Tableau.Create("5C 6C QH 8D TD QD 5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, t4, t5, t6, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(5, moves.Where(m => m.Type == MoveType.TableauToTableau).Count());
            Assert.Empty(moves.Where(m => m.Type == MoveType.TableauToTableau && m.From == 0 && m.To == 1));
            Assert.All(moves.Where(m => m.Type == MoveType.TableauToTableau), m => Assert.True(m.To == 1));
        }

        [Fact]
        public void GetValidMoves_skips_moves_that_are_reverse_of_last_move_tr_rt()
        {
            /*
             * CC DD HH SS                        CC DD HH SS
             * 4C 6D 2H 8S                        4C 6D 2H 8S
             *
             * aa bb cc dd                        aa bb cc dd
             * -- 6H KD JD                        9C 6H KD JD
             *                                    
             * 00 01 02 03 04 05 06 07    ===\    00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --    ===/    -- -- -- -- -- -- -- --
             * 9S    QS JS    JH 5C               9S    QS JS    JH 5C   
             * QD    QC 8C    9D 6C               QD    QC 8C    9D 6C   
             * 7D    TS 3H    9H QH               7D    TS 3H    9H QH   
             * TC    7C 7H    8H 8D               TC    7C 7H    8H 8D   
             * JC    KH KC    4H TD               JC    KH KC    4H TD   
             * TH    KS          5H               TH    KS          5H   
             * 9C                                                        
             */

            // Arrange
            var r = Reserve.Create(null, "6H", "KD", "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC JC TH 9C");
            var t2 = Tableau.Create("QS QC TS 7C KH KS");
            var t3 = Tableau.Create("JS 8C 3H 7H KC");
            var t5 = Tableau.Create("JH 9D 9H 8H 4H");
            var t6 = Tableau.Create("5C 6C QH 8D TD 5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, tRest, t5, t6, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0));

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Empty(moves.Where(m => m.Type == MoveType.ReserveToTableau && m.From == 0 && m.To == 0));
        }

        [Fact]
        public void GetValidMoves_skips_moves_that_are_reverse_of_last_move_tt_tt()
        {
            /*
             * CC DD HH SS                        CC DD HH SS
             * 4C 6D 2H 8S                        4C 6D 2H 8S
             *
             * aa bb cc dd                        aa bb cc dd
             * KS 6H KD JD                        KS 6H KD JD
             *                                    
             * 00 01 02 03 04 05 06 07    ===\    00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --    ===/    -- -- -- -- -- -- -- --
             * 9S    QS JS    JH 5C               9S    QS JS    JH 5C   
             * QD    QC 8C    9D 6C               QD    QC 8C    9D 6C   
             * 7D    TS 3H    9H QH               7D    TS 3H    9H QH   
             * TC    7C 7H    8H 8D               TC    7C 7H    8H 8D   
             * JC    KH KC    4H 5H               JC    KH KC    4H 5H   
             * TH                TD               TH                TD   
             * 9C                                                   9C     
             */

            // Arrange
            var r = Reserve.Create("KS", "6H", "KD", "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
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

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Empty(moves.Where(m => m.Type == MoveType.TableauToTableau && m.From == 6 && m.To == 0));
        }

        [Fact]
        public void GetValidMoves_skips_moves_which_are_whole_columns_to_empty_one()
        {
            /*
             * CC DD HH SS
             * 4C 6D 2H 8S
             *
             * aa bb cc dd
             * -- 6H -- JD
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * 9S    KH JS    9C 5C   
             * QD    QC 8C    9D 6C   
             * 7D    JH 3H    9H QH   
             * TC    TS 7H    8H 8D   
             * JC       KC    4H TD   
             * TH       7C    KS 5H     
             * QS             KD      
             */
            var r = Reserve.Create(null, "6H", null, "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC JC TH QS");
            var t2 = Tableau.Create("KH QC JH TS");
            var t3 = Tableau.Create("JS 8C 3H 7H KC 7C");
            var t5 = Tableau.Create("9C 9D 9H 8H 4H KS KD");
            var t6 = Tableau.Create("5C 6C QH 8D TD 5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, tRest, t5, t6, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Empty(moves.Where(m => m.Type == MoveType.TableauToTableau && m.From == 2 && m.Size == 4));
        }

        [Fact]
        public void GetValidMoves_skips_super_moves_when_not_enough_space_to_carry_move()
        {
            /*
             * CC DD HH SS
             * 4C 6D 2H 8S
             *
             * aa bb cc dd
             * QS 6H TH JD
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * 9S JC KH JS    9C 5C 5H
             * QD    QC 8C    9D 6C   
             * 7D    JH 3H    9H QH   
             * TC    TS 7H    8H 8D   
             *          KC    4H TD   
             *          7C    KS      
             *       ^        KD      
             *       |        ^
             *       |        | 
             *       ----------
             *        QC,JH,TS - Can't move since available is 2 and move size is 3
             */
            var r = Reserve.Create("QS", "6H", "TH", "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC");
            var t1 = Tableau.Create("JC");
            var t2 = Tableau.Create("KH QC JH TS");
            var t3 = Tableau.Create("JS 8C 3H 7H KC 7C");
            var t5 = Tableau.Create("9C 9D 9H 8H 4H KS KD");
            var t6 = Tableau.Create("5C 6C QH 8D TD");
            var t7 = Tableau.Create("5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, t1, t2, t3, tRest, t5, t6, t7);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Empty(moves.Where(m => m.Type == MoveType.TableauToTableau && m.From == 2 && m.To == 5 && m.Size == 3));
        }

        [Fact]
        public void GetValidMoves_calculates_maxMoveSize_correctly()
        {
            /*
             * CC DD HH SS
             * 4C 6D 2H 8S
             *
             * aa bb cc dd
             * QS 6H TH JD
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * 9S JC KD JS KS 9C 5C 5H
             * QD    KH 8C    9D 6C   
             * 7D    JH 3H    9H QH   
             * TC    TS 7H    8H 8D   
             *          KC    4H TD   
             *          7C    QC      
             *       ^        ^      
             *       |        |
             *       |        | 
             *       ----------
             *        JH,TS - Can't move since available is 1 and move size is 2
             */
            var r = Reserve.Create("QS", "6H", "TH", "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC");
            var t1 = Tableau.Create("JC");
            var t2 = Tableau.Create("KD KH JH TS");
            var t3 = Tableau.Create("JS 8C 3H 7H KC 7C");
            var t4 = Tableau.Create("KS");
            var t5 = Tableau.Create("9C 9D 9H 8H 4H QC");
            var t6 = Tableau.Create("5C 6C QH 8D TD");
            var t7 = Tableau.Create("5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, t1, t2, t3, t4, t5, t6, t7);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Empty(moves.Where(m => m.Type == MoveType.TableauToTableau && m.From == 2 && m.To == 5 && m.Size == 2));
        }

        [Fact]
        public void GetValidMoves_includes_super_moves_when_enough_is_available_to_carry_move()
        {
            /*
             * CC DD HH SS
             * 4C 6D 2H 8S
             *
             * aa bb cc dd
             * QS 6H TH JD
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * 9S    KH JS    9C 5C 5H
             * QD    QC 8C    KS 6C   
             * 7D    JH 3H    9H QH   
             * TC    TS 7H    8H 8D   
             * JC    9D KC    4H TD   
             *          7C    KD      
             *       ^                
             *       |        ^
             *       |        | 
             *       ----------
             *       QC,JH,TS,9D - Can move since available is (0+1)<<2=4 and move size is 4
             */
            var r = Reserve.Create("QS", "6H", "TH", "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC JC");
            var t2 = Tableau.Create("KH QC JH TS 9D");
            var t3 = Tableau.Create("JS 8C 3H 7H KC 7C");
            var t5 = Tableau.Create("9C KS 9H 8H 4H KD");
            var t6 = Tableau.Create("5C 6C QH 8D TD");
            var t7 = Tableau.Create("5H");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, t2, t3, tRest, t5, t6, t7);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Single(moves.Where(m => m.Type == MoveType.TableauToTableau && m.From == 2 && m.To == 5 && m.Size == 4));
        }

        [Fact]
        public void GetValidMoves_returns_moves_in_correct_order()
        {
            /* 
             * CC DD HH SS
             * QC KD -- JS
             *
             * aa bb cc dd
             * KC -- -- --
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * KS QS    AH            
             *          2H            
             *          3H            
             *          4H            
             *          5H            
             *          6H            
             *          7H            
             *          8H            
             *          9H            
             *          TH            
             *          JH            
             *          QH            
             *          KH            
             */
            var r = Reserve.Create("KC");
            var f = Foundation.Create(Ranks.Rq, Ranks.Rk, Ranks.Nil, Ranks.Rj);
            var t0 = Tableau.Create("KS");
            var t1 = Tableau.Create("QS");
            var t3 = Tableau.Create("AH 2H 3H 4H 5H 6H 7H 8H 9H TH JH QH KH");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, t1, tRest, t3, tRest, tRest, tRest, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(MoveType.ReserveToFoundation, moves[0].Type);
            Assert.Equal(MoveType.TableauToFoundation, moves[1].Type);
            Assert.Equal(MoveType.ReserveToTableau, moves[2].Type);
            Assert.Equal(MoveType.TableauToTableau, moves[3].Type);
            Assert.Equal(MoveType.TableauToTableau, moves[4].Type);
            Assert.Equal(MoveType.TableauToReserve, moves[5].Type);
            Assert.Equal(MoveType.TableauToReserve, moves[6].Type);
            Assert.Equal(MoveType.TableauToReserve, moves[7].Type);
        }

        [Fact]
        public void GetValidMoves_returns_all_valid_rf_moves()
        {
            /* 
             * CC DD HH SS
             * 3C 3D -- --
             *
             * aa bb cc dd
             * 4C 4D -- --              <-- should pick these 2
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * QD QC KC 2S 5H JH KD 5C
             * 3H JD JS 6H QS 6D 8D 9C
             * TD JC 8C 6C 8S 4S 5D QH
             * 7S 9D KS AS 6S 4H KH 8H
             * AH 9S TC 7C 3S TS 9H 2H
             * 5S    7H             7D
             *       TH               
             */

            // Arrange
            var r = Reserve.Create("4C", "4D");
            var f = Foundation.Create(Ranks.R3, Ranks.R3, Ranks.Nil, Ranks.Nil);
            var t0 = Tableau.Create("QD 3H TD 7S AH 5S");
            var t1 = Tableau.Create("QC JD JC 9D 9S");
            var t2 = Tableau.Create("KC JS 8C KS TC 7H TH");
            var t3 = Tableau.Create("2S 6H 6C AS 7C");
            var t4 = Tableau.Create("5H QS 8S 6S 3S");
            var t5 = Tableau.Create("JH 6D 4S 4H TS");
            var t6 = Tableau.Create("KD 8D 5D KH 9H");
            var t7 = Tableau.Create("5C 9C QH 8H 2H 7D");
            var ts = Tableaus.Create(t0, t1, t2, t3, t4, t5, t6, t7);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(2, moves.Where(m => m.Type == MoveType.ReserveToFoundation).Count());
            Assert.Equal(Move.Get(MoveType.ReserveToFoundation, 0), moves[0]);
            Assert.Equal(Move.Get(MoveType.ReserveToFoundation, 1), moves[1]);
        }

        [Fact]
        public void GetValidMoves_returns_all_valid_tf_moves()
        {
            /* 
             * CC DD HH SS
             * 3C 3D -- --
             *
             * aa bb cc dd
             * -- -- -- --
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * QD QC KC 2S 5H JH KD 5C
             * 3H JD JS 6H QS 6D 8D 9C
             * TD JC 8C 6C 8S 4S 5D QH
             * 7S 9D KS AS 6S 4H KH 8H
             * AH 9S TC 7C 3S TS 9H 2H
             * 5S    7H    4C    4D 7D
             *       TH               
             *             ^     ^
             *             |     |
             *       should pick these 2
             */

            // Arrange
            var r = Reserve.Create();
            var f = Foundation.Create(Ranks.R3, Ranks.R3, Ranks.Nil, Ranks.Nil);
            var t0 = Tableau.Create("QD 3H TD 7S AH 5S");
            var t1 = Tableau.Create("QC JD JC 9D 9S");
            var t2 = Tableau.Create("KC JS 8C KS TC 7H TH");
            var t3 = Tableau.Create("2S 6H 6C AS 7C");
            var t4 = Tableau.Create("5H QS 8S 6S 3S 4C");
            var t5 = Tableau.Create("JH 6D 4S 4H TS");
            var t6 = Tableau.Create("KD 8D 5D KH 9H 4D");
            var t7 = Tableau.Create("5C 9C QH 8H 2H 7D");
            var ts = Tableaus.Create(t0, t1, t2, t3, t4, t5, t6, t7);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(2, moves.Where(m => m.Type == MoveType.TableauToFoundation).Count());
            Assert.Equal(Move.Get(MoveType.TableauToFoundation, 4), moves[0]);
            Assert.Equal(Move.Get(MoveType.TableauToFoundation, 6), moves[1]);
        }

        [Fact]
        public void GetValidMoves_returns_all_valid_rt_moves()
        {
            /* 
             * CC DD HH SS
             * 3C 3D -- --
             *
             * aa bb cc dd
             * 4C 4D -- --              <-- should pick 4D to go to tableau 0 on top of 5S
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * QD QC KC 2S 5H JH KD 5C
             * 3H JD JS 6H QS 6D 8D 9C
             * TD JC 8C 6C 8S 4S 5D QH
             * 7S 9D KS AS 6S 4H KH 8H
             * AH 9S TC 7C 3S TS 9H 2H
             * 5S    7H             7D
             *       TH               
             */

            // Arrange
            var r = Reserve.Create("4C", "4D");
            var f = Foundation.Create(Ranks.R3, Ranks.R3, Ranks.Nil, Ranks.Nil);
            var t0 = Tableau.Create("QD 3H TD 7S AH 5S");
            var t1 = Tableau.Create("QC JD JC 9D 9S");
            var t2 = Tableau.Create("KC JS 8C KS TC 7H TH");
            var t3 = Tableau.Create("2S 6H 6C AS 7C");
            var t4 = Tableau.Create("5H QS 8S 6S 3S");
            var t5 = Tableau.Create("JH 6D 4S 4H TS");
            var t6 = Tableau.Create("KD 8D 5D KH 9H");
            var t7 = Tableau.Create("5C 9C QH 8H 2H 7D");
            var ts = Tableaus.Create(t0, t1, t2, t3, t4, t5, t6, t7);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Single(moves.Where(m => m.Type == MoveType.ReserveToTableau));
            Assert.Equal(Move.Get(MoveType.ReserveToTableau, 1, 0), moves[2]);
        }

        [Fact]
        public void GetValidMoves_returns_all_valid_tt_moves()
        {
            /*
             * 00 01 02 03 04 05 06 07 (Deal #984)
             * -- -- -- -- -- -- -- --
             * 7S TC 5D QC 8C 2C JD 3C
             * JS 9H AS AH QH AC AD JH
             * 7H 4D KC TS 3D 6D KS 4C
             * TH JC QS 6H 5C 2D 2S 8D
             * KH 3H QD 2H 9C KD 3S 5H
             * 9D 5S 6S 4H TD 6C 7D 7C
             * 9S 4S 8S               
             * 8H
             *
             * Should find the following:
             * 1) 0 -> 4 (2 cards)
             * 2) 5 -> 6
             * 3) 6 -> 2
             * 4) 7 -> 0
             */

            // Arrange
            var b = Board.FromDealNum(984);
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 3, 0));

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(4, moves.Where(m => m.Type == MoveType.TableauToTableau).Count());
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 0, 4, 2), moves[0]);
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 5, 6), moves[1]);
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 6, 2), moves[2]);
            Assert.Equal(Move.Get(MoveType.TableauToTableau, 7, 0), moves[3]);
        }

        [Fact]
        public void GetValidMoves_returns_all_valid_tr_moves()
        {
            /*
             * 00 01 02 03 04 05 06 07 (Deal #984)
             * -- -- -- -- -- -- -- --
             * 7S TC 5D QC 8C 2C JD 3C
             * JS 9H AS AH QH AC AD JH
             * 7H 4D KC TS 3D 6D KS 4C
             * TH JC QS 6H 5C 2D 2S 8D
             * KH 3H QD 2H 9C KD 3S 5H
             * 9D 5S 6S 4H TD 6C 7D 7C
             * 9S 4S 8S 8H            
             *
             * Should find 8 moves from each tableau to reserve slot#1
             */

            // Arrange
            var b = Board.FromDealNum(984);

            // Act
            var moves = b.GetValidMoves();

            // Assert
            Assert.Equal(8, moves.Where(m => m.Type == MoveType.TableauToReserve).Count());
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 0, 0), moves[5]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 1, 0), moves[6]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 2, 0), moves[7]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 3, 0), moves[8]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 4, 0), moves[9]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 5, 0), moves[10]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 6, 0), moves[11]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 7, 0), moves[12]);
        }

        [Fact]
        public void Moves_to_foundation_maintain_movesEstimated()
        {
            /*
             * 00 01 02 03 04 05 06 07 (Deal #984)
             * -- -- -- -- -- -- -- --
             * 7S TC 5D QC 8C 2C JD 3C
             * JS 9H AS AH QH AC AD JH
             * 7H 4D KC TS 3D 6D KS 4C
             * TH JC QS 6H 5C 2D 2S 8D
             * KH 3H QD 2H 9C KD 3S 5H
             * 9D 5S 6S 4H TD 6C 7D 7C
             * 9S 4S 8S 8H            
             */

            // Arrange
            var b = Board.FromDealNum(984)
                .ExecuteMove(Move.Get(MoveType.TableauToReserve, 6, 0))
                .ExecuteMove(Move.Get(MoveType.TableauToReserve, 6, 1))
                .ExecuteMove(Move.Get(MoveType.TableauToReserve, 6, 2));
            Assert.Equal(52, b.MovesEstimated);

            // Act
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 6, 3));

            // Assert
            Assert.Equal(51, b.MovesEstimated);
        }

        [Fact]
        public void ExecuteMove_stores_lastMove_prev_state_and_maintains_moveCount()
        {
            // Arrange
            var b1 = Board.FromDealNum(1);
            var move = Move.Get(MoveType.TableauToReserve, 0, 0);

            // Act
            var b2 = b1.ExecuteMove(move);

            // Assert
            var lastMove = b2.LastMove;
            Assert.True(lastMove.Type == move.Type && lastMove.From == move.From && lastMove.To == move.To && lastMove.Size == move.Size);
            Assert.Equal(b1, b2.Prev);
            Assert.Equal(1, b2._manualMoveCount);
        }

        [Fact]
        public void ExecuteMove_is_immutable()
        {
            // Arrange
            var b1 = Board.FromDealNum(1);
            var move = Move.Get(MoveType.TableauToReserve, 0, 0);

            // Act
            _ = b1.ExecuteMove(move);

            // Assert
            Assert.Equal(Board.FromDealNum(1), b1);
        }

        [Fact]
        public void ExecuteMove_executes_tf_moves()
        {
            /*
             * 00 01 02 03 04 05 06 07 (Deal #4)
             * -- -- -- -- -- -- -- --
             * KS QC 3D JS 5D KD 6S 3S
             * 2C AC KH 8C AH 9D 6C 5C
             * 6D TS QS 4D 4H 2S QH 7S
             * 9S 5S AS 8H 8D 4C 5H 3C
             * TC TH 7C 3H 7H 2H JH TD
             * JC QD KC 2D 8S 6H 9H JD
             * 9C 7D 4S AD          
             */

            // Arrange
            var b = Board.FromDealNum(4);

            // Act
            b = b.ExecuteMove(Move.Get(MoveType.TableauToFoundation, 3), false);

            // Assert
            Assert.Equal(Card.Get("2D"), b.Tableaus[3].Top);
            Assert.Equal(Ranks.R2, b.Foundation[Suits.Diamonds]);
        }

        [Fact]
        public void ExecuteMove_executes_tr_moves()
        {
            /*
             * 00 01 02 03 04 05 06 07 (Deal #4)
             * -- -- -- -- -- -- -- --
             * KS QC 3D JS 5D KD 6S 3S
             * 2C AC KH 8C AH 9D 6C 5C
             * 6D TS QS 4D 4H 2S QH 7S
             * 9S 5S AS 8H 8D 4C 5H 3C
             * TC TH 7C 3H 7H 2H JH TD
             * JC QD KC 2D 8S 6H 9H JD
             * 9C 7D 4S AD          
             */

            // Arrange
            var b = Board.FromDealNum(4);

            // Act
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0), false);

            // Assert
            Assert.Equal(Card.Get("JC"), b.Tableaus[0].Top);
            Assert.Equal(Card.Get("9C"), b.Reserve[0]);
        }

        [Fact]
        public void ExecuteMove_executes_tt_moves()
        {
            /*
             * 00 01 02 03 04 05 06 07 (Deal #4)
             * -- -- -- -- -- -- -- --
             * KS QC 3D JS 5D KD 6S 3S
             * 2C AC KH 8C AH 9D 6C 5C
             * 6D TS QS 4D 4H 2S QH 7S
             * 9S 5S AS 8H 8D 4C 5H 3C
             * TC TH 7C 3H 7H 2H JH TD
             * JC QD KC 2D 8S 6H 9H JD
             * 9C 7D 4S AD          
             */

            // Arrange
            var b = Board.FromDealNum(4);

            // Act
            b = b.ExecuteMove(Move.Get(MoveType.TableauToTableau, 4, 6), false);

            // Assert
            Assert.Equal(Card.Get("7H"), b.Tableaus[4].Top);
            Assert.Equal(Card.Get("8S"), b.Tableaus[6].Top);
        }

        [Fact]
        public void ExecuteMove_executes_rf_moves()
        {
            /*
             * aa bb cc dd
             * AD -- -- --
             *
             * 00 01 02 03 04 05 06 07 (Deal #4)
             * -- -- -- -- -- -- -- --
             * KS QC 3D JS 5D KD 6S 3S
             * 2C AC KH 8C AH 9D 6C 5C
             * 6D TS QS 4D 4H 2S QH 7S
             * 9S 5S AS 8H 8D 4C 5H 3C
             * TC TH 7C 3H 7H 2H JH TD
             * JC QD KC 2D 8S 6H 9H JD
             * 9C 7D 4S             
             */

            // Arrange
            var b = Board.FromDealNum(4);
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 3, 0), false);

            // Act
            b = b.ExecuteMove(Move.Get(MoveType.ReserveToFoundation, 0), false);

            // Assert
            Assert.Equal(Card.Null, b.Reserve[0]);
            Assert.Equal(Ranks.R2, b.Foundation[Suits.Diamonds]);
        }

        [Fact]
        public void ExecuteMove_executes_rt_moves()
        {
            /*
             * aa bb cc dd
             * 7D -- -- --
             *
             * 00 01 02 03 04 05 06 07 (Deal #4)
             * -- -- -- -- -- -- -- --
             * KS QC 3D JS 5D KD 6S 3S
             * 2C AC KH 8C AH 9D 6C 5C
             * 6D TS QS 4D 4H 2S QH 7S
             * 9S 5S AS 8H 8D 4C 5H 3C
             * TC TH 7C 3H 7H 2H JH TD
             * JC QD KC 2D 8S 6H 9H JD
             * 9C    4S AD          
             */

            // Arrange
            var b = Board.FromDealNum(4);
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 1, 0), false);

            // Act
            b = b.ExecuteMove(Move.Get(MoveType.ReserveToTableau, 0, 4), false);

            // Assert
            Assert.Equal(Card.Null, b.Reserve[0]);
            Assert.Equal(Card.Get("7D"), b.Tableaus[4].Top);
        }

        [Fact]
        public void ExecuteMove_auto_plays_all_possible_moves_next()
        {
            /* 
             * CC DD HH SS
             * TC KD KH KS
             *
             * aa bb cc dd
             * QC -- -- --
             *
             * 00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --
             * KC                     
             * JC
             */
            var r = Reserve.Create("QC");
            var f = Foundation.Create(Ranks.R10, Ranks.Rk, Ranks.Rk, Ranks.Rk);
            var t0 = Tableau.Create("KC JC");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, tRest, tRest, tRest, tRest, tRest, tRest, tRest);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            // Act
            b = b.ExecuteMove(Move.Get(MoveType.TableauToFoundation, 0));

            // Assert
            Assert.Equal(1, b._manualMoveCount);
            Assert.Equal(2, b.AutoMoveCount);
            Assert.Equal(3, b.MoveCount);
            Assert.True(b.IsSolved);
        }

        [Fact]
        public void RootAutoPlay_appends_root_node_to_prev_when_autoMove_found()
        {
            // Arrange
            var b = Board.FromDealNum(2); // Deal #2 has an auto move right from the start
            var clone = b.Clone();

            // Act
            b.RootAutoPlay();

            // Assert
            Assert.NotNull(b.Prev);
            Assert.Equal(clone, b.Prev);

            // Arrange
            b = Board.FromDealNum(1); // Deal #1 has no auto moves initially

            // Act
            b.RootAutoPlay();

            // Assert
            Assert.Null(b.Prev);
        }

        [Fact]
        public void ComputeCost_computes_cost()
        {
            /* 
             * CC DD HH SS
             * -- -- 3H 2S              colorDiff       = abs(0 + 2 - 0 - 3)            = 1
             *                          movesEstimated  = 52 - (0 + 0 + 3 + 2) = 47 * 2 = 94
             * aa bb cc dd
             * QC -- -- 9D              occupied        = 4 - 2                         = 2
             * 
             * 00 01 02 03 04 05 06 07  unsorted_size                                   = 18
             * -- -- -- -- -- -- -- --
             * KC 5D TS 3S 9C TH 4D QD  num_buried                                      = 10
             * QH AC 7H 5S 4S 7S 5C JS
             *    2D 8H 4H JC 6D AD TD
             *    KS    3C JH    9S   
             *    KH       8S    8D   
             *    6S       KD    7C   
             *    4C       QS    6H   
             *    3D       JD         
             *    2C       TC         
             *             9H         
             *             8C         
             *             7D         
             *             6C         
             *             5H         
             */
            var r = Reserve.Create("QC", null, null, "9D");
            var f = Foundation.Create(Ranks.Nil, Ranks.Nil, Ranks.R3, Ranks.R2);
            var t0 = Tableau.Create("KC QH");                                      // unsorted = 0, buried = 0
            var t1 = Tableau.Create("5D AC 2D KS KH 6S 4C 3D 2C AD");              // unsorted = 6, buried = 7 -> 7 for (AC), (AD) is within sorted stack so it wont be counted
            var t2 = Tableau.Create("TS 7H 8H");                                   // unsorted = 2, buried = 0
            var t3 = Tableau.Create("3S 5S 4H 3C");                                // unsorted = 1, buried = 3 -> 3 for (3S), (4H) is within sorted stack so it wont be counted
            var t4 = Tableau.Create("9C 4S JC JH 8S KD QS JD TC 9H 8C 7D 6C 5H");  // unsorted = 5, buried = 0
            var t5 = Tableau.Create("TH 7S 6D");                                   // unsorted = 1, buried = 0
            var t6 = Tableau.Create("4D 5C 9S 8D 7C 6H");                          // unsorted = 3, buried = 0
            var t7 = Tableau.Create("QD JS TD");                                   // unsorted = 0, buried = 0
            var ts = Tableaus.Create(t0, t1, t2, t3, t4, t5, t6, t7);
            var b = Board.Create(r, f, ts);
            Assert.True(b.IsValid());

            b.ComputeCost();
            Assert.Equal(125, b._cost);
        }

        [Fact]
        public void GetMoves_returns_moves1()
        {
            /*
             * 00 01 02 03 04 05 06 07 (Deal #4)
             * -- -- -- -- -- -- -- --
             * KS QC 3D JS 5D KD 6S 3S
             * 2C AC KH 8C AH 9D 6C 5C
             * 6D TS QS 4D 4H 2S QH 7S
             * 9S 5S AS 8H 8D 4C 5H 3C
             * TC TH 7C 3H 7H 2H JH TD
             * JC QD KC 2D 8S 6H 9H JD
             * 9C 7D 4S AD          
             */

            // Arrange
            var b = Board.FromDealNum(4);
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 3, 0));
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 2, 0));
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 2, 1));
            b = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 2, 2));

            // Act
            var moves = b.GetMoves().ToList();

            // Assert
            Assert.Equal(7, moves.Count); // 4 manual and 3 auto
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 3, 0), moves[0]);
            Assert.Equal(Move.Get(MoveType.ReserveToFoundation, 0), moves[1]);
            Assert.Equal(Move.Get(MoveType.TableauToFoundation, 3), moves[2]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 2, 0), moves[3]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 2, 1), moves[4]);
            Assert.Equal(Move.Get(MoveType.TableauToReserve, 2, 2), moves[5]);
            Assert.Equal(Move.Get(MoveType.TableauToFoundation, 2), moves[6]);
        }

        [Fact]
        public void GetMoves_returns_moves2()
        {
            // Arrange
            var b = Board.Create(Reserve.Create(), Foundation.Create(10, 12, 12, 12), Tableaus.Create(Tableau.Create("KC QC")));
            Assert.True(b.IsValid());
            b.RootAutoPlay();

            // Act
            var moves = b.GetMoves().ToList();

            // Assert
            Assert.Equal(2, moves.Count); // 2 auto
            Assert.Equal(Move.Get(MoveType.TableauToFoundation, 0), moves[0]);
            Assert.Equal(Move.Get(MoveType.TableauToFoundation, 0), moves[1]);
        }

        [Fact]
        public void Traverse_traverses_states_backwards()
        {
            // Arrange
            var b = Board.FromDealNum(4);
            var c = b.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0));

            var i = 0;
            c.Traverse(n =>
            {
                if (i++ == 0)
                {
                    Assert.Same(c, n);
                }
                else
                {
                    Assert.Equal(b, n);
                }
            });
        }

        [Fact]
        public void Clone_clones_object()
        {
            Assert.True(Board.FromDealNum(5) == Board.FromDealNum(5).Clone());

            var b1 = Board.FromDealNum(5)
                .ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0), false)
                .ExecuteMove(Move.Get(MoveType.TableauToFoundation, 1), false)
                .ExecuteMove(Move.Get(MoveType.TableauToReserve, 7, 1)); // auto play here

            var b2 = b1.Clone();
            Assert.True(b1 == b2);
            Assert.Equal(b1._manualMoveCount, b2._manualMoveCount);
            Assert.Equal(b1.AutoMoveCount, b2.AutoMoveCount);
            Assert.Equal(b1.MovesEstimated, b2.MovesEstimated);
            Assert.Equal(b1.LastMove, b2.LastMove);

            Assert.NotSame(b1, b2);
            Assert.NotSame(b1.Foundation, b2.Foundation);
            Assert.NotSame(b1.Reserve, b2.Reserve);
            Assert.NotSame(b1.Tableaus, b2.Tableaus);

            var fi = typeof(Reserve).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotSame(fi.GetValue(b1.Reserve), fi.GetValue(b2.Reserve));

            fi = typeof(Foundation).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotSame(fi.GetValue(b1.Foundation), fi.GetValue(b2.Foundation));

            fi = typeof(Tableaus).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotSame(fi.GetValue(b1.Tableaus), fi.GetValue(b2.Tableaus));

            fi = typeof(Tableau).GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            for (var i = 0; i < 8; i++)
            {
                Assert.NotSame(b1.Tableaus[i], b2.Tableaus[i]);
                Assert.Same(b1.Tableaus[i].Top, b2.Tableaus[i].Top);
                Assert.NotSame(fi.GetValue(b1.Tableaus[i]), fi.GetValue(b2.Tableaus[i]));
            }
        }

        [Fact]
        public void ToString_returns_string_representation()
        {
            var b = Board.FromDealNum(5);

            Assert.Equal(
@"CC DD HH SS
-- -- -- --

00 01 02 03
-- -- -- --

00 01 02 03 04 05 06 07
-- -- -- -- -- -- -- --
AH 8S 2D QS 4C 9H 2S 3D
5C AS 9C KH 4D 2C 3C 4S
3S 5D KC 3H KD 5H 6S 8D
TD 7S JD 7H 8H JH JC 7D
5S QH 8C 9D KS QD 4H AC
2H TC TH 6D 6H 6C QC JS
9S AD 7C TS            ", b.ToString());
        }

        [Theory]
        [InlineData(10, 0, 99, 0, -1)]
        [InlineData(10, 99, 99, 0, -1)]
        [InlineData(10, 0, 99, 99, -1)]
        [InlineData(99, 0, 10, 0, 1)]
        [InlineData(99, 99, 10, 0, 1)]
        [InlineData(99, 0, 10, 99, 1)]
        [InlineData(10, 10, 10, 10, 0)]
        public void ComparisonTests(int b1Cost, int b1MoveCount, int b2Cost, int b2MoveCount, int expected)
        {
            // Arrange
            var b1 = Board.FromDealNum(1);
            b1._cost = b1Cost;
            b1._manualMoveCount = b1MoveCount;

            var b2 = Board.FromDealNum(1);
            b2._cost = b2Cost;
            b2._manualMoveCount = b2MoveCount;

            // Act
            var result = b1.CompareTo(b2);

            // Assert
            Assert.True(
                (expected < 0 && result < 0) ||
                (expected > 0 && result > 0) ||
                (expected == 0 && result == 0));
        }

        [Fact]
        public void EqualityTests()
        {
            Assert.False(Board.FromDealNum(5).Equals(new object()));
            Assert.False(Board.FromDealNum(1) == Board.FromDealNum(2));

            var b1 = Board.FromDealNum(5);
            var b2 = Board.FromDealNum(5);
            Assert.True(b1 == b2);

            b1 = b1.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0), false);
            Assert.True(b1 != b2);

            b2 = b2.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0), false);
            Assert.True(b1 == b2);

            b1 = b1.ExecuteMove(Move.Get(MoveType.TableauToFoundation, 1), false);
            Assert.True(b1 != b2);

            b2 = b2.ExecuteMove(Move.Get(MoveType.TableauToFoundation, 1), false);
            Assert.True(b1 == b2);

            b1 = Board.Create(Reserve.Create("AD", "AH"), Foundation.Create(), Tableaus.Create());
            b2 = Board.Create(Reserve.Create("AC", "AS"), Foundation.Create(), Tableaus.Create());
            Assert.True(b1 != b2);

            b1 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KH")));
            b2 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KH")));
            Assert.True(b1 == b2);

            b1 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KH QS")));
            b2 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KD QS")));
            Assert.True(b1 != b2);

            b1 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KH QS")));
            b2 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KD")));
            Assert.True(b1 != b2);

            b1 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KH QS")));
            b2 = Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(Tableau.Create("KD TS 9D")));
            Assert.True(b1 != b2);
        }

        [Fact]
        public void HashingTests()
        {
            Assert.False(Board.FromDealNum(1).GetHashCode() == Board.FromDealNum(2).GetHashCode());

            var b1 = Board.FromDealNum(5);
            var b2 = Board.FromDealNum(5);
            Assert.True(b1.GetHashCode() == b2.GetHashCode());

            b1 = b1.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0), false);
            Assert.True(b1.GetHashCode() != b2.GetHashCode());

            b2 = b2.ExecuteMove(Move.Get(MoveType.TableauToReserve, 0, 0), false);
            Assert.True(b1.GetHashCode() == b2.GetHashCode());

            b1 = b1.ExecuteMove(Move.Get(MoveType.TableauToFoundation, 1), false);
            Assert.True(b1.GetHashCode() != b2.GetHashCode());

            b2 = b2.ExecuteMove(Move.Get(MoveType.TableauToFoundation, 1), false);
            Assert.True(b1.GetHashCode() == b2.GetHashCode());
        }

        [Fact]
        public void Board_should_be_equal_when_same_reserve_but_different_order()
        {
            /*
             * CC DD HH SS               |    CC DD HH SS
             * 4C 6D 2H 8S               |    4C 6D 2H 8S
             *                           |
             * aa bb cc dd               |    aa bb cc dd
             * -- 6H -- JD               |    6H JD -- --
             *                           |     
             * 00 01 02 03 04 05 06 07   |    00 01 02 03 04 05 06 07
             * -- -- -- -- -- -- -- --   |    -- -- -- -- -- -- -- --
             * 9S KS QS JS KD JH 5C      |    9S KS QS JS KD JH 5C   
             * QD    QC 8C    9D 6C      |    QD    QC 8C    9D 6C   
             * 7D    TS 3H    9H QH      |    7D    TS 3H    9H QH   
             * TC    7C 7H    8H 8D      |    TC    7C 7H    8H 8D   
             * JC    KH KC    4H 5H      |    JC    KH KC    4H 5H   
             * TH                TD      |    TH                TD   
             * 9C                        |                      9C     
             */

            // Arrange
            var r = Reserve.Create(null, "6H", null, "JD");
            var f = Foundation.Create(Ranks.R4, Ranks.R6, Ranks.R2, Ranks.R8);
            var t0 = Tableau.Create("9S QD 7D TC JC TH 9C");
            var t1 = Tableau.Create("KS");
            var t2 = Tableau.Create("QS QC TS 7C KH");
            var t3 = Tableau.Create("JS 8C 3H 7H KC");
            var t4 = Tableau.Create("KD");
            var t5 = Tableau.Create("JH 9D 9H 8H 4H");
            var t6 = Tableau.Create("5C 6C QH 8D 5H TD");
            var tRest = Tableau.Create();
            var ts = Tableaus.Create(t0, t1, t2, t3, t4, t5, t6, tRest);
            var b1 = Board.Create(r, f, ts);
            Assert.True(b1.IsValid());

            r = Reserve.Create("6H", "JD");
            var b2 = Board.Create(r, f, ts);
            Assert.True(b2.IsValid());

            Assert.True(b1 == b2);
            Assert.True(b1.GetHashCode() == b2.GetHashCode());
        }
    }
}