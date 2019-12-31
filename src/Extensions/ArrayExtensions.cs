using System;

namespace FreeCellSolver.Extensions
{
    public static class ArrayExtensions
    {
        // Performs stable sort with good performance on small lists
        // Note no error checks!
        public static void InsertionSort<T>(this T[] list, Comparison<T> comparison)
        {
            int count = list.Length;
            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }
    }
}