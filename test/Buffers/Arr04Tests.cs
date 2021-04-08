using System;
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
            for (var i =0; i < N; i++)
            {
                arr[i] = (byte)(i + 5);
            }

            // Act
            var index = arr.IndexOf(value);

            // Assert
            Assert.Equal(expectedIndex, index);
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
