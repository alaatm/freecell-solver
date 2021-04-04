using FreeCellSolver.Game;
using Xunit;

namespace FreeCellSolver.Test
{
    public class MoveTests
    {
        [Fact]
        public void Can_get_move()
        {
            var move = Move.Get(MoveType.ReserveToTableau, 0, 2);
            Assert.Equal(MoveType.ReserveToTableau, move.Type);
            Assert.Equal(0, move.From);
            Assert.Equal(2, move.To);
            Assert.Equal(1, move.Size);

            move = Move.Get(MoveType.TableauToTableau, 0, 2, 3);
            Assert.Equal(MoveType.TableauToTableau, move.Type);
            Assert.Equal(0, move.From);
            Assert.Equal(2, move.To);
            Assert.Equal(3, move.Size);
        }

        [Fact]
        public void GetHashCode_tests()
        {
            Assert.Equal(
                Move.Get(MoveType.TableauToTableau, 1, 3, 2).GetHashCode(),
                Move.Get(MoveType.TableauToTableau, 1, 3, 2).GetHashCode());

            Assert.Equal(((Move)default).GetHashCode(), ((Move)default).GetHashCode());

            Assert.NotEqual(
                Move.Get(MoveType.TableauToTableau, 1, 3, 2).GetHashCode(),
                Move.Get(MoveType.TableauToTableau, 1, 3, 1).GetHashCode());
        }

        [Fact]
        public void Equality_tests()
        {
            Assert.True(
                Move.Get(MoveType.TableauToTableau, 1, 3, 2) ==
                Move.Get(MoveType.TableauToTableau, 1, 3, 2));

            Assert.True(default == ((Move)default));

            Assert.True(
                Move.Get(MoveType.TableauToTableau, 1, 3, 2) !=
                Move.Get(MoveType.TableauToTableau, 1, 2, 2));

            Assert.True(
                Move.Get(MoveType.TableauToTableau, 1, 3, 2) !=
                Move.Get(MoveType.TableauToTableau, 1, 3, 1));

            Assert.False(((Move)default).Equals(new object()));
        }

        [Theory]
        [InlineData(MoveType.ReserveToFoundation, 0, 0, 1, "ah")]
        [InlineData(MoveType.ReserveToTableau, 1, 1, 1, "b1")]
        [InlineData(MoveType.TableauToFoundation, 2, 0, 1, "2h")]
        [InlineData(MoveType.TableauToTableau, 5, 1, 2, "51{2}")]
        [InlineData(MoveType.TableauToReserve, 7, 2, 1, "7c")]
        public void ToString_returns_string_representation(MoveType type, int from, int to, int size, string expectedToStringValue)
            => Assert.Equal(expectedToStringValue, Move.Get(type, from, to, size).ToString());
    }
}
