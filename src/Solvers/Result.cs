using FreeCellSolver.Game;

namespace FreeCellSolver.Solvers
{
    public struct Result
    {
        public Board GoalNode { get; init; }
        public int VisitedNodes { get; init; }
        public int Threads { get; init; }

        public bool IsSolved => GoalNode is not null;
    }
}
