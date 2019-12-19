namespace FreeCellSolver
{
    public static class MoveExtensions
    {
        public static bool IsReverseOf(this Move val, Move other)
            => (val.Type == MoveType.TableauToTableau && other.Type == MoveType.TableauToTableau
                || val.Type == MoveType.TableauToReserve && other.Type == MoveType.ReserveToTableau
                || val.Type == MoveType.ReserveToTableau && other.Type == MoveType.TableauToReserve)
                && val.From == other.To && val.To == other.From && val.Size == other.Size;
    }
}