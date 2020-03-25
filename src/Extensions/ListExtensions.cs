using System;
using System.Threading;
using System.Collections.Generic;

namespace FreeCellSolver.Extensions
{
    public static class ListExtensions
    {
        internal static class ThreadSafeRandom
        {
            [ThreadStatic] private static Random _local;

            public static Random Rand => _local ?? (_local = new Random(unchecked((Environment.TickCount * 31) + Thread.CurrentThread.ManagedThreadId)));
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.Rand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}