// using System;
// using System.Collections.Generic;
// using SkiaSharp;

// namespace FreeCellSolver
// {
//     public class Game
//     {
//         private List<SKImage> _captures = new List<SKImage>();

//         private readonly Reserve _bucket = new Reserve();
//         private readonly Foundation _foundation = new Foundation();
//         private readonly List<Tableau> _tableaus;

//         public Game(List<Tableau> deal)
//         {
//             if (Deal.IsValid(deal))
//             {
//                 _tableaus = deal;
//             }
//         }

//         public void Solve()
//         {
//             var moves = 0;
//             do
//             {
//                 moves = MoveToFoundation();
//             } while (moves >= 1);
//         }

//         private int MoveToFoundation()
//         {
//             var moves = 0;

//             foreach (var tableau in _tableaus)
//             {
//                 if (_foundation.CanPush(tableau.Top))
//                 {
//                     _foundation.Push(tableau.Pop());
//                     _captures.Add(ToImage());
//                     moves++;
//                 }
//             }

//             return moves;
//         }

//         public SKImage ToImage()
//         {
//             // From table's ToImage()
//             const float partialOffset = 0.27f;
//             var topOffset = (int)Math.Round(Assets.Instance.CardHeight * partialOffset, 0);

//             var margin = (int)Math.Round(200 * Assets.Instance.Scale, 0);
//             var spacing = (int)Math.Round(70 * Assets.Instance.Scale, 0);

//             var topMargin = (int)Math.Round(40 * Assets.Instance.Scale, 0);
//             var verticalSpacing = (int)Math.Round(40 * Assets.Instance.Scale, 0);
//             var bottomMargin = (int)Math.Round(150 * Assets.Instance.Scale, 0);

//             var width = Assets.Instance.CardWidth * 8 + spacing * 7 + margin;
//             var height = topMargin + Assets.Instance.CardHeight + verticalSpacing + (6 * topOffset) + Assets.Instance.CardHeight + bottomMargin;

//             return null;
//         }
//     }
// }