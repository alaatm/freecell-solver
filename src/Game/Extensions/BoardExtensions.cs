using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace FreeCellSolver.Game.Extensions
{
    public static class BoardExtensions
    {
        public static Board FromDealNum(int dealNum)
        {
            Span<byte> crv = stackalloc byte[] 
            {/*
              * SS  HH  DD  CC  */
                51, 50, 49, 48, // RK
                47, 46, 45, 44, // RQ
                43, 42, 41, 40, // RJ
                39, 38, 37, 36, // RT
                35, 34, 33, 32, // R9
                31, 30, 29, 28, // R8
                27, 26, 25, 24, // R7
                23, 22, 21, 20, // R6
                19, 18, 17, 16, // R5
                15, 14, 13, 12, // R4
                11, 10, 09, 08, // R3
                07, 06, 05, 04, // R2
                03, 02, 01, 00  // RA
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
                var count = col < 4 ? 7 : 6;

                for (var j = 0; j < count; j++)
                {
                    cards[j] = crv[col + (j * 8)];
                }

                tableaus[col++] = Tableau.Create(cards.Slice(0, count));
                i += count;
            }

            return Board.Create(Reserve.Create(), Foundation.Create(), Tableaus.Create(tableaus[0], tableaus[1], tableaus[2], tableaus[3], tableaus[4], tableaus[5], tableaus[6], tableaus[7]));
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
                // Note for cases where board.Foundation[i] - 1 evaluates to -1, 
                // the byte cast will make the result 255 which is equal to Ranks.Nil
                var c = (byte)(board.Foundation[i] - 1);
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