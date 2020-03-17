using System.Text;
using System.Collections.Generic;

namespace FreeCellSolver.Game.Extensions
{
    public static class MoveExtensions
    {
        public static bool IsReverseOf(this Move val, Move other)
            => other != null && (
                    (val.Type == MoveType.TableauToTableau && other.Type == MoveType.TableauToTableau) ||
                    (val.Type == MoveType.TableauToReserve && other.Type == MoveType.ReserveToTableau) ||
                    (val.Type == MoveType.ReserveToTableau && other.Type == MoveType.TableauToReserve)
                ) && 
                val.From == other.To && val.To == other.From && val.Size == other.Size;

        public static string AsJson(this IEnumerable<Move> moves)
        {
            var json = new StringBuilder();
            json.Append("[");
            foreach (var move in moves)
            {
                json.Append($"{{type:{(int)move.Type},from:{move.From},to:{move.To},size:{move.Size}}},");
            }
            json.Append("];");

            return json.ToString();
        }
    }
}