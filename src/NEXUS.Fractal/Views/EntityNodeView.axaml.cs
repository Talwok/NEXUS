using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace NEXUS.Fractal.Views;

public partial class EntityNodeView : UserControl
{
    private Point _lastMousePosition;
    private bool _isRotating;

    public EntityNodeView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is EntityNodeViewModel entityNodeViewModel)
        {
            ActualThemeVariantChanged += (_, _) => ReinsertOpenGlControl();

            entityNodeViewModel.WhenAnyValue(vm => vm.ColorTableRange)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => ReinsertOpenGlControl());

            entityNodeViewModel.WhenAnyValue(
                    vm => vm.HeightMap,
                    vm => vm.ColorTable)
                .Subscribe(_ => ReinsertOpenGlControl());
        }
    }

    private void ReinsertOpenGlControl()
    {
        ViewPanel?.Children.Remove(SurfaceOpenGl);
        ViewPanel?.Children.Add(SurfaceOpenGl);
    }

    private void OnViewCubeViewSelected(AxisViewType type)
    {
        SurfaceOpenGl?.SetCameraPreset(type);
    }

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
            SurfaceOpenGl?.RotateModel((float)(delta.X * 0.01), (float)(-delta.Y * 0.01));
            e.Handled = true;
        }
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        SurfaceOpenGl?.ZoomCamera((float)e.Delta.Y);
        e.Handled = true;
    }
}