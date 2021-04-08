using System;
using FreeCellSolver.Buffers;
using Xunit;

namespace FreeCellSolver.Test.Buffers
{
    public class Arr18Tests
    {
        const int N = 18;

        [Fact]
        public void Indexer_tests()
        {
            // Arrange
            var arr = new Arr18();

            // Act
            for (var i = 0; i < N; i++)
            {
                arr[i] = (byte)i;
            }

            // Assert
            for (var i = 0; i < N; i++)
            {
                Assert.Equal(i, arr[i]);
            }
        }

        [Fact]
        public void Equality_and_hashcode_tests()
        {
            var a = new Arr18();
            var b = new Arr18();
            Assert.True(a == b);
            Assert.True(Equals(a, b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());


            for (var i = 0; i < N; i++)
            {
                a[i] = (byte)(i + 1);
                Assert.True(a != b);
                Assert.False(Equals(a, b));
                Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
                b[i] = (byte)(i + 1);
                Assert.True(a == b);
                Assert.True(Equals(a, b));
                Assert.Equal(a.GetHashCode(), b.GetHashCode());
            }

            Assert.True(new Arr18().Equals((object)new Arr18()));
            Assert.False(a.Equals(0));
        }

        [Fact]
        public void CopyTo_copies_specified_buffer()
        {
            // Arrange
            var a = new Arr18();
            var b = new Arr18();
            var sourceIndex = 4;
            var destinationIndex = 10;
            var count = 3;

            for (var i = 0; i < N; i++)
            {
                a[i] = (byte)(i + 5);
            }

            // Act
            a.CopyTo(ref b, sourceIndex, destinationIndex, count);

            // Assert
            for (var i = 0; i < N; i++)
            {
                if (i >= destinationIndex && i < destinationIndex + count)
                {
                    Assert.Equal((byte)(i + 5 - destinationIndex + sourceIndex), b[i]);
                }
                else
                {
                    Assert.Equal(0, b[i]);
                }
            }
        }

        [Theory]
        [MemberData(nameof(SequenceEqual_returns_whether_specified_buffers_are_equal_testData))]
        public void SequenceEqual_returns_whether_specified_buffers_are_equal(int[] a, int[] b, int startIndex, int count, bool expectedEqual)
        {
            // Arrange
            var left = new Arr18();
            var right = new Arr18();
            for (var i = 0; i < a.Length; i++) { left[i] = (byte)a[i]; }
            for (var i = 0; i < b.Length; i++) { right[i] = (byte)b[i]; }

            // Act
            var equal = left.SequenceEqual(right, startIndex, count);

            // Assert
            Assert.Equal(expectedEqual, equal);
        }

        public static TheoryData<int[], int[], int, int, bool> SequenceEqual_returns_whether_specified_buffers_are_equal_testData() => new()
        {
            { new[] { 0 }, new[] { 0 }, 0, 0, true },
            { new[] { 1, 2, 3, 4, 5 }, new[] { 99, 2, 3, 4, 99 }, 1, 1, true },
            { new[] { 1, 2, 3, 4, 5 }, new[] { 99, 2, 3, 4, 99 }, 1, 2, true },
            { new[] { 1, 2, 3, 4, 5 }, new[] { 99, 2, 3, 4, 99 }, 1, 3, true },

            { new[] { 0 }, new[] { 1 }, 0, 0, true },
            { new[] { 0 }, new[] { 1 }, 0, 1, false },
            { new[] { 0 }, new[] { 1 }, 0, 2, false },
            { new[] { 0, 1, 2 }, new[] { 0, 9, 2 }, 0, 3, false },
        };

        [Fact]
        public void AsArray_returns_array_representation()
        {
            // Arrange
            var arr = new Arr18();
            for (var i = 0; i < N; i++)
            {
                arr[i] = (byte)(i + 1);
            }

            // Act
            var sysArr = arr.AsArray();

            // Assert
            Assert.Equal(N, sysArr.Length);
            for (var i = 0; i < N; i++)
            {
                Assert.Equal(arr[i], sysArr[i]);
            }
        }
    }
}
