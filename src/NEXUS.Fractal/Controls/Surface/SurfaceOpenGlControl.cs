using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Avalonia.Media.Color;

namespace NEXUS.Fractal.Controls.Surface;

/// <summary>
/// Custom OpenGL control for 3D surface rendering using Silk.NET
/// </summary>
internal partial class SurfaceOpenGlControl : OpenGlControlBase, INotifyPropertyChanged
{
    private void RecreateBuffersFromHeightMap()
    {
        if (_heightMap == null || _gl == null || _colorTable == null) return;

        int rows = _heightMap.GetLength(0), cols = _heightMap.GetLength(1);
        float sizeX = 10f * Math.Min(1f, cols / (float)rows);
        float sizeZ = 10f * Math.Min(1f, rows / (float)cols);
        float stepX = sizeX / (cols - 1), stepZ = sizeZ / (rows - 1);

        GetMinMaxHeight(_heightMap, out float minH, out float maxH);
        float rangeH = Math.Max(maxH - minH, 0.001f);
        var normals = ComputeNormals(rows, cols, stepX, stepZ, _heightMap);

        var vertices = GenerateSurfaceVertices(rows, cols, stepX, stepZ, sizeX, sizeZ, minH, rangeH, normals);
        int foundationStart = vertices.Count;
        AddFoundationVertices(vertices, rows, cols, stepX, stepZ, sizeX, sizeZ);

        var indices = new List<uint>();
        AddGridIndices(indices, rows, cols);
        AddGridIndices(indices, rows, cols, (uint)foundationStart);

        AddWalls(vertices, indices, cols, rows, foundationStart);

        UploadBuffers(vertices.ToArray(), indices.ToArray());
    }

    private static void GetMinMaxHeight(float[,] map, out float minH, out float maxH)
    {
        minH = float.MaxValue;
        maxH = float.MinValue;
        foreach (float h in map)
        {
            if (h < minH) minH = h;
            if (h > maxH) maxH = h;
        }
    }

    private Vector3[,] ComputeNormals(int rows, int cols, float stepX, float stepZ, float[,] heightMap)
    {
        var n = new Vector3[rows, cols];
        for (int z = 1; z < rows - 1; z++)
        for (int x = 1; x < cols - 1; x++)
        {
            var dx = new Vector3(2 * stepX, heightMap[z, x + 1] - heightMap[z, x - 1], 0);
            var dz = new Vector3(0, heightMap[z + 1, x] - heightMap[z - 1, x], 2 * stepZ);
            n[z, x] = Vector3.Normalize(Vector3.Cross(dz, dx));
        }

        for (int i = 0; i < rows; i++)
        {
            n[i, 0] = n[i, 1];
            n[i, cols - 1] = n[i, cols - 2];
        }

        for (int i = 0; i < cols; i++)
        {
            n[0, i] = n[1, i];
            n[rows - 1, i] = n[rows - 2, i];
        }

        return n;
    }

    private List<OpenGlPoint> GenerateSurfaceVertices(int rows, int cols, float stepX, float stepZ, float sizeX,
        float sizeZ, float minH, float rangeH, Vector3[,] normals)
    {
        var verts = new List<OpenGlPoint>();

        for (int z = 0; z < rows; z++)
        for (int x = 0; x < cols; x++)
        {
            float normH = (_heightMap[z, x] - minH) / rangeH;
            float posX = x * stepX - sizeX / 2;
            float posZ = z * stepZ - sizeZ / 2;
            float posY = normH;

            int colorIndex = GetColorIndex(normH);
            var c = _colorTable.Colors[colorIndex];
            var n = normals[z, x];

            verts.Add(new OpenGlPoint(posX, posY, posZ, c.Red / 255f, c.Green / 255f, c.Blue / 255f, n.X, n.Y, n.Z,
                false));
        }

        return verts;
    }

    private int GetColorIndex(float normH)
    {
        if (ColorTableUpperSelection != 0 || ColorTableLowerSelection != 0)
        {
            if (normH > ColorTableUpperSelection) return _colorTable.Colors.Count - 1;
            if (normH < ColorTableLowerSelection) return 0;
            normH = FrameHelper.Normalize(normH, ColorTableLowerSelection, ColorTableUpperSelection);
        }

        return Math.Clamp((int)(normH * (_colorTable.Colors.Count - 1)), 0, _colorTable.Colors.Count - 1);
    }

    private void AddFoundationVertices(List<OpenGlPoint> vertices, int rows, int cols, float stepX, float stepZ,
        float sizeX, float sizeZ)
    {
        for (int z = 0; z < rows; z++)
        for (int x = 0; x < cols; x++)
        {
            float posX = x * stepX - sizeX / 2;
            float posZ = z * stepZ - sizeZ / 2;
            vertices.Add(new OpenGlPoint(posX, 0, posZ, _foundationColor.R / 255f, _foundationColor.G / 255f,
                _foundationColor.B / 255f, 0, 1, 0, true));
        }
    }

