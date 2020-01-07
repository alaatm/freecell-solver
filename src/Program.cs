using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using FreeCellSolver.Solvers;
using FreeCellSolver.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace FreeCellSolver
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                using (var app = BuildCmdParser())
                {
                    return app.Execute(args);
                }
            }
            catch (CommandParsingException ex)
            {
                Console.Error.WriteLine(ex.Message);

                if (ex is UnrecognizedCommandParsingException uex && uex.NearestMatches.Any())
                {
                    Console.Error.WriteLine($"Did you mean '{uex.NearestMatches.First()}'?");
                }

                return 1;
            }
        }

        private static CommandLineApplication BuildCmdParser()
        {
            var app = new CommandLineApplication()
            {
                Name = "fc-solve",
                Description = "Freecell Solver",
            };
            app.HelpOption(inherited: true);

            app.Command("run", runCmd =>
            {
                runCmd.Description = "Runs the solver";
                var optSolver = runCmd.Option<SolverType>("-v|--solver <solver>", "Solver type (Required)", CommandOptionType.SingleValue).IsRequired();
                var optDeal = runCmd.Option<int>("-d|--deal <number>", "Deal number to solve", CommandOptionType.SingleValue).Accepts(n => n.Range(1, int.MaxValue));
                var optShort = runCmd.Option("-s|--short", "Solves 4 MS freecell deals", CommandOptionType.NoValue);
                var optFull = runCmd.Option("-f|--full", "Solves the first 1500 MS freecell deals", CommandOptionType.NoValue);
                var optAll = runCmd.Option("-a|--all", "Solves all 32000 MS freecell deals", CommandOptionType.NoValue);
                var optTag = runCmd.Option<string>("-t|--tag <tag>", "Tags the result file", CommandOptionType.SingleValue);

                runCmd.OnExecuteAsync(async (_) =>
                {
                    var opts = new[] { optDeal.HasValue(), optShort.HasValue(), optFull.HasValue(), optAll.HasValue() };
                    if (opts.Count(o => o == true) > 1)
                    {
                        Console.Error.WriteLine("Only one option maybe specified for this command: -d, -s, -f or -a");
                        return 1;
                    }
                    else if (opts.Count(o => o == true) == 0)
                    {
                        Console.Error.WriteLine("One of the following options must be specified for this command: -d, -f, -s or -a.");
                        return 1;
                    }

                    if (optDeal.HasValue())
                    {
                        await RunSingleBenchmarksAsync(optSolver.ParsedValue, optDeal.ParsedValue);
                    }
                    else if (optShort.HasValue())
                    {
                        await RunShortBenchmarksAsync(optSolver.ParsedValue);
                    }
                    else if (optFull.HasValue())
                    {
                        await RunFullBenchmarksAsync(optSolver.ParsedValue, 1500, optTag.ParsedValue);
                        await PrintBenchmarksSummaryAsync();
                    }
                    else if (optAll.HasValue())
                    {
                        await RunFullBenchmarksAsync(optSolver.ParsedValue, 32000, optTag.ParsedValue);
                        await PrintBenchmarksSummaryAsync();
                    }
                    else
                    {
                        throw new Exception("??");
                    }

                    return 0;
                });
            });

            app.Command("print-summary", printSummaryCmd =>
            {
                printSummaryCmd.Description = "Prints summary from log files";
                printSummaryCmd.OnExecuteAsync(async (_) =>
                {
                    await PrintBenchmarksSummaryAsync();
                    return 0;
                });
            });

            return app;
        }

        static async Task RunFullBenchmarksAsync(SolverType solverType, int count, string tag)
        {
            var logFile = solverType.ToString().ToLower();
            logFile += count == 32000 ? "-32k" : "";
            logFile += String.IsNullOrWhiteSpace(tag) ? $"-{DateTime.UtcNow.Ticks}" : $"-{tag}";

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
                fs.WriteLine($"Processing deal #{i}");
                sw.Restart();
                PrintSummary(fs, await Solver.RunParallelAsync(solverType, Board.FromDealNum(i)), sw);
                await fs.FlushAsync();
                GC.Collect();
            }
            return;
        }

        static async Task RunShortBenchmarksAsync(SolverType solverType)
        {
            var sw = new Stopwatch();

            foreach (var deal in new[] { 169, 178, 231, 261 })
            {
                Console.WriteLine($"Processing Deal #{deal}");
                sw.Restart();
                PrintSummary(Console.Out, await Solver.RunParallelAsync(solverType, Board.FromDealNum(deal)), sw);
                GC.Collect();
            }
        }

        static async Task RunSingleBenchmarksAsync(SolverType solverType, int dealNum, bool print = false)
        {
            var sw = new Stopwatch();
            var b = Board.FromDealNum(dealNum);

            Console.WriteLine($"Processing deal #{dealNum}");
            sw.Restart();
            var s = await Solver.RunParallelAsync(solverType, b);
            PrintSummary(Console.Out, s, sw);
            GC.Collect();

            if (print && s.SolvedBoard != null)
            {
                var path = $@"C:\personal-projs\freecell-solver\_temp\{dealNum}";
                Directory.CreateDirectory(path);
                Console.WriteLine($"Printing moves to {path}");
                s.SolvedBoard.PrintMoves(path);
            }
        }

        static void Debug(SolverType solverType, int dealNum)
        {
            var sw = new Stopwatch();
            Console.WriteLine($"Processing deal #{dealNum}");
            sw.Restart();
            PrintSummary(Console.Out, Solver.Run(solverType, Board.FromDealNum(dealNum)), sw);
            GC.Collect();
        }

        static void PrintSummary(TextWriter writer, ISolver s, Stopwatch sw)
        {
            writer.Write($"{(s.SolvedBoard != null ? "Done" : "Bailed")} in {sw.Elapsed} - initial id: {s.SolvedFromId} - visited nodes: {s.VisitedNodes,0:n0}");
            writer.WriteLine(s.SolvedBoard != null ? $" - #moves: {s.SolvedBoard.GetMoves().Count()}" : " - #moves: 0");
        }

        static async Task PrintBenchmarksSummaryAsync()
        {
            var tests = new List<(DateTime createDate, string name, TimeSpan ts, int total, int visited, int failed, double avgMoveCount)>();
            var logFiles = Directory.GetFiles($@"C:\personal-projs\freecell-solver\benchmarks", "*.log").Select(f => new { Path = f, CreateDate = File.GetLastWriteTime(f) });
            var len = logFiles.Select(f => Path.GetFileNameWithoutExtension(f.Path).Length).Max();

            foreach (var log in logFiles)
            {
                var lines = await File.ReadAllLinesAsync(log.Path);

                var count = lines.Length / 2;
                var failed = lines.Count(p => p.IndexOf("Bailed") >= 0);
                var ts = new TimeSpan();
                var nc = 0;
                var mc = 0;

                for (var l = 1; l < lines.Length; l += 2)
                {
                    var idxStart = lines[l].IndexOf("in ") + "in ".Length;
                    var length = lines[l].IndexOf(" - ", idxStart) - idxStart;
                    ts = ts.Add(TimeSpan.Parse(lines[l].Substring(idxStart, length)));

                    idxStart = lines[l].IndexOf("visited nodes: ") + "visited nodes: ".Length;
                    length = lines[l].IndexOf(" - ", idxStart) - idxStart;
                    nc += int.Parse(lines[l].Substring(idxStart, length).Replace(",", ""));

                    idxStart = lines[l].IndexOf("#moves: ") + "#moves: ".Length;
                    mc += int.Parse(lines[l].Substring(idxStart));
                }

                tests.Add((log.CreateDate, Path.GetFileNameWithoutExtension(log.Path), ts, count, nc, failed, (double)mc / count));
            }

            var maxLenName = tests.Select(p => p.name.Length).Max() + 1;
            var maxLenVisited = tests.Select(p => p.visited.ToString("n0").Length).Max();
            var maxLenTotal = tests.Select(p => p.total.ToString("n0").Length).Max();
            var maxLenFailed = tests.Select(p => p.failed.ToString("n0").Length).Max();

            foreach (var (createDate, name, ts, total, visited, failed, avgMoveCount) in tests.OrderBy(p => p.createDate))
            {
                var d = createDate.ToString("dd-MM-yy HH:mm:ss");
                var n = name.PadRight(maxLenName);
                var v = visited.ToString("n0").PadLeft(maxLenVisited);
                var c = total.ToString("n0").PadLeft(maxLenTotal);
                var f = failed.ToString("n0").PadLeft(maxLenFailed);

                Console.WriteLine($"{d} - {n}: {ts} - visited: {v} - total: {c} - failed: {f} - avg move count: {Math.Round(avgMoveCount, 4)}");
            }
        }
    }
}
