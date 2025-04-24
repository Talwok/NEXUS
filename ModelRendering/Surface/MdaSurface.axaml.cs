using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace ModelRendering.Surface;

public partial class MdaSurface : UserControl
{
    private Point _lastMousePosition;
    private bool _isRotating;
    private bool _isPanning;

    public MdaSurface()
    {
        InitializeComponent();

        // Загрузка данных высот
        var jagged = JsonSerializer.Deserialize<float[][]>(File.ReadAllText("Assets/MdaFrame_44"));
        SurfaceOpenGl.SetHeightMap(ConvertFromJaggedArray(jagged));

        // Обработчики событий мыши
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
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
        else if (point.Properties.IsMiddleButtonPressed)
        {
            _isPanning = true;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isRotating = false;
        _isPanning = false;
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
            // Более плавное вращение с ограничением по вертикали
            SurfaceOpenGl.CameraYaw -= (float)(delta.X * 0.005);
            SurfaceOpenGl.CameraPitch = Math.Clamp(
                SurfaceOpenGl.CameraPitch + (float)(delta.Y * 0.005),
                -MathF.PI * 0.49f,
                MathF.PI * 0.49f);
            e.Handled = true;
        }
        else if (_isPanning)
        {
            // Панорамирование с учетом направления камеры
            float panSpeed = 0.002f * SurfaceOpenGl.CameraDistance;
        
            // Векторы направления камеры
            float cosYaw = MathF.Cos(SurfaceOpenGl.CameraYaw);
            float sinYaw = MathF.Sin(SurfaceOpenGl.CameraYaw);
        
            SurfaceOpenGl.PanOffset += new Vector3(
                (float)(-delta.X * cosYaw - delta.Y * sinYaw) * panSpeed,
                0,
                (float)(delta.X * sinYaw - delta.Y * cosYaw) * panSpeed
            );
            e.Handled = true;
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // Более плавное приближение/отдаление с ограничениями
        float zoomFactor = 1.0f + (float)(e.Delta.Y * 0.05);
        SurfaceOpenGl.CameraDistance = Math.Clamp(
            SurfaceOpenGl.CameraDistance / zoomFactor,
            1.0f,
            100.0f);
        e.Handled = true;
    }
    public static float[,] ConvertFromJaggedArray(float[][] jaggedArray)
    {
        if (jaggedArray == null || jaggedArray.Length == 0)
            return new float[0, 0];

        int rows = jaggedArray.Length;
        int cols = jaggedArray[0].Length;

        for (int i = 1; i < rows; i++)
        {
            if (jaggedArray[i].Length != cols)
                throw new ArgumentException("Jagged array is not rectangular");
        }

        float[,] array2D = new float[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array2D[i, j] = jaggedArray[i][j];
            }
        }

        return array2D;
    }
}