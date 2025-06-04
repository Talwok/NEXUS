using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace NEXUS.Converters;

public class RangeToGradientConverter : IMultiValueConverter
{
    public static RangeToGradientConverter Instance = new();
    
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count != 3 || 
            !(values[0] is LinearGradientBrush originalBrush) || 
            !(values[1] is double rangeStart) || 
            !(values[2] is double rangeMaximum))
            return Brushes.Transparent;

        if (originalBrush.GradientStops.Count == 0)
            return Brushes.Transparent;

        var newBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative)
        };

        double rangeStartOffset = rangeStart / rangeMaximum;
        double rangeEndOffset = (values[2] is double rangeEnd) ? rangeEnd / rangeMaximum : 1;

        // Добавляем точки градиента
        newBrush.GradientStops.AddRange(new[]
        {
            new GradientStop(originalBrush.GradientStops.First().Color, 0),
            new GradientStop(originalBrush.GradientStops.First().Color, rangeStartOffset),
            new GradientStop(originalBrush.GradientStops.Last().Color, rangeEndOffset),
            new GradientStop(originalBrush.GradientStops.Last().Color, 1)
        });

        return newBrush;
    }
}