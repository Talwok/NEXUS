using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Material.Icons;

namespace NEXUS.Converters;

public class MaterialIconKindToPathConverter : IValueConverter
{
    public static MaterialIconKindToPathConverter Instance = new();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is MaterialIconKind kind ? MaterialIconDataProvider.GetData(kind) : BindingOperations.DoNothing;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
