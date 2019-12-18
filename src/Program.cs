using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            // var b = BoardExtensions.GetFastBoard();
            // b.ToImage().Save(@"C:\personal-projs\freecell-solver\_temp\test.jpg");
            // var solved = Solver.Solve(b);
            // return;

            // var fs = File.CreateText(@"C:\personal-projs\freecell-solver\log.log");

            // var sw = new Stopwatch();
            // for (var i = 1; i < 32000; i++)
            // {
            //     fs.WriteLine($"Attempting deal #{i}");
            //     sw.Restart();
            //     var b = await ParallelSolver.SolveAsync(new Board(new Deal(i)));
            //     fs.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            //     await fs.FlushAsync();
            //     GC.Collect();
            // }
            // return;

            // var board = BoardExtensions.GetSolitareDeck();
            // if (board.Deal.IsValid())
            // {
            //     var b = await ParallelSolver.SolveAsync(board, b => b.Foundation.State.Values.Count(x => x >= 5) >= 2);
            //     if (b != null)
            //     {
            //         b.PrintMoves(@"C:\personal-projs\freecell-solver\_temp");
            //     }
            // }
            // return;

            await Task.Delay(0);
            // await RunBenchmarksAsync();
            // GC.Collect();
            RunBenchmarks();

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
            Console.WriteLine("=====================");
            Console.WriteLine("Parallel benchmarks");
            Console.WriteLine("=====================");
            Console.WriteLine();

            var sw = new Stopwatch();
            Board b;

            Console.WriteLine($"Processing fast board");
            sw.Restart();
            b = await ParallelSolver.SolveAsync(BoardExtensions.GetFastBoard());
            Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            Console.WriteLine();

            Console.WriteLine($"Processing normal board");
            sw.Restart();
            b = await ParallelSolver.SolveAsync(BoardExtensions.GetNormalBoard());
            Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            Console.WriteLine();

            Console.WriteLine($"Processing slow board");
            sw.Restart();
            b = await ParallelSolver.SolveAsync(BoardExtensions.GetSlowBoard());
            Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            Console.WriteLine();

            Console.WriteLine($"Processing Deal #6 board");
            sw.Restart();
            b = await ParallelSolver.SolveAsync(new Board(new Deal(6)));
            Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            Console.WriteLine();

            // Console.WriteLine($"Processing Deal #12 board");
            // sw.Restart();
            // b = await ParallelSolver.SolveAsync(new Board(new Deal(12)));
            // Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            // Console.WriteLine();

            // Console.WriteLine($"Processing Deal #169 board");
            // sw.Restart();
            // b = await ParallelSolver.SolveAsync(new Board(new Deal(169)));
            // Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            // Console.WriteLine();

            // Console.WriteLine($"Processing Deal #231 board");
            // sw.Restart();
            // b = await ParallelSolver.SolveAsync(new Board(new Deal(231)));
            // Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            // Console.WriteLine();
        }

        static void RunBenchmarks()
        {
            Console.WriteLine("=====================");
            Console.WriteLine("Sequential benchmarks");
            Console.WriteLine("=====================");
            Console.WriteLine();

            var sw = new Stopwatch();
            Board b;

            Console.WriteLine($"Processing extremly fast board");
            sw.Restart();
            b = Solver.Solve(BoardExtensions.GetExtremlyFastBoard());
            Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            Console.WriteLine();

            // Console.WriteLine($"Processing normal board");
            // sw.Restart();
            // b = Solver.Solve(BoardExtensions.GetNormalBoard());
            // Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            // Console.WriteLine();

            // Console.WriteLine($"Processing slow board");
            // sw.Restart();
            // b = Solver.Solve(BoardExtensions.GetSlowBoard());
            // Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {sw.Elapsed}");
            // Console.WriteLine();
        }
    }
}
