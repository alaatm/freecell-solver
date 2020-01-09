using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FreeCellSolver.Drawing.Extensions;
using FreeCellSolver.Game;
using FreeCellSolver.Solvers;
using McMaster.Extensions.CommandLineUtils;

namespace FreeCellSolver.Entry
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                using var app = CommandLineHelper.BuildCmdParser();
                return app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);

                if (ex is UnrecognizedCommandParsingException uex && uex.NearestMatches.Any())
                {
                    Console.Error.WriteLine($"Did you mean '{uex.NearestMatches.First()}'?");
                }
                Console.ResetColor();

                return 1;
            }
        }
    }
}
