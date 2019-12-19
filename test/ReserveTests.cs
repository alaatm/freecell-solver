using FreeCellSolver.Extensions;
using Xunit;

namespace FreeCellSolver.Test
{
    public class ReserveTests
    {
        [Fact]
        public void State_is_initialized_to_empty_reserve()
        {
            var reserve = new Reserve();

            for (var i = 0; i < 4; i++)
            {
                Assert.Null(reserve[i]);
            }
        }

        [Fact]
        public void FreeCount_returns_free_cell_count()
        {
            var reserve = new Reserve();

            Assert.Equal(4, reserve.FreeCount);

            reserve.Insert(0, Cards.KingOfSpades);
            Assert.Equal(3, reserve.FreeCount);

            reserve.Insert(1, Cards.KingOfHearts);
            Assert.Equal(2, reserve.FreeCount);

            reserve.Insert(2, Cards.KingOfDiamonds);
            Assert.Equal(1, reserve.FreeCount);

            reserve.Insert(3, Cards.KingOfClubs);
            Assert.Equal(0, reserve.FreeCount);
        }

        [Fact]
        public void CanInsert_returns_whether_card_can_be_inserted_along_with_target_index()
        {
            var reserve = new Reserve();
            reserve.Insert(0, Cards.AceOfClubs);
            reserve.Insert(2, Cards.AceOfDiamonds);
            reserve.Insert(3, Cards.AceOfHearts);

            var (canInsert, index) = reserve.CanInsert(Cards.AceOfSpades);
            Assert.Equal(1, index);
            Assert.True(canInsert);
        }

        [Theory]
        [InlineData(4, 0, false)]
        [InlineData(4, 1, false)]
        [InlineData(4, 2, false)]
        [InlineData(4, 3, false)]
        [InlineData(3, 0, false)]
        [InlineData(3, 1, false)]
        [InlineData(3, 2, false)]
        [InlineData(3, 3, true)]
        [InlineData(2, 0, false)]
        [InlineData(2, 1, false)]
        [InlineData(2, 2, true)]
        [InlineData(2, 3, true)]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, true)]
        [InlineData(1, 3, true)]
        [InlineData(0, 0, true)]
        [InlineData(0, 1, true)]
        [InlineData(0, 2, true)]
        [InlineData(0, 3, true)]
        public void CanInsert_returns_whether_card_can_be_inserted_or_not(int occupiedCellsCount, int insertAt, bool expectedCanInsert)
        {
            var cards = new[] { Cards.KingOfSpades, Cards.KingOfHearts, Cards.KingOfDiamonds, Cards.KingOfClubs };

            var reserve = new Reserve();
            for (var i = 0; i < occupiedCellsCount; i++)
            {
                reserve.Insert(i, cards[i]);
            }

            Assert.Equal(expectedCanInsert, reserve.CanInsert(insertAt, Cards.AceOfSpades));
        }

        [Theory]
        [InlineData("AS", "", true)]    // Empty target tableau
        [InlineData("AS", "2H", true)]
        [InlineData("AS", "2S", false)]
        public void CanMove_returns_whether_card_can_be_moved_to_tableau_or_not(string cardToMove, string topCardAtTarget, bool expectedCanRemove)
        {
            var card = new Card(cardToMove);
            var tableau = new Tableau(0, topCardAtTarget);

            var reserve = new Reserve();
            reserve.Insert(0, card);

            Assert.Equal(expectedCanRemove, reserve.CanMove(card, tableau));
        }

        [Theory]
        [InlineData("AC", -1, true)]    // Empty foundation slot
        [InlineData("2C", -1, false)]
        [InlineData("2C", 0, true)]
        public void CanMove_returns_whether_card_can_be_moved_to_foundation_or_not(string cardToMove, int clubsTop, bool expectedCanRemove)
        {
            var card = new Card(cardToMove);
            var foundation = new Foundation();
            for (var i = -1; i < clubsTop; i++)
            {
                foundation.Push(new Card(Suit.Clubs, (Rank)i + 1));
            }

            var reserve = new Reserve();
            reserve.Insert(0, card);

            Assert.Equal(expectedCanRemove, reserve.CanMove(card, foundation));
        }

        [Fact]
        public void Insert_inserts_card()
        {
            var reserve = new Reserve();
            var index = 2;
            var card = Cards.FiveOfClubs;

            reserve.Insert(index, card);
            Assert.Equal(card, reserve[index]);
        }

        [Fact]
        public void Remove_removes_card()
        {
            var reserve = new Reserve();
            var index = 2;
            var card = Cards.FiveOfClubs;

            reserve.Insert(index, card);
            reserve.Remove(card);
            Assert.Null(reserve[index]);
        }

        [Fact]
        public void Move_moves_card_to_tableau()
        {
            var card = Cards.AceOfDiamonds;
            var tableau = new Tableau(0, "2C");
            var reserve = new Reserve();
            reserve.Insert(1, card);

            reserve.Move(card, tableau);

            Assert.Equal(4, reserve.FreeCount);
            Assert.Equal(2, tableau.Size);
            Assert.Equal(card, tableau.Top);
        }

        [Fact]
        public void Move_moves_card_to_foundation()
        {
            var card = Cards.AceOfClubs;
            var foundation = new Foundation();
            var reserve = new Reserve();
            reserve.Insert(1, card);

            reserve.Move(card, foundation);

            Assert.Equal(4, reserve.FreeCount);
            Assert.Equal(0, foundation[Suit.Clubs]);
        }
    }
}
