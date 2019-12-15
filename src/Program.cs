using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading.Tasks;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            await RunBenchmarksAsync();

            // var cards = Deck.Random();
            // var tableaus = new List<Tableau>();
            // for (var i = 0; i < 8; i++)
            // {
            //     tableaus.Add(new Tableau(i, cards.Skip(i < 4 ? i * 7 : i * 6).Take(i < 4 ? 7 : 6).ToList()));
            // }
            // Solver.Solve(new Board(new Deal(tableaus)));
        }

        static async Task RunBenchmarksAsync()
        {
            var sw = new Stopwatch();
            Board b;

            // Console.WriteLine($"Processing fast board");
            // sw.Restart();
            // b = await ParallelSolver.SolveAsync(BoardExtensions.GetFastBoard());
            // Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            // Console.WriteLine();

            // Console.WriteLine($"Processing normal board");
            // sw.Restart();
            // b = await ParallelSolver.SolveAsync(BoardExtensions.GetNormalBoard());
            // Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            // Console.WriteLine();

            Console.WriteLine($"Processing slow board");
            sw.Restart();
            b = await ParallelSolver.SolveAsync(BoardExtensions.GetSlowBoard());
            Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
        }
    }
}
