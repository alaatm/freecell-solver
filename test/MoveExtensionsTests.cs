using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;
using Xunit;

namespace FreeCellSolver.Test
{
    public class MoveExtensionsTests
    {
        [Theory]
        [InlineData(0, 1, 2, MoveType.ReserveToFoundation, 0, 0, 1, false)]
        [InlineData(0, 1, 2, MoveType.TableauToFoundation, 0, 0, 1, false)]
        [InlineData(0, 1, 2, MoveType.ReserveToTableau, 0, 0, 1, false)]
        [InlineData(0, 1, 2, MoveType.TableauToReserve, 0, 0, 1, false)]

        [InlineData(0, 1, 2, MoveType.TableauToTableau, 0, 1, 2, false)]
        [InlineData(0, 1, 2, MoveType.TableauToTableau, 2, 0, 2, false)]
        [InlineData(0, 1, 2, MoveType.TableauToTableau, 1, 0, 1, false)]

        [InlineData(0, 1, 2, MoveType.TableauToTableau, 1, 0, 2, true)]
        public void IsReverseOfTT_returns_whether_TT_move1_is_reverse_of_TT_move2(int currentFrom, int currentTo, int currentSize, MoveType lastType, int lastFrom, int lastTo, int lastSize, bool expected)
        {
            // Arrange
            var current = Move.Get(MoveType.TableauToTableau, currentFrom, currentTo, currentSize);
            var last = Move.Get(lastType, lastFrom, lastTo, lastSize);

            // Act
            var r = current.IsReverseOfTT(last);

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
