using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Material.Icons;
using Svg;
using Svg.Skia;

namespace NEXUS.Fractal.Converters;

public class FileExtensionToIconConverter : IValueConverter
{
    public static FileExtensionToIconConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".xyz":
                case ".bcr":
                case ".mdt":
                    return MaterialIconKind.ImageFrame;

                case ".txt":
                    return MaterialIconKind.FileDocument;

                default:
                    return MaterialIconKind.File;
            }
        }
        return null;
    }

    public string GetSvgPathData(string svgFilePath)
    {
        var svg = new SKSvg();
        svg.Load(svgFilePath);

        // Get the first path from the SVG
        if (svg.Picture != null)
        {
            // Convert SVG to path data (this is a simplified approach)
            // For more complex extraction, you'd need to parse the SVG DOM
            var svgDocument = SvgDocument.Open(svgFilePath);

            // Find all path elements
            var paths = svgDocument.Descendants().OfType<SvgPath>().ToList();
            if (paths.Any())
            {
                var data = paths.First().PathData.ToString();
                return data;
            }
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}