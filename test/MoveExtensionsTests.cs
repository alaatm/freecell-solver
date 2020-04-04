using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;
using Xunit;

namespace FreeCellSolver.Test
{
    public class MoveExtensionsTests
    {
        [Fact]
        public void AsJson_returns_json_string() => Assert.Equal(
            "[{type:0,from:0,to:1,size:1},{type:1,from:1,to:2,size:1},{type:2,from:2,to:3,size:2},{type:3,from:3,to:3,size:1},{type:4,from:3,to:5,size:1},];",
            new[]
            {
                Move.Get(MoveType.TableauToFoundation, 0, 1),
                Move.Get(MoveType.TableauToReserve, 1, 2),
                Move.Get(MoveType.TableauToTableau, 2, 3, 2),
                Move.Get(MoveType.ReserveToFoundation, 3, 3),
                Move.Get(MoveType.ReserveToTableau, 3, 5),
            }.AsJson());
    }
}
