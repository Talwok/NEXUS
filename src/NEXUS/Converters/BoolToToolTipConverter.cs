using System.Globalization;
using Avalonia.Data.Converters;

namespace NEXUS.Converters;

public class BoolToToolTipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && parameter is string tooltips)
        {
            var parts = tooltips.Split('|');
            return isChecked ? parts[1] : parts[0];
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    public static BoolToToolTipConverter Instance { get; } = new ();
}