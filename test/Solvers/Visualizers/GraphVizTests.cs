using System;
using System.IO;
using System.Collections.Generic;
using FreeCellSolver.Solvers.Visualizers;
using Xunit;

namespace FreeCellSolver.Test.Solvers.Visualizers
{
    public class GraphVizTests
    {
        [Fact]
        public void X()
        {
            var root = new TestObj { Tag = "0" };
            var c0 = new TestObj { Tag = "1" };
            var c1 = new TestObj { Tag = "1" };
            var c2 = new TestObj { Tag = "1" };

            var c10 = new TestObj { Tag = "2" };
            var c11 = new TestObj { Tag = "2" };

            var c110 = new TestObj { Tag = "3" };
            var c111 = new TestObj { Tag = "3" };
            var c112 = new TestObj { Tag = "3" };
            var c113 = new TestObj { Tag = "3" };
            var c114 = new TestObj { Tag = "3" };

            var events = new List<Event<TestObj>>
            {
                new Event<TestObj>(null, root, EventType.Enqueue),
                new Event<TestObj>(root, EventType.Dequeue),

                new Event<TestObj>(root, c0, EventType.Enqueue),
                new Event<TestObj>(root, c1, EventType.Enqueue),
                new Event<TestObj>(root, c2, EventType.Enqueue),

                new Event<TestObj>(c1, EventType.Dequeue),

                new Event<TestObj>(c1, c10, EventType.Enqueue),
                new Event<TestObj>(c1, c11, EventType.Enqueue),

                new Event<TestObj>(c11, EventType.Dequeue),

                new Event<TestObj>(c11, c110, EventType.CloseExist),
                new Event<TestObj>(c11, c111, EventType.OpenExist),
                new Event<TestObj>(c11, c112, EventType.ReplaceAdd),
                new Event<TestObj>(c10, EventType.ReplaceRemove),
                new Event<TestObj>(c11, c113, EventType.Enqueue),
                new Event<TestObj>(c11, c114, EventType.Enqueue),

                new Event<TestObj>(c112, EventType.Dequeue),
                new Event<TestObj>(c112, EventType.Goal),
            };

            var tmpPath = Path.Join(Path.GetTempPath(), "freecell_solver_graphvis");
            Directory.CreateDirectory(tmpPath);

            GraphViz.WriteTo(tmpPath, events, false);

            Assert.Equal(17, Directory.GetFiles(tmpPath).Length);

            var expected1 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=lightblue];",
            });

            var expected2 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
            });

            var expected3 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
            });

            var expected4 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=lightblue];",
            });

            var expected5 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=lightblue];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
            });

            var expected6 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
            });

            var expected7 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=lightblue];",
            });

            var expected8 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=lightblue];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=lightblue];",
            });

            var expected9 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=lightblue];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
            });

            var expected10 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=lightblue];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
            });

            var expected11 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{c11.Id}--{c111.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=lightblue];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
                $"{c111.Id}[label=\"{c111.Tag}\",style=filled,color=pink];",
            });

            var expected12 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{c11.Id}--{c111.Id};",
                $"{c11.Id}--{c112.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=lightblue];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
                $"{c111.Id}[label=\"{c111.Tag}\",style=filled,color=pink];",
                $"{c112.Id}[label=\"{c112.Tag}\",style=filled,color=mediumslateblue];",
            });

            var expected13 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{c11.Id}--{c111.Id};",
                $"{c11.Id}--{c112.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=yellow];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
                $"{c111.Id}[label=\"{c111.Tag}\",style=filled,color=pink];",
                $"{c112.Id}[label=\"{c112.Tag}\",style=filled,color=mediumslateblue];",
            });

            var expected14 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{c11.Id}--{c111.Id};",
                $"{c11.Id}--{c112.Id};",
                $"{c11.Id}--{c113.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=yellow];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
                $"{c111.Id}[label=\"{c111.Tag}\",style=filled,color=pink];",
                $"{c112.Id}[label=\"{c112.Tag}\",style=filled,color=mediumslateblue];",
                $"{c113.Id}[label=\"{c113.Tag}\",style=filled,color=lightblue];",
            });

            var expected15 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{c11.Id}--{c111.Id};",
                $"{c11.Id}--{c112.Id};",
                $"{c11.Id}--{c113.Id};",
                $"{c11.Id}--{c114.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=yellow];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
                $"{c111.Id}[label=\"{c111.Tag}\",style=filled,color=pink];",
                $"{c112.Id}[label=\"{c112.Tag}\",style=filled,color=mediumslateblue];",
                $"{c113.Id}[label=\"{c113.Tag}\",style=filled,color=lightblue];",
                $"{c114.Id}[label=\"{c114.Tag}\",style=filled,color=lightblue];",
            });

            var expected16 = string.Join("", new[]
{
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{c11.Id}--{c111.Id};",
                $"{c11.Id}--{c112.Id};",
                $"{c11.Id}--{c113.Id};",
                $"{c11.Id}--{c114.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=yellow];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
                $"{c111.Id}[label=\"{c111.Tag}\",style=filled,color=pink];",
                $"{c112.Id}[label=\"{c112.Tag}\",style=filled,color=magenta];",
                $"{c113.Id}[label=\"{c113.Tag}\",style=filled,color=lightblue];",
                $"{c114.Id}[label=\"{c114.Tag}\",style=filled,color=lightblue];",
            });

            var expected17 = string.Join("", new[]
            {
                $"{root.Id};",
                $"{root.Id}--{c0.Id};",
                $"{root.Id}--{c1.Id};",
                $"{root.Id}--{c2.Id};",
                $"{c1.Id}--{c10.Id};",
                $"{c1.Id}--{c11.Id};",
                $"{c11.Id}--{c110.Id};",
                $"{c11.Id}--{c111.Id};",
                $"{c11.Id}--{c112.Id};",
                $"{c11.Id}--{c113.Id};",
                $"{c11.Id}--{c114.Id};",
                $"{root.Id}[label=\"{root.Tag}\",style=filled,color=magenta];",
                $"{c0.Id}[label=\"{c0.Tag}\",style=filled,color=lightblue];",
                $"{c1.Id}[label=\"{c1.Tag}\",style=filled,color=magenta];",
                $"{c2.Id}[label=\"{c2.Tag}\",style=filled,color=lightblue];",
                $"{c10.Id}[label=\"{c10.Tag}\",style=filled,color=yellow];",
                $"{c11.Id}[label=\"{c11.Tag}\",style=filled,color=magenta];",
                $"{c110.Id}[label=\"{c110.Tag}\",style=filled,color=red];",
                $"{c111.Id}[label=\"{c111.Tag}\",style=filled,color=pink];",
                $"{c112.Id}[label=\"{c112.Tag}\",style=filled,color=forestgreen];",
                $"{c113.Id}[label=\"{c113.Tag}\",style=filled,color=lightblue];",
                $"{c114.Id}[label=\"{c114.Tag}\",style=filled,color=lightblue];",
            });

            var expected = new[] { expected1, expected2, expected3, expected4, expected5, expected6, expected7, expected8,
            expected9, expected10, expected11, expected12, expected13, expected14, expected15, expected16, expected17 };

            for (var i = 0; i < 17; i++)
            {
                var actual = File.ReadAllText(Path.Join(tmpPath, $"{i:00000}.dot"));
                Assert.Equal($"graph{{{expected[i]}}}", actual);
            }
        }

        class TestObj : ITaggable
        {
            public string Id { get; } = $"n{Guid.NewGuid():N}";
            public string Tag { get; set; }
        }
    }
}
