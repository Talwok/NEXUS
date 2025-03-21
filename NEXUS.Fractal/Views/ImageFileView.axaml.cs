using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace NEXUS.Fractal.Views;

public partial class ImageFileView : UserControl
{
    public ImageFileView()
    {
        InitializeComponent();
    }

    private void ZoomSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        var y = ZoomBorder.OffsetY + Image.Bounds.Height / 2.0;
        var x = ZoomBorder.OffsetX + Image.Bounds.Width / 2.0;
        ZoomBorder.Zoom(e.NewValue, x, y);
    }

    private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        ZoomSlider.Value = (e.ZoomX + e.ZoomY) / 2;
    }

    private void ButtonZoomOut_OnClick(object? sender, RoutedEventArgs e)
    {
        ZoomBorder.ZoomOut();
    }

    private void ButtonZoomIn_OnClick(object? sender, RoutedEventArgs e)
    {
        ZoomBorder.ZoomIn();
    }

    private void ButtonRestoreZoom_OnClick(object? sender, RoutedEventArgs e)
    {
        ZoomBorder.Zoom(1, 0, 0);
    }
}