using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace NEXUS.Fractal.Converters;

public class EntityPathToNameConverter : IValueConverter
{
    public static EntityPathToNameConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string fullPath)
        {
            return Path.GetFileName(fullPath);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}