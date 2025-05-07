using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using NEXUS.Parsers.MDT.Models.Pallete;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Avalonia.Media.Color;

namespace NEXUS.Fractal.Controls.Surface;

/// <summary>
/// Custom OpenGL control for 3D surface rendering using Silk.NET
/// </summary>
internal class SurfaceOpenGlControl : OpenGlControlBase, INotifyPropertyChanged
{
    private GL _gl;

    /// <summary>
    /// Vertex data structure (position + color + normal)
    /// </summary>
    private struct OpenGlPoint
    {
        public float X;
        public float Y;
        public float Z;
        public float R;
        public float G;
        public float B;
        public float Nx, Ny, Nz;
        public float IsBasement;

        public OpenGlPoint(float x, float y, float z, float r, float g, float b, float nx, float ny, float nz,
            bool isBasement)
        {
            X = x;
            Y = y;
            Z = z;
            R = r;
            G = g;
            B = b;
            Nx = nx;
            Ny = ny;
            Nz = nz;
            IsBasement = isBasement ? 1 : 0;
        }
    }

    // OpenGL objects
    private uint _vbo;
    private uint _ebo;
    private uint _shaderProgram;
    private uint _vertexShader;
    private uint _fragmentShader;
    private uint _vao;

    // Shader uniforms
    private int _modelLocation;
    private int _viewLocation;
    private int _projectionLocation;
    private int _heightMultiplierLocation;
    private int _showFoundationLocation;
    private int _lightPositionLocation;
    private int _cameraPositionLocation;

    private string _glShaderVersion = "#version 300 es";
    private double[,]? _heightMap;

    // Camera parameters
    private static readonly Vector3 CameraStartPosition = new(0, 0, 75);
    private Vector3 _cameraPosition = CameraStartPosition;
    private Vector3 _cameraTarget = Vector3.Zero;

    private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
    private float _modelYaw;
    private float _modelPitch = 1.5f;
    private float _heightMultiplier = 1f;
    private bool _showFoundation = true;
    private Color _foundationColor = Colors.LightGray;
    private PaletteColorTable _colorTable;
    private int _indicesCount;
    private Vector3 _lightPosition = new(0, 0, 100);
    private float _minZoom = 1;
    private float _maxZoom = 300;
    private Color _backgroundColor;

    /// <summary>
    /// Height multiplier for the surface
    /// </summary>
    public float HeightMultiplier
    {
        get => _heightMultiplier;
        set
        {
            _heightMultiplier = value;
            OnPropertyChanged(nameof(HeightMultiplier));
            UpdateRender();
        }
    }

    public bool ShowFoundation
    {
        get => _showFoundation;
        set
        {
            _showFoundation = value;
            OnPropertyChanged(nameof(ShowFoundation));
            UpdateRender();
        }
    }

    public float ModelPitch
    {
        get => _modelPitch;
        set
        {
            _modelPitch = value;
            OnPropertyChanged(nameof(ModelPitch));
            UpdateRender();
        }
    }

    public float ModelYaw
    {
        get => _modelYaw;
        set
        {
            _modelYaw = value;
            OnPropertyChanged(nameof(ModelYaw));
            UpdateRender();
        }
    }

    public float LightPositionX
    {
        get => _lightPosition.X;
        set
        {
            _lightPosition.X = value;
            OnPropertyChanged(nameof(LightPositionX));
            UpdateRender();
        }
    }

    public float LightPositionY
    {
        get => _lightPosition.Y;
        set
        {
            _lightPosition.Y = value;
            OnPropertyChanged(nameof(LightPositionY));
            UpdateRender();
        }
    }

    public float LightPositionZ
    {
        get => _lightPosition.Z;
        set
        {
            _lightPosition.Z = value;
            OnPropertyChanged(nameof(LightPositionZ));
            UpdateRender();
        }
    }

