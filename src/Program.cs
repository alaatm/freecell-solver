using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using FreeCellSolver.Extensions;
using SkiaSharp;

namespace FreeCellSolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            Console.WriteLine($"Processing started");

            var sw = new Stopwatch();
            sw.Restart();
            var b = await ParallelSolver.SolveAsync(BoardExtensions.GetSlowBoard());
            var t = sw.Elapsed;

            Console.WriteLine($"{(b != null ? "Done" : "Bailed")} in {t}");

            // var cards = Deck.Random();
            // var tableaus = new List<Tableau>();
            // for (var i = 0; i < 8; i++)
            // {
            //     tableaus.Add(new Tableau(i, cards.Skip(i < 4 ? i * 7 : i * 6).Take(i < 4 ? 7 : 6).ToList()));
            // }
            // Solver.Solve(new Board(new Deal(tableaus)));
        }
    }
}
