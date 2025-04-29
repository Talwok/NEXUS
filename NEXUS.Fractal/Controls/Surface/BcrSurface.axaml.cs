using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using NEXUS.Parsers.BCR;
using NEXUS.Parsers.BCR.Helpers;
using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models.Pallete;

namespace NEXUS.Fractal.Controls.Surface;

public partial class BcrSurface : UserControl
{
    private Point _lastMousePosition;
    private bool _isRotating;
    
    public BcrSurface()
    {
        InitializeComponent();
        
        // Обработчики событий мыши
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    private BcrFile _bcrFrame;

    public static readonly DirectProperty<BcrSurface, BcrFile> BcrFrameProperty = AvaloniaProperty.RegisterDirect<BcrSurface, BcrFile>(
        nameof(BcrFrame), o => o.BcrFrame, (o, v) => o.BcrFrame = v);

    public BcrFile BcrFrame
    {
        get => _bcrFrame;
        set => SetAndRaise(BcrFrameProperty, ref _bcrFrame, value);
    }

    private PaletteColorTable _colorTable;

    public static readonly DirectProperty<BcrSurface, PaletteColorTable> ColorTableProperty = AvaloniaProperty.RegisterDirect<BcrSurface, PaletteColorTable>(
        nameof(ColorTable), o => o.ColorTable, (o, v) => o.ColorTable = v);

    public PaletteColorTable ColorTable
    {
        get => _colorTable;
        set => SetAndRaise(ColorTableProperty, ref _colorTable, value);
    }
    

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        // Загрузка данных высот
        var processor = new BcrFrameImageProcessor(BcrFrame);
        SurfaceOpenGl.SetHeightMap(processor.GetHeightMap());
        SurfaceOpenGl.SetColorTable(ColorTable);
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