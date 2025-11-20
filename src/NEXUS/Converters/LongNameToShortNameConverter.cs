using System.Globalization;
using Avalonia.Data.Converters;

namespace NEXUS.Converters
{
    public class LongNameToShortNameConverter : IValueConverter
    {
        public static LongNameToShortNameConverter Instance { get; } = new();
        public int MaxLength { get; set; } = 20; // длина по умолчанию

        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            if (value is not string text)
                return string.Empty;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (parameter is string paramStr && int.TryParse(paramStr, out var paramLength))
                MaxLength = paramLength;

            return text.Length <= MaxLength
                ? text
                : text[..(MaxLength - 3)] + "...";
        }

        public object? ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}