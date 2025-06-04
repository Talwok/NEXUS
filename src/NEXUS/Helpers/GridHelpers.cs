using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;

namespace NEXUS.Helpers;

public static class GridHelpers
{
    public static readonly AttachedProperty<ObservableCollection<double>> PixelColumnWidthsProperty =
        AvaloniaProperty.RegisterAttached<Control, ObservableCollection<double>>(
            "PixelColumnWidths", typeof(GridHelpers));

    public static void SetPixelColumnWidths(AvaloniaObject element, ObservableCollection<double> value)
        => element.SetValue(PixelColumnWidthsProperty, value);

    public static ObservableCollection<double> GetPixelColumnWidths(AvaloniaObject element)
        => element.GetValue(PixelColumnWidthsProperty);

    static GridHelpers()
    {
        PixelColumnWidthsProperty.Changed.Subscribe(args =>
        {
            if (args is { Sender: Grid grid, NewValue.HasValue: true })
            {
                var widths = args.NewValue.Value;
                grid.ColumnDefinitions.Clear();

                var i = 0;
                foreach (var width in widths)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition(
                        i == 2 
                        ? new GridLength(1, GridUnitType.Star) 
                        : new GridLength(width, GridUnitType.Pixel)));
                    i++;
                }
            }
        });
    }
}