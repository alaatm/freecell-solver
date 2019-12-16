using System;
using System.Collections.Generic;
using System.Threading;

namespace FreeCellSolver.Extensions
{
    public static class ListExtensions
    {
        internal static class ThreadSafeRandom
        {
            [ThreadStatic] private static Random Local;

            public static Random Rand
            {
                get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
            }
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.Rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static int Times<T>(this IEnumerable<T> list, T val)
        {
            var c = 0;
            foreach (var v in list)
            {
                if (v.Equals(val))
                {
                    c++;
                }
            }

            return c;
        }

        public static Stack<T> Clone<T>(this Stack<T> original)
        {
            var arr = new T[original.Count];
            original.CopyTo(arr, 0);
            Array.Reverse(arr);
            return new Stack<T>(arr);
        }
    }
}