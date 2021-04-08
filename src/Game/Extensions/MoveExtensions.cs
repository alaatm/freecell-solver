using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver.Game.Extensions
{
    public static class MoveExtensions
    {
        public static string AsJson(this IEnumerable<Move> moves)
        {
            var json = new StringBuilder();
            json.Append('[');
            foreach (var move in moves)
            {
                Debug.Assert(move.Type != MoveType.None);
                json.Append($"{{type:{(int)move.Type - 1},from:{move.From},to:{move.To},size:{move.Size}}},");
            }
            json.Append("];");

            return json.ToString();
        }
    }
}