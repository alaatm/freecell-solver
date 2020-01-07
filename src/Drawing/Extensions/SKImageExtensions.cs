using System.IO;
using SkiaSharp;

namespace FreeCellSolver.Drawing.Extensions
{
    public static class SKImageExtensions
    {
        public static void Save(this SKImage img, string path)
        {
            using var fs = File.Open(path, FileMode.Create);
            var data = img.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(fs);
        }
    }
}