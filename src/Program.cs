using System;
using System.IO;
using System.Runtime;
using System.Diagnostics;
using System.Threading.Tasks;
using FreeCellSolver.Solvers;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            await ProcessArgsAsync(args);
        }

        static async Task ProcessArgsAsync(string[] args)
        {
            if (args.Length > 0)
            {
                var arg = args[0];
                if (arg == "--32k")
                {
                    await RunFullBenchmarksAsync(DfsSolveMethod.Stack, 32000);
                }
                else if (arg == "--full")
                {
                    await RunFullBenchmarksAsync(DfsSolveMethod.Recursive);
                    await RunFullBenchmarksAsync(DfsSolveMethod.Stack);
                }
                else if (arg == "--short")
                {
                    await RunShortBenchmarksAsync(DfsSolveMethod.Recursive);
                    await RunShortBenchmarksAsync(DfsSolveMethod.Stack);
                }
                else if (arg == "--single")
                {
                    await RunSingleBenchmarksAsync(DfsSolveMethod.Recursive);
                    await RunSingleBenchmarksAsync(DfsSolveMethod.Stack);
                }
                else if (arg == "--deal")
                {
                    var dealNum = int.Parse(args[1]);
                    var print = args.Length > 2 && args[2] == "--print";
                    await RunSingleBenchmarksAsync(DfsSolveMethod.Recursive, dealNum, print);
                    await RunSingleBenchmarksAsync(DfsSolveMethod.Stack, dealNum, print);
                }
            }
            else
            {
                RunBenchmarks(DfsSolveMethod.Recursive);
                RunBenchmarks(DfsSolveMethod.Stack);
            }
        }

        static async Task RunFullBenchmarksAsync(DfsSolveMethod method, int count = 1500)
        {
            var logFile = method switch
            {
                DfsSolveMethod.Recursive => "current-recursive",
                DfsSolveMethod.Stack => "current-stack",
                _ => null,
            };

            logFile += count == 32000 ? "-32k" : "";

            var fs = File.CreateText($@"C:\personal-projs\freecell-solver\{logFile}.log");
            var sw = new Stopwatch();
            for (var i = 1; i <= count; i++)
            {
                fs.WriteLine($"Attempting deal #{i}");
                sw.Restart();
                var b = await Dfs.RunParallelAsync(new Board(Deal.FromDealNum(i)), method);
                fs.WriteLine($"{(b.SolvedBoard != null ? "Done" : "Bailed")} in {sw.Elapsed}");
                await fs.FlushAsync();
                GC.Collect();
            }
            return;
        }

        static async Task RunShortBenchmarksAsync(DfsSolveMethod method)
        {
            var sw = new Stopwatch();

            Console.WriteLine($"Processing Deal #169 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(new Board(Deal.FromDealNum(169)), method), sw);
            GC.Collect();

            Console.WriteLine($"Processing Deal #178 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(new Board(Deal.FromDealNum(178)), method), sw);
            GC.Collect();

            Console.WriteLine($"Processing Deal #231 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(new Board(Deal.FromDealNum(231)), method), sw);
            GC.Collect();

            Console.WriteLine($"Processing Deal #261 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(new Board(Deal.FromDealNum(261)), method), sw);
            GC.Collect();
        }

        static async Task RunSingleBenchmarksAsync(DfsSolveMethod method, int dealNum = 169, bool print = false)
        {
            var sw = new Stopwatch();
            var b = new Board(Deal.FromDealNum(dealNum));

            Console.WriteLine($"Processing Deal #{dealNum} board");
            sw.Restart();
            var s = await Dfs.RunParallelAsync(b, method);
            PrintSummary(s, sw);
            GC.Collect();

            if (print && s.SolvedBoard != null)
            {
                var path = $@"C:\personal-projs\freecell-solver\_temp\{dealNum}\{method}";
                Directory.CreateDirectory(path);
                Console.WriteLine($"Printing moves to {path}");
                s.SolvedBoard.PrintMoves(path, b.Tableaus);
            }
        }

        static void RunBenchmarks(DfsSolveMethod method)
        {
            var sw = new Stopwatch();
            Console.WriteLine($"Processing extremly fast board");
            sw.Restart();
            PrintSummary(Dfs.Run(BoardExtensions.GetExtremlyFastBoard(), method), sw);
            GC.Collect();
        }

        static void PrintSummary(Dfs s, Stopwatch sw)
        {
            Console.WriteLine($"{(s.SolvedBoard != null ? "Done" : "Bailed")} in {sw.Elapsed} - initial id: {s.SolvedFromId} - visited nodes: {s.TotalVisitedNodes,0:n0}");
            Console.WriteLine();
        }
    }
}
