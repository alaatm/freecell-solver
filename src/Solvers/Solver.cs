using System;
using System.Threading.Tasks;
using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    public enum SolverType
    {
        AStar,
        Dfs,
    }

    public interface ISolver
    {
        Board SolvedBoard { get; }
        int SolvedFromId { get; }
        int VisitedNodes { get; }
    }

    public static class Solver
    {
        public static ISolver Run(SolverType solverType, Board board, bool best = false) => solverType switch
        {
            SolverType.AStar => AStar.Run(board, best),
            SolverType.Dfs => Dfs.Run(board),
            _ => throw new ArgumentException($"Invalid value for '{nameof(solverType)}'."),
        };

        public static Task<ISolver> RunParallelAsync(SolverType solverType, Board board, bool best = false) => solverType switch
        {
            SolverType.AStar => AStar.RunParallelAsync(board, best),
            SolverType.Dfs => Dfs.RunParallelAsync(board),
            _ => throw new ArgumentException($"Invalid value for '{nameof(solverType)}'."),
        };
    }
}