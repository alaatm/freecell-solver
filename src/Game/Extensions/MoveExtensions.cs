using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace FreeCellSolver.Game.Extensions
{
    public static class MoveExtensions
    {
        public static bool IsReverseOf(this Move val, Move other)
            => other.Type != MoveType.None && (
                    (val.Type == MoveType.TableauToTableau && other.Type == MoveType.TableauToTableau) ||
                    (val.Type == MoveType.TableauToReserve && other.Type == MoveType.ReserveToTableau) ||
                    (val.Type == MoveType.ReserveToTableau && other.Type == MoveType.TableauToReserve)
                ) &&
                val.From == other.To && val.To == other.From && val.Size == other.Size;

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