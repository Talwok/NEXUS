using Avalonia;
using NEXUS.Parsers.MDT.Models.Pallete;

namespace NEXUS.Converters;

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

public class ColorsToGradientConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<PaletteColor> colors && colors.Count > 0)
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
                StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
                GradientStops = gradientStops
            };
        }
        
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static ColorsToGradientConverter Instance = new();
}