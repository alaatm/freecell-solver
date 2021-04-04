using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;
using Xunit;

namespace FreeCellSolver.Test
{
    public class MoveExtensionsTests
    {
        [Fact]
        public void IsReverseOf_returns_false_when_other_is_emptyMove()
        {
            // Arrange
            var m1 = Move.Get(MoveType.ReserveToFoundation, 0);

            // Act
            var r = m1.IsReverseOf(default);

            // Assert
            Assert.False(r);
        }

        [Theory]
        [InlineData(MoveType.TableauToTableau, 1, 2, 1, MoveType.TableauToTableau, 2, 1, 1, true)]
        [InlineData(MoveType.TableauToTableau, 1, 2, 2, MoveType.TableauToTableau, 2, 1, 2, true)]
        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.ReserveToTableau, 2, 1, 1, true)]
        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.TableauToReserve, 2, 1, 1, true)]

        [InlineData(MoveType.TableauToTableau, 1, 2, 1, MoveType.TableauToTableau, 2, 1, 2, false)]
        [InlineData(MoveType.TableauToTableau, 1, 2, 1, MoveType.TableauToTableau, 3, 1, 1, false)]
        [InlineData(MoveType.TableauToTableau, 1, 2, 1, MoveType.TableauToReserve, 2, 1, 1, false)]
        [InlineData(MoveType.TableauToTableau, 1, 2, 1, MoveType.ReserveToTableau, 2, 1, 1, false)]
        [InlineData(MoveType.TableauToTableau, 1, 2, 1, MoveType.ReserveToFoundation, 2, 0, 1, false)]
        [InlineData(MoveType.TableauToTableau, 1, 2, 1, MoveType.TableauToFoundation, 2, 0, 1, false)]

        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.TableauToTableau, 2, 1, 2, false)]
        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.TableauToTableau, 3, 1, 1, false)]
        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.TableauToReserve, 2, 1, 1, false)]
        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.ReserveToTableau, 3, 1, 1, false)]
        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.ReserveToTableau, 2, 2, 1, false)]
        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.ReserveToFoundation, 2, 0, 1, false)]
        [InlineData(MoveType.TableauToReserve, 1, 2, 1, MoveType.TableauToFoundation, 2, 0, 1, false)]

        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.TableauToTableau, 2, 1, 2, false)]
        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.TableauToTableau, 3, 1, 1, false)]
        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.TableauToReserve, 3, 1, 1, false)]
        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.TableauToReserve, 2, 2, 1, false)]
        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.ReserveToTableau, 2, 1, 1, false)]
        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.ReserveToFoundation, 2, 0, 1, false)]
        [InlineData(MoveType.ReserveToTableau, 1, 2, 1, MoveType.TableauToFoundation, 2, 0, 1, false)]
        public void IsReverseOf_returns_whether_move1_is_reverse_of_move2(MoveType mt1, int from1, int to1, int size1, MoveType mt2, int from2, int to2, int size2, bool expected)
        {
            // Arrange
            var m1 = Move.Get(mt1, from1, to1, size1);
            var m2 = Move.Get(mt2, from2, to2, size2);

            // Act
            var r = m1.IsReverseOf(m2);

            // Assert
            Assert.Equal(expected, r);
        }

        [Fact]
        public void AsJson_returns_json_string() => Assert.Equal(
            "[{type:0,from:0,to:0,size:1},{type:1,from:1,to:2,size:1},{type:2,from:2,to:3,size:2},{type:3,from:3,to:0,size:1},{type:4,from:3,to:5,size:1},];",
            new[]
            {
                Move.Get(MoveType.TableauToFoundation, 0),
                Move.Get(MoveType.TableauToReserve, 1, 2),
                Move.Get(MoveType.TableauToTableau, 2, 3, 2),
                Move.Get(MoveType.ReserveToFoundation, 3),
                Move.Get(MoveType.ReserveToTableau, 3, 5),
            }.AsJson());
    }
}
