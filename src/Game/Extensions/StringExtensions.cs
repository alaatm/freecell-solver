namespace FreeCellSolver.Game.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value) => string.IsNullOrWhiteSpace(value);
    }
}