    private void AddGridIndices(List<uint> indices, int rows, int cols, uint offset = 0)
    {
        for (int z = 0; z < rows - 1; z++)
        for (int x = 0; x < cols - 1; x++)
        {
            uint i0 = offset + (uint)(z * cols + x);
            uint i1 = i0 + 1;
            uint i2 = offset + (uint)((z + 1) * cols + x);
            uint i3 = i2 + 1;

            indices.AddRange(new[] { i0, i2, i1, i1, i2, i3 });
        }
    }

    private void AddWalls(List<OpenGlPoint> vertices, List<uint> indices, int cols, int rows, int foundationStart)
    {
        void AddWall(Func<int, (OpenGlPoint top, OpenGlPoint basePoint)> selector, Vector3 normal)
        {
            int startIdx = vertices.Count;
            for (int i = 0; i < (normal.X != 0 ? rows : cols); i++)
            {
                var (top, basePoint) = selector(i);
                vertices.Add(new OpenGlPoint(top.X, top.Y, top.Z, _foundationColor.R / 255f, _foundationColor.G / 255f,
                    _foundationColor.B / 255f, normal.X, normal.Y, normal.Z, true));
                vertices.Add(new OpenGlPoint(basePoint.X, basePoint.Y, basePoint.Z, _foundationColor.R / 255f,
                    _foundationColor.G / 255f, _foundationColor.B / 255f, normal.X, normal.Y, normal.Z, true));
            }

            for (int i = 0; i < (normal.X != 0 ? rows : cols) - 1; i++)
            {
                uint i0 = (uint)(startIdx + i * 2);
                uint i1 = i0 + 1;
                uint i2 = i0 + 2;
                uint i3 = i2 + 1;
                indices.AddRange(new[] { i0, i2, i1, i1, i2, i3 });
            }
        }

        AddWall(i => (vertices[i], vertices[foundationStart + i]), new Vector3(0, 0, -1)); // Bottom
        AddWall(i => (vertices[(rows - 1) * cols + i], vertices[foundationStart + (rows - 1) * cols + i]),
            new Vector3(0, 0, 1)); // Top
        AddWall(i => (vertices[i * cols], vertices[foundationStart + i * cols]), new Vector3(-1, 0, 0)); // Left
        AddWall(i => (vertices[i * cols + cols - 1], vertices[foundationStart + i * cols + cols - 1]),
            new Vector3(1, 0, 0)); // Right
    }

    private void UploadBuffers(OpenGlPoint[] vertices, uint[] indices)
    {
        _indicesCount = indices.Length;

        EBO = new BufferObject<uint>(_gl, indices, BufferTargetARB.ElementArrayBuffer);
        VBO = new BufferObject<OpenGlPoint>(_gl, vertices, BufferTargetARB.ArrayBuffer);
        VAO = new VertexArrayObject<OpenGlPoint, uint>(_gl, VBO, EBO);

        VAO.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 0);
        VAO.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 3);
        VAO.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, 6);
        VAO.VertexAttributePointer(3, 1, VertexAttribPointerType.Float, 9);
    }
    
    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        _gl = GL.GetApi(gl.GetProcAddress);

        Shader = new Shader(_gl);

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

        VAO.Dispose();
        VBO.Dispose();
        EBO.Dispose();

        Shader.Dispose();
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

        VAO.Bind();
        VBO.Bind();
        EBO.Bind();

        Shader.Use();

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

        Shader.UniformMatrix4("model", _modelMatrix);
        Shader.UniformMatrix4("view", view);
        Shader.UniformMatrix4("projection", projection);
        Shader.Uniform1("heightMultiplier", _heightMultiplier);
        Shader.Uniform1("showFoundation", _showFoundation ? 1f : 0f);
        Shader.Uniform3("lightPosition", new Vector3(LightPositionX, LightPositionY, LightPositionZ));
        Shader.Uniform3("cameraPosition", _cameraPosition);
        Shader.Uniform1("ambientStrength", _ambientStrength);
        Shader.Uniform1("specularStrength", _specularStrength);

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

            Rgba32[] pixelsRgba32 = new Rgba32[pixels.Length / 4];

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
            Image = ConvertToBitmap(image);
            OnPropertyChanged(nameof(Image));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load frame buffer: {ex.Message}");
        }
    }

    private Bitmap ConvertToBitmap(Image<Rgba32> image)
    {
        var stream = new MemoryStream();
        image.SaveAsBmp(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var bitmap = new Bitmap(stream);
        stream.Close();
        return bitmap;
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

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}