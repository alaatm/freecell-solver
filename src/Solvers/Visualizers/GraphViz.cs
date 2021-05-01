using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCellSolver.Solvers.Visualizers
{
    public enum EventType
    {
        Goal,
        Enqueue,
        Dequeue,
        CloseExist,
        OpenExist,
        ReplaceAdd,
        ReplaceRemove,
    }

    public enum Format
    {
        Svg,
        Tiff,
    }

    public interface ITaggable
    {
        string Id { get; }
        string Tag { get; }
    }

    public struct Event<T> where T : class, ITaggable
    {
        public T Parent;
        public T Node;
        public EventType EventType;

        public Event(T parent, T node, EventType eventType)
        {
            Parent = parent;
            Node = node;
            EventType = eventType;
        }

        public Event(T node, EventType eventType)
        {
            Parent = null;
            Node = node;
            EventType = eventType;
        }
    }

    public static class GraphViz
    {
        public static void WriteTo<T>(string path, List<Event<T>> events, bool onlyFinal) where T : class, ITaggable
        {
            var events_ = events.ToArray().AsSpan();
            var end = onlyFinal ? events_.Length - 1 : 0;

            var eventsDict = new Dictionary<string, Event<T>>();

            while (end++ < events_.Length)
            {
                using var fs = File.OpenWrite(Path.Join(path, $"{end - 1:00000}.dot"));
                using var writer = new StreamWriter(fs);

                var section = events_.Slice(0, end);

                eventsDict.Clear();

                writer.Write("graph{");

                foreach (var e in section)
                {
                    if (e.Parent is not null)
                    {
                        writer.Write($"{e.Parent.Id}--{e.Node.Id};");
                    }
                    else if (e.Parent is null && e.EventType == EventType.Enqueue)
                    {
                        writer.Write($"{e.Node.Id};");
                    }

                    if (!eventsDict.ContainsKey(e.Node.Id))
                    {
                        eventsDict.Add(e.Node.Id, GetLastEvent(section, e.Node.Id));
                    }
                }

                foreach (var kvp in eventsDict)
                {
                    var color = "";
                    switch (kvp.Value.EventType)
                    {
                        case EventType.Goal: color = "forestgreen"; break;
                        case EventType.Enqueue: color = "lightblue"; break;
                        case EventType.Dequeue: color = "magenta"; break;
                        case EventType.CloseExist: color = "red"; break;
                        case EventType.OpenExist: color = "pink"; break;
                        case EventType.ReplaceAdd: color = "mediumslateblue"; break;
                        case EventType.ReplaceRemove: color = "yellow"; break;
                    }

                    writer.Write($"{kvp.Value.Node.Id}[label=\"{kvp.Value.Node.Tag}\",style=filled,color={color}];");
                }

                writer.Write('}');
                writer.Flush();
            }

            static Event<T> GetLastEvent(Span<Event<T>> events, string id)
            {
                for (var i = events.Length - 1; i >= 0; i--)
                {
                    if (events[i].Node.Id == id)
                    {
                        return events[i];
                    }
                }
                Debug.Fail("");
                return default;
            }
        }

        public static void Generate(string path, Format format)
        {
            var outputType = format == Format.Svg ? "svg" : "tif";

            var pLevel = Environment.ProcessorCount / 2;
            var files = Directory.GetFiles(path, "*.dot");
            var sectionCount = (int)Math.Ceiling((double)files.Length / pLevel);

            var tasks = Enumerable.Range(0, pLevel).Select(i => Task.Run(() =>
            {
                foreach (var file in files.Skip(sectionCount * i).Take(sectionCount))
                {
                    var psi = new ProcessStartInfo();
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.FileName = @"C:\Program Files\Graphviz\bin\dot.exe";
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.Arguments = $"-T {outputType} -O \"{file}\"";

                    try
                    {
                        // Start the process with the info we specified.
                        // Call WaitForExit and then the using statement will close.
                        var p = Process.Start(psi);
                        p.WaitForExit();
                    }
                    catch
                    {
                        // Log error.
                    }
                }
            }));

            Task.WaitAll(tasks.ToArray());

            if (format == Format.Svg)
            {
                pLevel = Environment.ProcessorCount / 2;
                files = Directory.GetFiles(path, "*.svg");
                sectionCount = (int)Math.Ceiling((double)files.Length / pLevel);

                tasks = Enumerable.Range(0, pLevel).Select(i => Task.Run(() =>
                {
                    foreach (var file in files.Skip(sectionCount * i).Take(sectionCount))
                    {
                        var filename = Path.GetFileNameWithoutExtension(file);

                        var svg = File.ReadAllText(file);
                        svg = svg.Replace("stroke=\"transparent\"", "stroke=\"black\"");
                        File.WriteAllText(file, svg);

                        var psi = new ProcessStartInfo();
                        psi.CreateNoWindow = true;
                        psi.UseShellExecute = false;
                        psi.FileName = @"C:\github\svgasm\out\build\x64-Release\tools\svgcleaner_win.exe";
                        psi.WindowStyle = ProcessWindowStyle.Hidden;
                        psi.Arguments = $"--multipass --quiet \"{file}\" \"{file.Replace(filename, filename + ".clean")}\"";

                        try
                        {
                            // Start the process with the info we specified.
                            // Call WaitForExit and then the using statement will close.
                            using var p = Process.Start(psi);
                            p.WaitForExit();
                            File.Delete(file);
                        }
                        catch
                        {
                            // Log error.
                        }
                    }
                }));

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
