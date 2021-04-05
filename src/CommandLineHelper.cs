using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using FreeCellSolver.Game;
using FreeCellSolver.Solvers;
using FreeCellSolver.Game.Extensions;
using FreeCellSolver.Drawing.Extensions;
using McMaster.Extensions.CommandLineUtils;
using System.Globalization;
using System.Text;

namespace FreeCellSolver.Entry
{
    static class CommandLineHelper
    {
        static readonly Stopwatch _sw = new();

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
                            $"Executes solver against short (1500) or full (32000) deals",
                            CommandOptionType.SingleValue)
                        .Accepts(x => x.Values("short", "full"))
                        .IsRequired();

                    var optTag = benchmarksRunCmd
                        .Option<string>(
                            "-t|--tag <TAG>",
                            "Tags the result file",
                            CommandOptionType.SingleValue);

                    benchmarksRunCmd.OnExecute(() =>
                    {
                        var type = optType.ParsedValue.ToUpperInvariant();
                        var count = type == "SHORT" ? 1500 : type == "FULL" ? 32000 : -1;

                        RunBenchmarks(count, optTag.ParsedValue);
                        PrintBenchmarksSummary();

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
                    benchmarkShowCmd.OnExecute(() =>
                    {
                        PrintBenchmarksSummary();
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

                var optVisualize = runCmd
                    .Option<string>(
                        "-v|--visualize <PATH>",
                        "Outputs an html file to visualize the solution",
                        CommandOptionType.SingleValue)
                    .Accepts(x => x.ExistingDirectory());

                runCmd.OnExecute(() =>
                {
                    RunSingle(optDeal.ParsedValue, optVisualize.ParsedValue);
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

        static void RunBenchmarks(int count, string tag)
        {
            var logFile = count == 32000 ? "32k" : "1.5k";
            logFile += string.IsNullOrWhiteSpace(tag) ? $"-{DateTime.UtcNow.Ticks}" : $"-{tag}";

            var path = Path.Combine(Directory.GetCurrentDirectory(), "benchmarks", $"{logFile}.log");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            if (File.Exists(path))
            {
                Console.WriteLine("A log with the same tag name already exists. Are you sure you want to continue? [Y] [N]");
                if (Console.ReadKey(true).Key == ConsoleKey.N)
                {
                    Console.WriteLine("Operation cancelled.");
                    return;
                }
            }

            using var fs = File.CreateText(path);
            for (var i = 1; i <= count; i++)
            {
                Execute(fs, i, Board.FromDealNum(i), true);
            }
            return;
        }

        static void RunSingle(int dealNum, string visualizePath)
        {
            var b = Board.FromDealNum(dealNum);
            var result = Execute(Console.Out, dealNum, b, false);

            if (result.IsSolved)
            {
                Console.WriteLine("moves: " + string.Join("", result.GoalNode.GetMoves().Select(m => m.ToString())));

                if (!string.IsNullOrWhiteSpace(visualizePath))
                {
                    var path = Path.Combine(visualizePath, dealNum.ToString());
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(Path.Combine(path, "assets"));

                    WriteResource("FreeCellSolver.assets.bg.jpg", Path.Combine(path, "assets", "bg.jpg"));
                    WriteResource("FreeCellSolver.assets.empty.png", Path.Combine(path, "assets", "empty.png"));
                    WriteResource("FreeCellSolver.visualizer.dist.index.min.js", Path.Combine(path, "index.min.js"));
                    WriteResource("FreeCellSolver.visualizer.dist.visualizer.min.html", Path.Combine(path, "visualizer.html"));

                    for (var i = 0; i < 52; i++)
                    {
                        Card.Get(i).ToImage().Save(Path.Combine(path, "assets", $"{i}.png"));
                    }

                    var html = File.ReadAllText(Path.Combine(path, "visualizer.html"));
                    html = html.Replace("var board=[]", $"var board={b.AsJson()}");
                    html = html.Replace(",moves=[];", $"var moves={result.GoalNode.GetMoves().AsJson()}");
                    File.WriteAllText(Path.Combine(path, "visualizer.html"), html);

                    Console.WriteLine();
                    Console.WriteLine($"Visualizer written at '{path}'");
                }
            }
        }

        static void WriteResource(string name, string dest)
        {
            using var stream = typeof(CommandLineHelper).Assembly.GetManifestResourceStream(name);
            using var fs = File.Create(dest);
            stream.CopyTo(fs);
            fs.Close();
        }

        static Result Execute(TextWriter writer, int deal, Board b, bool writeToLog)
        {
            Console.Write($"Processing deal #{deal}");
            _sw.Restart();
            var result = AStar.Run(b);
            _sw.Stop();
            AStar.Reset();
            Console.WriteLine(". Done");
            writer.Write($"{(result.IsSolved ? "Done" : "Bailed")} in {(writeToLog ? _sw.ElapsedTicks.ToString("0000000000000") : _sw.Elapsed)} - threads: {result.Threads} - visited nodes: {result.VisitedNodes,0:n0}");
            writer.WriteLine(result.IsSolved ? $" - #moves: {result.GoalNode.MoveCount}" : " - #moves: 0");
            writer.Flush();
            return result;
        }

        public static void PrintBenchmarksSummary()
        {
            var lenOfVisitedNodes = "visited nodes: ".Length;
            var lenOfMoves = "#moves: ".Length;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "benchmarks");
            if (!Directory.Exists(path))
            {
                Console.WriteLine("No benchmark files found.");
                return;
            }

            var logFiles = Directory.GetFiles(path, "*.log").Select(f => new { Path = f, CreateDate = File.GetLastWriteTime(f) }).ToList();
            var tests = new List<(DateTime createDate, string name, TimeSpan ts, int total, int visited, int failed, double avgMoveCount)>(logFiles.Count);
            var len = logFiles.Select(f => Path.GetFileNameWithoutExtension(f.Path).Length).Max();

            foreach (var log in logFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(log.Path);
                var lines = ReadAllLines(log.Path, fileName.StartsWith('1') ? 1500 : 32000);

                var count = lines.Length;
                var failCount = 0;
                var ts = new TimeSpan();
                var nc = 0;
                var mc = 0;

                for (var l = 0; l < lines.Length; l++)
                {
                    var line = lines[l].AsSpan();
                    var firstChar = line[0];

                    failCount += firstChar == 'B' ? 1 : 0;
                    ts = ts.Add(TimeSpan.FromTicks(firstChar == 'D' ? int.Parse(line.Slice(8, 13)) : int.Parse(line.Slice(10, 13))));

                    var idxStart = line.IndexOf("visited nodes: ") + lenOfVisitedNodes;
                    var length = line[idxStart..].IndexOf(" - ");
                    nc += int.Parse(line.Slice(idxStart, length), NumberStyles.AllowThousands);

                    idxStart = line.IndexOf("#moves: ") + lenOfMoves;
                    mc += int.Parse(line[idxStart..]);
                }

                tests.Add((log.CreateDate, fileName, ts, count, nc, failCount, (double)mc / count));
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

        private static ReadOnlySpan<string> ReadAllLines(string path, int size)
        {
            string line;
            Span<string> lines = new string[size];

            var i = 0;
            using var sr = new StreamReader(path, Encoding.UTF8);
            while ((line = sr.ReadLine()) is not null)
            {
                lines[i++] = line;
            }

            return i < size ? lines.Slice(0, i) : lines;
        }
    }
}