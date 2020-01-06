using FreeCellSolver.Extensions;
using Xunit;

namespace test.Extensions
{
    public class ArrayExtensionsTests
    {
        [Fact]
        public void InsertionSort_performs_stable_sort()
        {
            // Arrange
            var o1 = new Dummy { Value = 500 };
            var o2 = new Dummy { Value = 500 };
            var o3 = new Dummy { Value = 600 };
            var o4 = new Dummy { Value = 600 };
            var arr = new Dummy[] { o4, o3, o2, o1 };

            // Act
            arr.InsertionSort((a, b) => a.Value - b.Value);

            // Assert - Should maintain original order of insertion
            Assert.Same(o2, arr[0]);
            Assert.Same(o1, arr[1]);
            Assert.Same(o4, arr[2]);
            Assert.Same(o3, arr[3]);
        }

        class Dummy
        {
            public int Value { get; set; }
        }
    }
}