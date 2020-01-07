using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using FreeCellSolver.Game;
using FreeCellSolver.Solvers;
using FreeCellSolver.Game.Extensions;

namespace FreeCellSolver
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                using var app = BuildCmdParser();
                return app.Execute(args);
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

            app.Command("benchmarks", benchmarksCmd =>
            {
                benchmarksCmd.Description = "Executes or shows benchmarks";
                benchmarksCmd.Command("run", benchmarksRunCmd =>
                {
                    benchmarksRunCmd.Description = "Executes benchmarks";
                    var optSolver = benchmarksRunCmd.Option<SolverType>("-v|--solver <SOLVER>", "Solver type (Required)", CommandOptionType.SingleValue).IsRequired();
                    var optType = benchmarksRunCmd.Option<string>("-p|--type <TYPE>", $"Executes solver against short (1500) or full (32000) deals{Environment.NewLine}Allowed values are: short, full", CommandOptionType.SingleValue).Accepts(x => x.Values("short", "full"));
                    var optTag = benchmarksRunCmd.Option<string>("-t|--tag <TAG>", "Tags the result file", CommandOptionType.SingleValue);
                    benchmarksRunCmd.OnExecuteAsync(async (_) =>
                    {
                        if (optType.ParsedValue.ToLower() == "short")
                        {
                            await RunBenchmarksAsync(optSolver.ParsedValue, 1500, optTag.ParsedValue);
                            await PrintBenchmarksSummaryAsync();
                        }
                        else if (optType.ParsedValue.ToLower() == "full")
                        {
                            await RunBenchmarksAsync(optSolver.ParsedValue, 32000, optTag.ParsedValue);
                            await PrintBenchmarksSummaryAsync();
                        }
                        else
                        {
                            throw new Exception("??");
                        }

                        return 0;
                    });
                });

                benchmarksCmd.Command("show", benchmarkShowCmd =>
                {
                    benchmarkShowCmd.Description = "Shows past benchmarks results";
                    benchmarkShowCmd.OnExecuteAsync(async (_) =>
                    {
                        await PrintBenchmarksSummaryAsync();
                        return 0;
                    });
                });
            });

            app.Command("run", runCmd =>
            {
                runCmd.Description = "Runs the solver";
                var optSolver = runCmd.Option<SolverType>("-v|--solver <SOLVER>", "Solver type (Required)", CommandOptionType.SingleValue).IsRequired();
                var optDeal = runCmd.Option<int>("-d|--deal <NUM>", "Deal number to solve", CommandOptionType.SingleValue).Accepts(n => n.Range(1, Int32.MaxValue));
                var optBest = runCmd.Option<bool>("-b|--best", "Attempts to find a solution with the least amount of moves. Applicable only to 'AStar' solver.", CommandOptionType.NoValue);
                var optPrint = runCmd.Option<string>("-p|--print <PATH>", "Prints moves images to specified path", CommandOptionType.SingleValue).Accepts(x => x.ExistingDirectory());

                runCmd.OnExecuteAsync(async (_) =>
                {
                    await RunSingleAsync(optSolver.ParsedValue, optDeal.ParsedValue, optBest.HasValue(), optPrint.ParsedValue);
                    return 0;
                });
            });

            return app;
        }

        static async Task RunBenchmarksAsync(SolverType solverType, int count, string tag)
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

        static async Task RunSingleAsync(SolverType solverType, int dealNum, bool best, string printPath)
        {
            var sw = new Stopwatch();
            var b = Board.FromDealNum(dealNum);

            Console.WriteLine($"Processing deal #{dealNum}");
            sw.Restart();
            var s = await Solver.RunParallelAsync(solverType, b, best);
            PrintSummary(Console.Out, s, sw);
            GC.Collect();

            if (!String.IsNullOrWhiteSpace(printPath) && s.SolvedBoard != null)
            {
                var path = Path.Combine(printPath, dealNum.ToString());
                Console.WriteLine($"Printing solved board into {path}...");

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
                    nc += Int32.Parse(lines[l].Substring(idxStart, length).Replace(",", ""));

                    idxStart = lines[l].IndexOf("#moves: ") + "#moves: ".Length;
                    mc += Int32.Parse(lines[l].Substring(idxStart));
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
