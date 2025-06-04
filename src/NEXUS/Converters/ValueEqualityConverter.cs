using System.Globalization;
using Avalonia.Data.Converters;

namespace NEXUS.Converters;

public class ValueEqualityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is int intParameter)
                return intValue == intParameter;
            if (value is double doubleValue && parameter is double doubleParameter)
                return Math.Abs(doubleValue - doubleParameter) < 0.0000000001;
            if (value is bool boolValue && parameter is bool boolParameter)
                return boolValue == boolParameter;
            if (value is string stringValue && parameter is string stringParameter)
                return stringValue == stringParameter;
            
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    public static ValueEqualityConverter Instance { get; } = new ();
}