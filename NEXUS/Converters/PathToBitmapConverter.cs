using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace NEXUS.Converters;

public class PathToBitmapConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string filePath && File.Exists(filePath))
        {
            return new Bitmap(filePath);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static PathToBitmapConverter Instance { get; } = new ();
}