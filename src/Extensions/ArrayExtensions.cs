using System;

namespace FreeCellSolver.Extensions
{
    public static class ArrayExtensions
    {
        // Performs stable sort with good performance on small lists
        // Note no error checks!
        public static void InsertionSort<T>(this T[] list, Comparison<T> comparison)
        {
            var count = list.Length;
            for (var j = 1; j < count; j++)
            {
                var key = list[j];

                var i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }
    }
}