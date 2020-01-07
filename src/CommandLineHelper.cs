using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using FreeCellSolver.Game;
using FreeCellSolver.Solvers.Shared;
using FreeCellSolver.Drawing.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace FreeCellSolver.Entry
{
    static class CommandLineHelper
    {
        static Stopwatch _sw = new Stopwatch();

        public static CommandLineApplication BuildCmdParser()
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

                    var optSolver = benchmarksRunCmd
                        .Option<SolverType>(
                            "-v|--solver <SOLVER>",
                            "Solver type (Required)",
                            CommandOptionType.SingleValue)
                        .IsRequired();

                    var optType = benchmarksRunCmd
                        .Option<string>(
                            "-p|--type <TYPE>",
                            $"Executes solver against short (1500) or full (32000) deals{Environment.NewLine}Allowed values are: short, full",
                            CommandOptionType.SingleValue)
                        .Accepts(x => x.Values("short", "full"))
                        .IsRequired();

                    var optTag = benchmarksRunCmd
                        .Option<string>(
                            "-t|--tag <TAG>",
                            "Tags the result file",
                            CommandOptionType.SingleValue);

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

                    benchmarksRunCmd.OnValidationError(r =>
                    {
                        benchmarksRunCmd.ShowHelp();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine(r.ErrorMessage);
                        Console.ResetColor();
                        return 1;
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

                benchmarksCmd.OnExecute(() =>
                {
                    benchmarksCmd.ShowHelp();
                    return 1;
                });
            });

            app.Command("run", runCmd =>
            {
                runCmd.Description = "Runs the solver";

                var optSolver = runCmd
                    .Option<SolverType>(
                        "-v|--solver <SOLVER>",
                        "Solver type (Required)",
                        CommandOptionType.SingleValue)
                    .IsRequired();

                var optDeal = runCmd
                    .Option<int>(
                        "-d|--deal <NUM>",
                        "Deal number to solve",
                        CommandOptionType.SingleValue)
                    .Accepts(n => n.Range(1, Int32.MaxValue));

                var optBest = runCmd
                    .Option<bool>(
                        "-b|--best",
                        "Attempts to find a solution with the least amount of moves. Applicable only to 'AStar' solver.",
                        CommandOptionType.NoValue);

                var optPrint = runCmd
                    .Option<string>(
                        "-p|--print <PATH>",
                        "Prints moves images to specified path",
                        CommandOptionType.SingleValue)
                    .Accepts(x => x.ExistingDirectory());

                runCmd.OnExecuteAsync(async (_) =>
                {
                    await RunSingleAsync(optSolver.ParsedValue, optDeal.ParsedValue, optBest.HasValue(), optPrint.ParsedValue);
                    return 0;
                });

                runCmd.OnValidationError(r =>
                {
                    runCmd.ShowHelp();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(r.ErrorMessage);
                    Console.ResetColor();
                    return 1;
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
            for (var i = 1; i <= count; i++)
            {
                await ExecuteAsync(fs, solverType, i);
            }
            return;
        }

        static async Task RunSingleAsync(SolverType solverType, int dealNum, bool best, string printPath)
        {
            var sw = new Stopwatch();
            var b = Board.FromDealNum(dealNum);

            var s = await ExecuteAsync(Console.Out, solverType, dealNum);

            if (!String.IsNullOrWhiteSpace(printPath) && s.SolvedBoard != null)
            {
                var path = Path.Combine(printPath, dealNum.ToString());
                Console.WriteLine($"Printing solved board into {path}...");

                Directory.CreateDirectory(path);
                Console.WriteLine($"Printing moves to {path}");
                s.SolvedBoard.PrintMoves(path);
            }
        }

        static async Task<ISolver> ExecuteAsync(TextWriter writer, SolverType solverType, int deal)
        {
            writer.WriteLine($"Processing deal #{deal}");
            _sw.Restart();
            var solver = await Solver.RunParallelAsync(solverType, Board.FromDealNum(deal));
            _sw.Stop();
            writer.Write($"{(solver.SolvedBoard != null ? "Done" : "Bailed")} in {_sw.Elapsed} - initial id: {solver.SolvedFromId} - visited nodes: {solver.VisitedNodes,0:n0}");
            writer.WriteLine(solver.SolvedBoard != null ? $" - #moves: {solver.SolvedBoard.GetMoves().Count()}" : " - #moves: 0");
            await writer.FlushAsync();
            GC.Collect();
            return solver;
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