    public float Zoom
    {
        get => Vector3.Distance(_cameraPosition, _cameraTarget);
        set
        {
            var direction = Vector3.Normalize(_cameraTarget - _cameraPosition);
            var newDistance = Math.Clamp(value, MinZoom, MaxZoom);
            _cameraPosition = _cameraTarget - direction * newDistance;
            OnPropertyChanged(nameof(Zoom));
            UpdateRender();
        }
    }

    public float MinZoom
    {
        get => _minZoom;
        set
        {
            _minZoom = value;
            OnPropertyChanged(nameof(MinZoom));
            UpdateRender();
        }
    }

    public float MaxZoom
    {
        get => _maxZoom;
        set
        {
            _maxZoom = value;
            OnPropertyChanged(nameof(MaxZoom));
            UpdateRender();
        }
    }

    public Bitmap Image { get; set; }

    /// <summary>
    /// Sets the height map for rendering
    /// </summary>
    public void SetHeightMap(double[,] heightMap)
    {
        _heightMap = heightMap;
        RegenerateModel();
    }

    private void RegenerateModel()
    {
        if (_heightMap != null)
        {
            UpdateRender();
        }
    }

    /// <summary>
    /// Creates vertex and index buffers from height map
    /// </summary>
    private void RecreateBuffersFromHeightMap()
    {
        if (_heightMap == null || _gl == null) return;

        int rows = _heightMap.GetLength(0);
        int cols = _heightMap.GetLength(1);

        var ratioX = cols / (float)rows;
        var ratioY = rows / (float)cols;

        float sizeX = 10f * (ratioX >= 1f ? 1f : ratioX);
        float sizeZ = 10f * (ratioY >= 1f ? 1f : ratioY);

        float stepX = sizeX / (cols - 1);
        float stepZ = sizeZ / (rows - 1);

        float minH = float.MaxValue;
        float maxH = float.MinValue;
        foreach (float h in _heightMap)
        {
            if (h < minH) minH = h;
            if (h > maxH) maxH = h;
        }

        float rangeH = Math.Max(maxH - minH, 0.001f);

        var normals = new Vector3[rows, cols];

        for (int z = 1; z < rows - 1; z++)
        {
            for (int x = 1; x < cols - 1; x++)
            {
                Vector3 dx = new Vector3(2 * stepX, (float)(_heightMap[z, x + 1] - _heightMap[z, x - 1]), 0);
                Vector3 dz = new Vector3(0, (float)(_heightMap[z + 1, x] - _heightMap[z - 1, x]), 2 * stepZ);
                var normal = Vector3.Normalize(Vector3.Cross(dz, dx));
                normals[z, x] = normal;
            }
        }

        // Handle borders
        for (int z = 0; z < rows; z++)
        {
            normals[z, 0] = normals[z, 1];
            normals[z, cols - 1] = normals[z, cols - 2];
        }

        for (int x = 0; x < cols; x++)
        {
            normals[0, x] = normals[1, x];
            normals[rows - 1, x] = normals[rows - 2, x];
        }

        // Create vertices
        List<OpenGlPoint> vertices = new();
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                float normH = (float)((_heightMap[z, x] - minH) / rangeH);
                float posX = (x * stepX) - sizeX / 2;
                float posZ = (z * stepZ) - sizeZ / 2;
                float posY = normH;

                int colorIndex = (int)(normH * (_colorTable.Colors.Count - 1));
                colorIndex = Math.Clamp(colorIndex, 0, _colorTable.Colors.Count - 1);
                var color = _colorTable.Colors[colorIndex];

                var normal = normals[z, x];
                vertices.Add(new OpenGlPoint(
                    posX, posY, posZ,
                    color.Red / 255f, color.Green / 255f, color.Blue / 255f,
                    normal.X, normal.Y, normal.Z, false));
            }
        }

        // Add foundation vertices
        int foundationStartIndex = vertices.Count;
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                float posX = (x * stepX) - sizeX / 2;
                float posZ = (z * stepZ) - sizeZ / 2;

                vertices.Add(new OpenGlPoint(
                    posX, 0, posZ,
                    _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                    0, 1, 0, true));
            }
        }

        // Create indices
        List<uint> indices = new();

        // Surface indices
        for (int z = 0; z < rows - 1; z++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                uint i0 = (uint)(z * cols + x);
                uint i1 = (uint)(z * cols + x + 1);
                uint i2 = (uint)((z + 1) * cols + x);
                uint i3 = (uint)((z + 1) * cols + x + 1);

                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        // Foundation indices
        for (int z = 0; z < rows - 1; z++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                uint i0 = (uint)(foundationStartIndex + z * cols + x);
                uint i1 = (uint)(foundationStartIndex + z * cols + x + 1);
                uint i2 = (uint)(foundationStartIndex + (z + 1) * cols + x);
                uint i3 = (uint)(foundationStartIndex + (z + 1) * cols + x + 1);

                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        // Добавляем вершины и индексы для боковых стенок
        int wallStartIndex = vertices.Count;

        // Создаем стенки по периметру
        // 1. Нижняя граница (z = 0)
        for (int x = 0; x < cols; x++)
        {
            // Вершина поверхности
            var surfaceVert = vertices[x];
            // Вершина фундамента
            var foundationVert = vertices[foundationStartIndex + x];

            // Добавляем обе вершины (дублируем для нормалей)
            vertices.Add(new OpenGlPoint(
                surfaceVert.X, surfaceVert.Y, surfaceVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f,
                _foundationColor.B / 255f, // Темно-серый цвет для стенок
                0, 0, -1, true)); // Нормаль наружу

            vertices.Add(new OpenGlPoint(
                foundationVert.X, foundationVert.Y, foundationVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                0, 0, -1, true));
        }

        // Индексы для нижней стенки
        for (int x = 0; x < cols - 1; x++)
        {
            uint i0 = (uint)(wallStartIndex + x * 2);
            uint i1 = (uint)(wallStartIndex + x * 2 + 1);
            uint i2 = (uint)(wallStartIndex + (x + 1) * 2);
            uint i3 = (uint)(wallStartIndex + (x + 1) * 2 + 1);

            indices.Add(i0);
            indices.Add(i2);
            indices.Add(i1);
            indices.Add(i1);
            indices.Add(i2);
            indices.Add(i3);
        }

        // 2. Верхняя граница (z = rows-1)
        wallStartIndex = vertices.Count;
        int topRowStart = (rows - 1) * cols;
        for (int x = 0; x < cols; x++)
        {
            var surfaceVert = vertices[topRowStart + x];
            var foundationVert = vertices[foundationStartIndex + topRowStart + x];

            vertices.Add(new OpenGlPoint(
                surfaceVert.X, surfaceVert.Y, surfaceVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                0, 0, 1, true));

            vertices.Add(new OpenGlPoint(
                foundationVert.X, foundationVert.Y, foundationVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                0, 0, 1, true));
        }

        // Индексы для верхней стенки
        for (int x = 0; x < cols - 1; x++)
        {
            uint i0 = (uint)(wallStartIndex + x * 2);
            uint i1 = (uint)(wallStartIndex + x * 2 + 1);
            uint i2 = (uint)(wallStartIndex + (x + 1) * 2);
            uint i3 = (uint)(wallStartIndex + (x + 1) * 2 + 1);

            indices.Add(i0);
            indices.Add(i1);
            indices.Add(i2);
            indices.Add(i1);
            indices.Add(i3);
            indices.Add(i2);
        }

        // 3. Левая граница (x = 0)
        wallStartIndex = vertices.Count;
        for (int z = 0; z < rows; z++)
        {
            var surfaceVert = vertices[z * cols];
            var foundationVert = vertices[foundationStartIndex + z * cols];

            vertices.Add(new OpenGlPoint(
                surfaceVert.X, surfaceVert.Y, surfaceVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                -1, 0, 0, true));

            vertices.Add(new OpenGlPoint(
                foundationVert.X, foundationVert.Y, foundationVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                -1, 0, 0, true));
        }

        // Индексы для левой стенки
        for (int z = 0; z < rows - 1; z++)
        {
            uint i0 = (uint)(wallStartIndex + z * 2);
            uint i1 = (uint)(wallStartIndex + z * 2 + 1);
            uint i2 = (uint)(wallStartIndex + (z + 1) * 2);
            uint i3 = (uint)(wallStartIndex + (z + 1) * 2 + 1);

            indices.Add(i0);
            indices.Add(i2);
            indices.Add(i1);
            indices.Add(i1);
            indices.Add(i2);
            indices.Add(i3);
        }

        // 4. Правая граница (x = cols-1)
        wallStartIndex = vertices.Count;
        for (int z = 0; z < rows; z++)
        {
            var surfaceVert = vertices[z * cols + cols - 1];
            var foundationVert = vertices[foundationStartIndex + z * cols + cols - 1];

            vertices.Add(new OpenGlPoint(
                surfaceVert.X, surfaceVert.Y, surfaceVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                1, 0, 0, true));

            vertices.Add(new OpenGlPoint(
                foundationVert.X, foundationVert.Y, foundationVert.Z,
                _foundationColor.R / 255f, _foundationColor.G / 255f, _foundationColor.B / 255f,
                1, 0, 0, true));
        }

        // Индексы для правой стенки
        for (int z = 0; z < rows - 1; z++)
        {
            uint i0 = (uint)(wallStartIndex + z * 2);
            uint i1 = (uint)(wallStartIndex + z * 2 + 1);
            uint i2 = (uint)(wallStartIndex + (z + 1) * 2);
            uint i3 = (uint)(wallStartIndex + (z + 1) * 2 + 1);

            indices.Add(i0);
            indices.Add(i1);
            indices.Add(i2);
            indices.Add(i1);
            indices.Add(i3);
            indices.Add(i2);
        }

        UploadBuffers(vertices.ToArray(), indices.ToArray());
    }

    private void UploadBuffers(OpenGlPoint[] vertices, uint[] indices)
    {
        _indicesCount = indices.Length;

        // Index Buffer
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        _gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * Marshal.SizeOf<uint>()),
            indices, BufferUsageARB.StaticDraw);

        // Vertex Buffer
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData<OpenGlPoint>(BufferTargetARB.ArrayBuffer,
            (nuint)(vertices.Length * Marshal.SizeOf<OpenGlPoint>()),
            vertices, BufferUsageARB.StaticDraw);

        _gl.GenVertexArrays(1, out _vao);
        _gl.BindVertexArray(_vao);

        var pointSize = Marshal.SizeOf<OpenGlPoint>();
        var floatSize = Marshal.SizeOf<float>();

        // Vertex Attributes
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)pointSize, 0);
        _gl.EnableVertexAttribArray(0);

        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)pointSize, 3 * floatSize);
        _gl.EnableVertexAttribArray(1);

        _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)pointSize, 6 * floatSize);
        _gl.EnableVertexAttribArray(2);

        _gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, (uint)pointSize, 9 * floatSize);
        _gl.EnableVertexAttribArray(3);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        _gl = GL.GetApi(gl.GetProcAddress);

        ConfigureShaders();
        RecreateBuffersFromHeightMap();

        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(DepthFunction.Less);
        
        if (this.TryFindResource("SolidBackgroundFillColorTertiary",
                ActualThemeVariant,
                out var backgroundTertiary)
            && backgroundTertiary is Color backgroundColor)
        {
            _backgroundColor = backgroundColor;
        }
        
        _gl.ClearColor(_backgroundColor.R / 255f, _backgroundColor.G / 255f, _backgroundColor.B / 255f, 1);

    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        _gl.BindVertexArray(0);
        _gl.UseProgram(0);

        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteProgram(_shaderProgram);
        _gl.DeleteShader(_vertexShader);
        _gl.DeleteShader(_fragmentShader);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        _gl.ClearColor(_backgroundColor.R / 255f, _backgroundColor.G / 255f, _backgroundColor.B / 255f, 1);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _gl.Enable(EnableCap.DepthTest);

        var window = TopLevel.GetTopLevel(this) as Window;
        int width = (int)(Bounds.Width * window?.DesktopScaling ?? 1);
        int height = (int)(Bounds.Height * window?.DesktopScaling ?? 1);

        _gl.Viewport(0, 0, (uint)width, (uint)height);

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        _gl.UseProgram(_shaderProgram);

        // Model matrix
        _modelMatrix = Matrix4x4.CreateRotationY(ModelYaw) *
                       Matrix4x4.CreateRotationX(ModelPitch) *
                       Matrix4x4.CreateScale(5f) *
                       Matrix4x4.CreateTranslation(0, 0, 0);

        // View and projection matrices
        float aspectRatio = (float)width / height;
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, aspectRatio, 0.1f, 1000f);
        Matrix4x4 view = Matrix4x4.CreateLookAt(
            _cameraPosition,
            _cameraTarget,
            Vector3.UnitY);

        _gl.UniformMatrix4(_modelLocation, 1, false, MemoryMarshal.CreateReadOnlySpan(ref _modelMatrix.M11, 16));
        _gl.UniformMatrix4(_viewLocation, 1, false, MemoryMarshal.CreateReadOnlySpan(ref view.M11, 16));
        _gl.UniformMatrix4(_projectionLocation, 1, false, MemoryMarshal.CreateReadOnlySpan(ref projection.M11, 16));

        _gl.Uniform1(_heightMultiplierLocation, _heightMultiplier);
        _gl.Uniform1(_showFoundationLocation, _showFoundation ? 1f : 0f);

        _gl.Uniform3(_lightPositionLocation, _lightPosition);
        _gl.Uniform3(_cameraPositionLocation, _cameraPosition);

        if (_heightMap != null)
        {
            _gl.DrawElements<uint>(PrimitiveType.Triangles, (uint)_indicesCount, DrawElementsType.UnsignedInt, null);
            SetFrameBufferToBitmap(width, height);
        }
    }

    private unsafe void SetFrameBufferToBitmap(int width, int height)
    {
        try
        {
            // Allocate a byte array for the pixel data
            byte[] pixels = new byte[width * height * 4]; // 4 bytes per pixel (RGBA)

            // Pin the array to get a pointer
            fixed (byte* ptr = pixels)
            {
                // Read the pixel data into the pinned array
                _gl.ReadPixels(0, 0, (uint)width, (uint)height, GLEnum.Rgba, GLEnum.UnsignedByte, ptr);
            }

            Rgba32[] pixelsRgba32 = new Rgba32[pixels.Length / 4];;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixelsRgba32[i / 4] = new Rgba32(
                    pixels[i], 
                    pixels[i + 1], 
                    pixels[i + 2], 
                    pixels[i + 3]);
            }

            // Save the pixel data to a PNG file using ImageSharp
            using var image =
                SixLabors.ImageSharp.Image.LoadPixelData(new ReadOnlySpan<Rgba32>(pixelsRgba32), width,
                    height);
            image.Mutate(ctx => ctx.Flip(FlipMode.Vertical));
            Image = image.ConvertToBitmap();
            OnPropertyChanged(nameof(Image));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load frame buffer: {ex.Message}");
        }
    }

    private void ConfigureShaders()
    {
        var version = _gl.GetStringS(StringName.Version);

        _vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(_vertexShader, VertexShaderSource);
        _gl.CompileShader(_vertexShader);
        CheckShaderCompileError(_vertexShader);

        _fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(_fragmentShader, FragmentShaderSource);
        _gl.CompileShader(_fragmentShader);
        CheckShaderCompileError(_fragmentShader);

        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, _vertexShader);
        _gl.AttachShader(_shaderProgram, _fragmentShader);
        _gl.LinkProgram(_shaderProgram);
        CheckProgramLinkError(_shaderProgram);

        // Get uniform locations
        _modelLocation = _gl.GetUniformLocation(_shaderProgram, "model");
        _viewLocation = _gl.GetUniformLocation(_shaderProgram, "view");
        _projectionLocation = _gl.GetUniformLocation(_shaderProgram, "projection");
        _heightMultiplierLocation = _gl.GetUniformLocation(_shaderProgram, "heightMultiplier");
        _showFoundationLocation = _gl.GetUniformLocation(_shaderProgram, "showFoundation");
        _lightPositionLocation = _gl.GetUniformLocation(_shaderProgram, "lightPosition");
        _cameraPositionLocation = _gl.GetUniformLocation(_shaderProgram, "cameraPosition");

        _gl.DetachShader(_shaderProgram, _vertexShader);
        _gl.DetachShader(_shaderProgram, _fragmentShader);
        _gl.DeleteShader(_vertexShader);
        _gl.DeleteShader(_fragmentShader);
    }

    private void CheckShaderCompileError(uint shader)
    {
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            throw new Exception($"Shader compilation error: {infoLog}");
        }
    }

    private void CheckProgramLinkError(uint program)
    {
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = _gl.GetProgramInfoLog(program);
            throw new Exception($"Program linking error: {infoLog}");
        }
    }

    public void RotateModel(float deltaYaw, float deltaPitch)
    {
        ModelYaw += deltaYaw;
        ModelPitch += deltaPitch;
    }

    public void ZoomCamera(float delta)
    {
        Zoom *= 1 - delta * 0.1f;
    }

    public void SetColorTable(PaletteColorTable colorTable)
    {
        _colorTable = colorTable;
    }

    public void SetCameraPreset(AxisViewType view)
    {
        switch (view)
        {
            case AxisViewType.Top:
                ModelYaw = 0;
                ModelPitch = 1.5f;
                break;
            case AxisViewType.Front:
                ModelYaw = 0;
                ModelPitch = 0;
                break;
            case AxisViewType.Side:
                ModelYaw = 1.75f;
                ModelPitch = 0;
                break;
            case AxisViewType.Isometric:
            default:
                ModelYaw = 0.8f;
                ModelPitch = 0.7f;
                break;
        }

        UpdateRender();
    }

    public void UpdateRender()
    {
        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
    }

    // Shader sources remain the same as in original
    private string VertexShaderSource => _glShaderVersion + @"
    precision mediump float;
    layout(location = 0) in vec3 aPos;
    layout(location = 1) in vec3 aColor;
    layout(location = 2) in vec3 aNormal;
    layout(location = 3) in float aIsBasement;
    uniform mat4 model;
    uniform mat4 view;
    uniform mat4 projection;
    uniform float heightMultiplier;
    out vec3 FragPos;
    out vec3 Normal;
    out vec3 VertexColor;
    out float IsBasement;
    void main()
    {
        FragPos = vec3(model * vec4(aPos.x, aPos.y * heightMultiplier, aPos.z, 1.0));
        Normal = mat3(transpose(inverse(model))) * aNormal;
        VertexColor = aColor;
        gl_Position = projection * view * vec4(FragPos, 1.0);
        IsBasement = aIsBasement;
    }";

    private string FragmentShaderSource => _glShaderVersion + @"
    precision mediump float; 
    in vec3 FragPos;
    in vec3 Normal;
    in vec3 VertexColor;
    in float IsBasement;
    uniform vec3 lightPosition;
    uniform vec3 cameraPosition;    
    uniform float showFoundation;
    out vec4 FragColor;
    void main()
    {
        if (showFoundation < 0.5 && IsBasement > 0.5) 
        {
            discard;
        }
        // Ambient
        float ambientStrength = 0.3;
        vec3 ambient = ambientStrength * VertexColor;
        // Diffuse
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(lightPosition - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * VertexColor;
        // Specular
        float specularStrength = 0.4;
        vec3 viewDir = normalize(cameraPosition - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
        vec3 specular = specularStrength * spec * vec3(1.0);
        vec3 result = IsBasement > 0.5 ? VertexColor : ambient + diffuse + specular;
        FragColor = vec4(result, 1.0);  
    }";


    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}