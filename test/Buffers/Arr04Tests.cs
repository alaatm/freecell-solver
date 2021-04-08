using System.Linq;
using FreeCellSolver.Buffers;
using Xunit;

namespace FreeCellSolver.Test.Buffers
{
    public class Arr04Tests
    {
        const int N = 4;

        [Fact]
        public void Indexer_tests()
        {
            // Arrange
            var arr = new Arr04();

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
            var a = new Arr04();
            var b = new Arr04();
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

            Assert.True(new Arr04().Equals((object)new Arr04()));
            Assert.False(a.Equals(0));
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(6, 1)]
        [InlineData(7, 2)]
        [InlineData(8, 3)]
        [InlineData(9, -1)]
        public void IndexOf_tests(byte value, int expectedIndex)
        {
            // Arrange
            var arr = new Arr04();
            for (var i = 0; i < N; i++)
            {
                arr[i] = (byte)(i + 5);
            }

            // Act
            var index = arr.IndexOf(value);

            // Assert
            Assert.Equal(expectedIndex, index);
        }

        [Theory]
        [InlineData(5, 0, -1)]
        [InlineData(6, 0, -1)]
        [InlineData(7, 0, -1)]
        [InlineData(8, 0, -1)]
        [InlineData(9, 0, -1)]

        [InlineData(5, 1, 0)]
        [InlineData(6, 1, -1)]
        [InlineData(7, 1, -1)]
        [InlineData(8, 1, -1)]
        [InlineData(9, 1, -1)]

        [InlineData(5, 2, 0)]
        [InlineData(6, 2, 1)]
        [InlineData(7, 2, -1)]
        [InlineData(8, 2, -1)]
        [InlineData(9, 2, -1)]

        [InlineData(5, 3, 0)]
        [InlineData(6, 3, 1)]
        [InlineData(7, 3, 2)]
        [InlineData(8, 3, -1)]
        [InlineData(9, 3, -1)]

        [InlineData(5, 4, 0)]
        [InlineData(6, 4, 1)]
        [InlineData(7, 4, 2)]
        [InlineData(8, 4, 3)]
        [InlineData(9, 4, -1)]
        public void IndexOf_with_len_tests(byte value, int len, int expectedIndex)
        {
            // Arrange
            var arr = new Arr04();
            for (var i = 0; i < N; i++)
            {
                arr[i] = (byte)(i + 5);
            }

            // Act
            var index = arr.IndexOf(value, len);

            // Assert
            Assert.Equal(expectedIndex, index);
        }

        [Theory]
        [MemberData(nameof(Remove_single_tests_testData))]
        public void Remove_single_tests(byte valueToRemove, byte len, int[] expectedArr, int expectedLen)
        {
            // Arrange
            var arr = new Arr04();
            for (var i = 0; i < N; i++)
            {
                arr[i] = (byte)(i + 5);
            }

            // Act
            arr.Remove(valueToRemove, ref len);

            // Assert
            Assert.Equal(expectedLen, len);
            Assert.Equal(expectedArr, arr.AsArray().Select(i => (int)i));
        }

        public static TheoryData<byte, byte, int[], int> Remove_single_tests_testData() => new()
        {
            { 5, 0, new[] { 5, 6, 7, 8 }, 0 },
            { 6, 0, new[] { 5, 6, 7, 8 }, 0 },
            { 7, 0, new[] { 5, 6, 7, 8 }, 0 },
            { 8, 0, new[] { 5, 6, 7, 8 }, 0 },

            { 5, 1, new[] { 5, 6, 7, 8 }, 0 },
            { 6, 1, new[] { 5, 6, 7, 8 }, 1 },
            { 7, 1, new[] { 5, 6, 7, 8 }, 1 },
            { 8, 1, new[] { 5, 6, 7, 8 }, 1 },

            { 5, 2, new[] { 6, 6, 7, 8 }, 1 },
            { 6, 2, new[] { 5, 6, 7, 8 }, 1 },
            { 7, 2, new[] { 5, 6, 7, 8 }, 2 },
            { 8, 2, new[] { 5, 6, 7, 8 }, 2 },

            { 5, 3, new[] { 6, 7, 7, 8 }, 2 },
            { 6, 3, new[] { 5, 7, 7, 8 }, 2 },
            { 7, 3, new[] { 5, 6, 7, 8 }, 2 },
            { 8, 3, new[] { 5, 6, 7, 8 }, 3 },

            { 5, 4, new[] { 6, 7, 8, 8 }, 3 },
            { 6, 4, new[] { 5, 7, 8, 8 }, 3 },
            { 7, 4, new[] { 5, 6, 8, 8 }, 3 },
            { 8, 4, new[] { 5, 6, 7, 8 }, 3 },
        };

        [Theory]
        [MemberData(nameof(Remove_multi_tests_testData))]
        public void Remove_multi_tests(byte valueToRemove, byte len, int[] expectedArr, int expectedLen)
        {
            // Arrange
            var arr = new Arr04();
            arr[0] = arr[1] = 5;
            arr[2] = arr[3] = 6;

            // Act
            arr.Remove(valueToRemove, ref len);

            // Assert
            Assert.Equal(expectedLen, len);
            Assert.Equal(expectedArr, arr.AsArray().Select(i => (int)i));
        }

        public static TheoryData<byte, byte, int[], int> Remove_multi_tests_testData() => new()
        {
            { 5, 0, new[] { 5, 5, 6, 6 }, 0 },
            { 5, 1, new[] { 5, 5, 6, 6 }, 0 },
            { 5, 2, new[] { 5, 5, 6, 6 }, 0 },
            { 5, 3, new[] { 6, 6, 6, 6 }, 1 },
            { 5, 4, new[] { 6, 6, 6, 6 }, 2 },

            { 6, 0, new[] { 5, 5, 6, 6 }, 0 },
            { 6, 1, new[] { 5, 5, 6, 6 }, 1 },
            { 6, 2, new[] { 5, 5, 6, 6 }, 2 },
            { 6, 3, new[] { 5, 5, 6, 6 }, 2 },
            { 6, 4, new[] { 5, 5, 6, 6 }, 2 },
        };

        [Fact]
        public void Remove_does_nothing_when_value_not_found()
        {
            // Arrange
            byte len = 4;
            var arr = new Arr04();
            for (var i = 0; i < N; i++)
            {
                arr[i] = (byte)(i + 5);
            }

            // Act
            arr.Remove(100, ref len);

            // Assert
            Assert.Equal(4, len);
            Assert.Equal(new[] { 5, 6, 7, 8 }, arr.AsArray().Select(i => (int)i));
        }

        [Fact]
        public void AsArray_returns_array_representation()
        {
            // Arrange
            var arr = new Arr04();
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