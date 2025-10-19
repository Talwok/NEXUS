using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NEXUS.Parsers.MDT.Models.Pallete;

namespace NEXUS.Converters;

public class VerticalColorsToGradientConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<PaletteColor> { Count: > 0 } colors)
        {
            var gradientStops = new GradientStops();

            // Равномерно распределяем цвета вдоль линии
            double offsetStep = 1.0 / (colors.Count - 1);
            for (int i = 0; i < colors.Count; i++)
            {
                var color = colors[i];
                gradientStops.Add(new GradientStop(
                    Color.FromRgb(color.Red, color.Green, color.Blue),
                    i * offsetStep));
            }

            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                GradientStops = gradientStops
            };
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static VerticalColorsToGradientConverter Instance = new();
}