using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using FreeCellSolver.Solvers;
using FreeCellSolver.Extensions;

namespace FreeCellSolver
{
    class Program
    {
        static async Task /*void*/ Main(string[] args)
        {
            await ProcessArgsAsync(args);
        }

        static async Task ProcessArgsAsync(string[] args)
        {
            if (args.Length > 0)
            {
                var arg = args[0];
                if (arg == "--32k")
                {
                    await RunFullBenchmarksAsync(32000, "");
                }
                else if (arg == "--full")
                {
                    var tag = args.Length == 3 && (args[1] == "-t" || args[1] == "--tag") ? args[2] : "current";

                    await RunFullBenchmarksAsync(1500, tag);
                    await PrintBenchmarksSummaryAsync();
                }
                else if (arg == "--short")
                {
                    await RunShortBenchmarksAsync();
                }
                else if (arg == "--single")
                {
                    await RunSingleBenchmarksAsync();
                }
                else if (arg == "--deal")
                {
                    var dealNum = int.Parse(args[1]);
                    var print = args.Length > 2 && args[2] == "--print";
                    await RunSingleBenchmarksAsync(dealNum, print);
                }
                else if (arg == "--print-summary")
                {
                    await PrintBenchmarksSummaryAsync();
                }
            }
            else
            {
                RunBenchmarks();
            }
        }

        static async Task RunFullBenchmarksAsync(int count, string tag)
        {
            var logFile = "dfs";
            logFile += count == 32000 ? "-32k" : "";
            logFile += String.IsNullOrWhiteSpace(tag) ? "" : $"-{tag}";

            var path = $@"C:\personal-projs\freecell-solver\benchmarks\{logFile}.log";

            if (File.Exists(path))
            {
                Console.WriteLine("A log with the same tag name already exists. Are you sure you want to continue? [Y] [N]");
                var answer = Console.ReadKey(true);
                if (answer.Key == ConsoleKey.N)
                {
                    Console.WriteLine("Operation cancelled.");
                    return;
                }
            }

            using var fs = File.CreateText(path);
            var sw = new Stopwatch();
            for (var i = 1; i <= count; i++)
            {
                fs.WriteLine($"Attempting deal #{i}");
                sw.Restart();
                var s = await Dfs.RunParallelAsync(Board.FromDealNum(i));
                fs.WriteLine($"{(s.SolvedBoard != null ? "Done" : "Bailed")} in {sw.Elapsed} - visited nodes: {s.VisitedNodes,0:n0}");
                await fs.FlushAsync();
                GC.Collect();
            }
            return;
        }

        static async Task RunShortBenchmarksAsync()
        {
            var sw = new Stopwatch();

            Console.WriteLine($"Processing Deal #169 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(Board.FromDealNum(169)), sw);
            GC.Collect();

            Console.WriteLine($"Processing Deal #178 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(Board.FromDealNum(178)), sw);
            GC.Collect();

            Console.WriteLine($"Processing Deal #231 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(Board.FromDealNum(231)), sw);
            GC.Collect();

            Console.WriteLine($"Processing Deal #261 board");
            sw.Restart();
            PrintSummary(await Dfs.RunParallelAsync(Board.FromDealNum(261)), sw);
            GC.Collect();
        }

        static async Task RunSingleBenchmarksAsync(int dealNum = 169, bool print = false)
        {
            var sw = new Stopwatch();
            var b = Board.FromDealNum(dealNum);

            Console.WriteLine($"Processing Deal #{dealNum} board");
            sw.Restart();
            var s = await Dfs.RunParallelAsync(b);
            PrintSummary(s, sw);
            GC.Collect();

            if (print && s.SolvedBoard != null)
            {
                var path = $@"C:\personal-projs\freecell-solver\_temp\{dealNum}";
                Directory.CreateDirectory(path);
                Console.WriteLine($"Printing moves to {path}");
                s.SolvedBoard.PrintMoves(path);
            }
        }

        static void RunBenchmarks()
        {
            var sw = new Stopwatch();
            Console.WriteLine($"Processing Deal #1566");
            sw.Restart();
            PrintSummary(Dfs.Run(Board.FromDealNum(1566)), sw);
            GC.Collect();
        }

        static void PrintSummary(Dfs s, Stopwatch sw)
        {
            Console.Write($"{(s.SolvedBoard != null ? "Done" : "Bailed")} in {sw.Elapsed} - initial id: {s.SolvedFromId} - visited nodes: {s.VisitedNodes,0:n0}");
            Console.WriteLine(s.SolvedBoard != null ? $" - #moves: {s.SolvedBoard.GetMoves().Count()}" : "");
            Console.WriteLine();
        }

        static async Task PrintBenchmarksSummaryAsync()
        {
            var tests = new List<(DateTime createDate, string name, TimeSpan ts, int total, int visited, int failed)>();
            var logFiles = Directory.GetFiles($@"C:\personal-projs\freecell-solver\benchmarks", "*.log").Select(f => new { Path = f, CreateDate = File.GetLastWriteTime(f) });
            var len = logFiles.Select(f => Path.GetFileNameWithoutExtension(f.Path).Length).Max();

            foreach (var log in logFiles)
            {
                var lines = await File.ReadAllLinesAsync(log.Path);

                var count = lines.Length / 2;
                var failed = lines.Count(p => p.IndexOf("Bailed") >= 0);
                var ts = new TimeSpan();
                var nc = 0;

                for (var l = 1; l < lines.Length; l += 2)
                {
                    var idxStart = lines[l].IndexOf("in ") + "in ".Length;
                    var length = lines[l].IndexOf(" - ", idxStart) - idxStart;
                    ts = ts.Add(TimeSpan.Parse(lines[l].Substring(idxStart, length)));

                    idxStart = lines[l].IndexOf("visited nodes: ") + "visited nodes: ".Length;
                    nc += int.Parse(lines[l].Substring(idxStart).Replace(",", ""));
                }

                tests.Add((log.CreateDate, Path.GetFileNameWithoutExtension(log.Path), ts, count, nc, failed));
            }

            var maxLenName = tests.Select(p => p.name.Length).Max() + 1;
            var maxLenVisited = tests.Select(p => p.visited.ToString("n0").Length).Max();
            var maxLenTotal = tests.Select(p => p.total.ToString("n0").Length).Max();
            var maxLenFailed = tests.Select(p => p.failed.ToString("n0").Length).Max();

            foreach (var (createDate, name, ts, total, visited, failed) in tests.OrderBy(p => p.createDate))
            {
                var d = createDate.ToString("dd-MM-yy HH:mm:ss");
                var n = name.PadRight(maxLenName);
                var v = visited.ToString("n0").PadLeft(maxLenVisited);
                var c = total.ToString("n0").PadLeft(maxLenTotal);
                var f = failed.ToString("n0").PadLeft(maxLenFailed);

                Console.WriteLine($"{d} - {n}: {ts} - visited: {v} - total: {c} - failed: {f}");
            }
        }
    }
}
