using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using ReactiveUI;
using System;

namespace NEXUS.Fractal.Controls.Surface;

public partial class Surface : UserControl
{
    private Point _lastMousePosition;
    private bool _isRotating;
    
    public Surface()
    {
        InitializeComponent();
        
        // Обработчики событий мыши
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    private FrameViewModel _frame;

    public static readonly DirectProperty<Surface, FrameViewModel> FrameProperty = AvaloniaProperty.RegisterDirect<Surface, FrameViewModel>(
        nameof(Frame), o => o.Frame, (o, v) => o.Frame = v);

    public FrameViewModel Frame
    {
        get => _frame;
        set => SetAndRaise(FrameProperty, ref _frame, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        Frame.WhenAnyValue(
                vm => vm.HeightMap,
                vm => vm.ColorTable)
            .Subscribe(_ =>
            {
                ViewPanel.Children.Remove(SurfaceOpenGl);
                ViewPanel.Children.Add(SurfaceOpenGl);
            });
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(SurfaceOpenGl);
        _lastMousePosition = point.Position;

        if (point.Properties.IsLeftButtonPressed)
        {
            _isRotating = true;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isRotating = false;
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
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
    
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        SurfaceOpenGl.ZoomCamera((float)e.Delta.Y);
        e.Handled = true;
    }

    private void OnViewCubeViewSelected(AxisViewType axisViewType)
    {
        SurfaceOpenGl.SetCameraPreset(axisViewType);
    }
}