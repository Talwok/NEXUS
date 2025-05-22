using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NEXUS.Parsers.MDT.Models.Pallete;

namespace NEXUS.Converters;

public struct ColorTableRange(float maximum, float minimum, float upperSelection, float lowerSelection, List<PaletteColor> colors)
{
    public List<PaletteColor> Colors { get; set; } = colors;
    public float Maximum { get; set; } = maximum;
    public float Minimum { get; set; } = minimum;
    public float UpperSelection { get; set; } = upperSelection;
    public float LowerSelection { get; set; } = lowerSelection;
}

public class VerticalBorderColorsToGradientConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ColorTableRange { Colors.Count: > 0 } range)
        {
            var gradientStops = new GradientStops();

            var first = range.Colors.First();
            var last = range.Colors.Last();
            var firstColor = Color.FromRgb(first.Red, first.Green, first.Blue);
            var lastColor = Color.FromRgb(last.Red, last.Green, last.Blue);

            if (range.Maximum - range.Minimum != 0)
            {
                var lowerStopOffset = (range.LowerSelection - range.Minimum) / (range.Maximum - range.Minimum); 
                var upperStopOffset = (range.UpperSelection - range.Minimum) / (range.Maximum - range.Minimum); 
            
                gradientStops.Add(new GradientStop(firstColor, 0));
                gradientStops.Add(new GradientStop(firstColor, lowerStopOffset));
                gradientStops.Add(new GradientStop(lastColor, upperStopOffset));
                gradientStops.Add(new GradientStop(lastColor, 1));    
            }
            else
            {
                gradientStops.Add(new GradientStop(firstColor, 0));
                gradientStops.Add(new GradientStop(firstColor, 0.5));
                gradientStops.Add(new GradientStop(lastColor, 0.5));
                gradientStops.Add(new GradientStop(lastColor, 1));
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

    public static VerticalBorderColorsToGradientConverter Instance = new();
}