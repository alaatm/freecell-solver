using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FreeCellSolver.Game.Extensions
{
    public static class BoardExtensions
    {
        [SkipLocalsInit]
        public static unsafe Tableau[] FromDealNum(int dealNum)
        {
            Span<byte> crv = stackalloc byte[]
            {/*
              * SS  HH  DD  CC  */
                55, 52, 54, 53, // RK
                51, 48, 50, 49, // RQ
                47, 44, 46, 45, // RJ
                43, 40, 42, 41, // RT
                39, 36, 38, 37, // R9
                35, 32, 34, 33, // R8
                31, 28, 30, 29, // R7
                27, 24, 26, 25, // R6
                23, 20, 22, 21, // R5
                19, 16, 18, 17, // R4
                15, 12, 14, 13, // R3
                11, 08, 10, 09, // R2
                07, 04, 06, 05, // RA
                // The non sequential numbers in the array is because the original ms algorithm
                // uses Clubs to 0, Diamonds to 1, Hearts to 2 and Spades to 3 but in our solution
                // we set Hearts to 0, Clubs to 1, Diamonds to 2 and Spades to 3.
                // Also, since Ace is now equal to 1 instead of 0 so we start from 4 instead of 0.
            };

            var seed = dealNum;

            for (var i = 0; i < crv.Length; i++)
            {
                seed = ((seed * 214013) + 2531011) & int.MaxValue;
                var pos = 51 - ((seed >> 16) % (52 - i));

                var tmp = crv[i];
                crv[i] = crv[pos];
                crv[pos] = tmp;
            }

            var col = 0;
            var tableaus = new Tableau[8];
            Span<byte> cards = stackalloc byte[8];

            for (var i = 0; i < crv.Length;)
            {
                var count = 6;
                cards[0] = crv[col + 0];
                cards[1] = crv[col + 8];
                cards[2] = crv[col + 16];
                cards[3] = crv[col + 24];
                cards[4] = crv[col + 32];
                cards[5] = crv[col + 40];
                if (col < 4)
                {
                    cards[6] = crv[col + 48];
                    count = 7;
                }

                tableaus[col++] = Tableau.Create(cards.Slice(0, count));
                i += count;
            }

            return tableaus;
        }

        public static bool IsValid(this Board board)
        {
            var isValid = true;

            var allCards = Enumerable.Range(0, 52).Select(c => Card.Get(c));

            var boardCards = board.AllCards.ToList();
            var uniqueCards = new HashSet<Card>(board.AllCards);

            if (uniqueCards.Count != 52)
            {
                var missing = string.Join(", ", allCards.Except(uniqueCards).Select(c => $"'{c}'"));
                Console.Error.WriteLine($"Invalid card count, should be '52' but found '{uniqueCards.Count}' cards.");
                Console.Error.WriteLine($"The following card(s) are missing: {missing}");
                isValid = false;
            }
            else if (boardCards.Count != 52)
            {
                var duplicates = string.Join(", ", boardCards.GroupBy(x => x.RawValue).Where(g => g.Count() > 1).Select(g => $"'{Card.Get(g.Key)}'"));
                Console.Error.WriteLine($"Invalid card count, should be '52' but found '{boardCards.Count}' cards.");
                Console.Error.WriteLine($"The following card(s) are duplicates: {duplicates}");
                isValid = false;
            }

            return isValid;
        }

        public static void Traverse(this Board board, Action<Board> visit)
        {
            var prev = board;

            while (prev is not null)
            {
                visit(prev);
                prev = prev.Prev;
            }
        }

        public static string AsJson(this Board board)
        {
            var json = new StringBuilder();
            json.Append('[');
            for (var i = 0; i < 8; i++)
            {
                var t = board.Tableaus[i];
                json.Append('[');
                for (var j = t.Size - 1; j >= 0; j--)
                {
                    json.Append($"{t[t.Size - j - 1].RawValue},");
                }
                json.Append("],");
            }
            json.Append("];");

            return json.ToString();
        }

        public static void EmitCSharpCode(this Board board, TextWriter writer)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine(board.ToString());
            sb.AppendLine("*/");
            sb.AppendLine();

            var rSb = new StringBuilder();
            rSb.Append("\tReserve.Create(");
            for (var i = 0; i < 4; i++)
            {
                var c = board.Reserve[i];
                rSb.Append(c != Card.Null ? $"\"{c}\"" : "null");
                if (i < 3)
                {
                    rSb.Append(", ");
                }
            }
            rSb.AppendLine("),");

            var fSb = new StringBuilder();
            fSb.Append("\tFoundation.Create(");
            for (var i = 0; i < 4; i++)
            {
                var c = board.Foundation[i];
                fSb.Append(c != Ranks.Nil ? $"Ranks.{GetRank(c)}" : "Ranks.Nil");
                if (i < 3)
                {
                    fSb.Append(", ");
                }
            }
            fSb.AppendLine("),");

            var tsSb = new StringBuilder();
            for (var i = 0; i < 8; i++)
            {
                tsSb.Append("\tTableau.Create(\"");
                var t = board.Tableaus[i];
                for (var j = 0; j < t.Size; j++)
                {
                    tsSb.Append(t[j]);
                    if (j < t.Size - 1)
                    {
                        tsSb.Append(' ');
                    }
                }
                tsSb.AppendLine(i < 7 ? "\")," : "\")");
            }

            sb.AppendLine("var b = Board.Create(");
            sb.Append(rSb);
            sb.Append(fSb);
            sb.Append(tsSb);
            sb.AppendLine(");");
            sb.Append("if (!b.IsValid()) { throw new Exception(); }");

            writer.Write(sb.ToString()
                .Replace("(null, null, null, null)", "()")
                .Replace(", null, null, null)", ")")
                .Replace(", null, null)", ")")
                .Replace(", null)", ")")
                .Replace("(\"\")", "()"));

            static string GetRank(int c) => c switch
            {
                Ranks.Ace => "Ace",
                Ranks.R2 => "R2",
                Ranks.R3 => "R3",
                Ranks.R4 => "R4",
                Ranks.R5 => "R5",
                Ranks.R6 => "R6",
                Ranks.R7 => "R7",
                Ranks.R8 => "R8",
                Ranks.R9 => "R9",
                Ranks.R10 => "R10",
                Ranks.Rj => "Rj",
                Ranks.Rq => "Rq",
                Ranks.Rk => "Rk",
                _ => throw new IndexOutOfRangeException(),
            };
        }
    }
}