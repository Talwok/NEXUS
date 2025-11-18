using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Material.Icons;

namespace NEXUS.Fractal.Converters;

public class EntityPathToIconConverter : IValueConverter
{
    public static EntityPathToIconConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string fullPath)
        {
            if (Directory.Exists(fullPath))
            {
                return MaterialIconKind.Folder;
            }

            return fullPath switch
            {
                ".jpg" or ".jpeg" or ".png" or ".bmp" or ".xyz" or ".bcr" or ".mdt" => MaterialIconKind.ImageFrame,
                ".txt" => MaterialIconKind.FileDocument,
                _ => MaterialIconKind.File
            };
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}