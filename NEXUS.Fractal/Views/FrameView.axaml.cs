using Avalonia;
using Avalonia.Controls;
using NEXUS.Fractal.ViewModels;
using ReactiveUI;
using System;
using Avalonia.Input;
using NEXUS.Fractal.Models;

namespace NEXUS.Fractal.Views;

public partial class FrameView : UserControl
{
    private Point _lastMousePosition;
    private bool _isRotating;
    
    public FrameView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is FrameViewModel frameViewModel)
        {
            frameViewModel.WhenAnyValue(
                    vm => vm.HeightMap,
                    vm => vm.ColorTable)
                .Subscribe(_ =>
                {
                    ViewPanel.Children.Remove(SurfaceOpenGl);
                    ViewPanel.Children.Add(SurfaceOpenGl);
                });    
        }
    }
    
    private void OnViewCubeViewSelected(AxisViewType type) 
        => SurfaceOpenGl.SetCameraPreset(type);

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(SurfaceOpenGl);
        _lastMousePosition = point.Position;

        if (point.Properties.IsLeftButtonPressed)
        {
            _isRotating = true;
            e.Handled = true;
        }
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isRotating = false;
        e.Handled = true;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(SurfaceOpenGl);
        var currentPosition = point.Position;
        var delta = currentPosition - _lastMousePosition;
        _lastMousePosition = currentPosition;

        if (_isRotating)
        {
            SurfaceOpenGl.RotateModel((float)(delta.X * 0.01), (float)(-delta.Y * 0.01));
            e.Handled = true;
        }
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        SurfaceOpenGl.ZoomCamera((float)e.Delta.Y);
        e.Handled = true;
    }
}