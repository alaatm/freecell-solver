using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using FreeCellSolver.Game;
using FreeCellSolver.Game.Extensions;
using FreeCellSolver.Drawing.Extensions;
using McMaster.Extensions.CommandLineUtils;
using FreeCellSolver.Solvers;

namespace FreeCellSolver.Entry
{
    static class CommandLineHelper
    {
        static readonly Stopwatch _sw = new Stopwatch();

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
                        if (optType.ParsedValue.ToUpperInvariant() == "SHORT")
                        {
                            await RunBenchmarksAsync(1500, optTag.ParsedValue).ConfigureAwait(false);
                            await PrintBenchmarksSummaryAsync().ConfigureAwait(false);
                        }
                        else if (optType.ParsedValue.ToUpperInvariant() == "FULL")
                        {
                            await RunBenchmarksAsync(32000, optTag.ParsedValue).ConfigureAwait(false);
                            await PrintBenchmarksSummaryAsync().ConfigureAwait(false);
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
                        await PrintBenchmarksSummaryAsync().ConfigureAwait(false);
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

                var optDeal = runCmd
                    .Option<int>(
                        "-d|--deal <NUM>",
                        "Deal number to solve",
                        CommandOptionType.SingleValue)
                    .Accepts(n => n.Range(1, int.MaxValue));

                var optBest = runCmd
                    .Option<bool>(
                        "-b|--best",
                        "Attempts to find a solution with the least amount of manual moves.",
                        CommandOptionType.NoValue);

                var optVisualize = runCmd
                    .Option<string>(
                        "-v|--visualize <PATH>",
                        "Outputs an html file to visualize the solution",
                        CommandOptionType.SingleValue)
                    .Accepts(x => x.ExistingDirectory());

                runCmd.OnExecuteAsync(async (_) =>
                {
                    await RunSingleAsync(optDeal.ParsedValue, optBest.HasValue(), optVisualize.ParsedValue).ConfigureAwait(false);
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

        static async Task RunBenchmarksAsync(int count, string tag)
        {
            var logFile = count == 32000 ? "-32k" : "-1.5k";
            logFile += string.IsNullOrWhiteSpace(tag) ? $"-{DateTime.UtcNow.Ticks}" : $"-{tag}";

            var path = Path.Combine(Directory.GetCurrentDirectory(), "benchmarks", $"{logFile}.log");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

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
                await ExecuteAsync(fs, i, false).ConfigureAwait(false);
            }
            return;
        }

        static async Task RunSingleAsync(int dealNum, bool best, string visualizePath)
        {
            var sw = new Stopwatch();
            var b = Board.FromDealNum(dealNum);

            var s = await ExecuteAsync(Console.Out, dealNum, best).ConfigureAwait(false);

            if (s.SolvedBoard != null)
            {
                Console.WriteLine("moves: " + string.Join("", s.SolvedBoard.GetMoves().Select(m => m.ToString())));

                if (!string.IsNullOrWhiteSpace(visualizePath))
                {
                    var path = Path.Combine(visualizePath, dealNum.ToString());
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(Path.Combine(path, "assets"));

                    await WriteResourceAsync("FreeCellSolver.assets.bg.jpg", Path.Combine(path, "assets", "bg.jpg")).ConfigureAwait(false);
                    await WriteResourceAsync("FreeCellSolver.assets.empty.png", Path.Combine(path, "assets", "empty.png")).ConfigureAwait(false);
                    await WriteResourceAsync("FreeCellSolver.visualizer.dist.index.min.js", Path.Combine(path, "index.min.js")).ConfigureAwait(false);
                    await WriteResourceAsync("FreeCellSolver.visualizer.dist.visualizer.min.html", Path.Combine(path, "visualizer.html")).ConfigureAwait(false);

                    for (sbyte i = 0; i < 52; i++)
                    {
                        Card.Get(i).ToImage().Save(Path.Combine(path, "assets", $"{i}.png"));
                    }

                    var html = await File.ReadAllTextAsync(Path.Combine(path, "visualizer.html")).ConfigureAwait(false);
                    html = html.Replace("var board=[]", $"var board={b.AsJson()}");
                    html = html.Replace(",moves=[];", $"var moves={s.SolvedBoard.GetMoves().AsJson()}");
                    await File.WriteAllTextAsync(Path.Combine(path, "visualizer.html"), html).ConfigureAwait(false);

                    Console.WriteLine();
                    Console.WriteLine($"Visualizer written at '{path}'");
                }
            }
        }

        static async Task WriteResourceAsync(string name, string dest)
        {
            using var stream = typeof(CommandLineHelper).Assembly.GetManifestResourceStream(name);
            using var fs = File.Create(dest);
            await stream.CopyToAsync(fs).ConfigureAwait(false);
            fs.Close();
        }

        static async Task<AStar> ExecuteAsync(TextWriter writer, int deal, bool best)
        {
            if (writer != Console.Out)
            {
                Console.WriteLine($"Processing deal #{deal}");
            }
            writer.WriteLine($"Processing deal #{deal}");
            _sw.Restart();
            var solver = await AStar.RunParallelAsync(Board.FromDealNum(deal), best).ConfigureAwait(false);
            _sw.Stop();
            writer.Write($"{(solver.SolvedBoard != null ? "Done" : "Bailed")} in {_sw.Elapsed} - threads: {solver.Threads,0:n0} - initial id: {solver.SolvedFromId} - visited nodes: {solver.VisitedNodes,0:n0}");
            writer.WriteLine(solver.SolvedBoard != null ? $" - #moves: {solver.SolvedBoard.MoveCount}" : " - #moves: 0");
            await writer.FlushAsync().ConfigureAwait(false);
            GC.Collect();
            return solver;
        }

        static async Task PrintBenchmarksSummaryAsync()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "benchmarks");
            if (!Directory.Exists(path))
            {
                Console.WriteLine("No benchmark files found.");
                return;
            }

            var tests = new List<(DateTime createDate, string name, TimeSpan ts, int total, int visited, int failed, double avgMoveCount)>();
            var logFiles = Directory.GetFiles(path, "*.log").Select(f => new { Path = f, CreateDate = File.GetLastWriteTime(f) });
            var len = logFiles.Select(f => Path.GetFileNameWithoutExtension(f.Path).Length).Max();

            foreach (var log in logFiles)
            {
                var lines = await File.ReadAllLinesAsync(log.Path).ConfigureAwait(false);

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
