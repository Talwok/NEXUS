using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using NEXUS.Fractal.Models;
using NEXUS.Parsers.MDT.Models.Pallete;
using static Avalonia.OpenGL.GlConsts;

namespace NEXUS.Fractal.Controls.MdaSurface;

/// <summary>
/// Кастомный OpenGL-контрол для рендеринга 3D поверхности
/// </summary>
internal class SurfaceOpenGlControl : OpenGlControlBase, INotifyPropertyChanged
{
    /// <summary>
    /// Константы OpenGL
    /// </summary>
    private static class GlConsts
    {
        public const int GL_UNSIGNED_INT = 0x1405;
        public const int GL_CONTEXT_PROFILE_MASK = 0x9126;
    }

    /// <summary>
    /// Структура для хранения данных вершины (позиция + цвет)
    /// </summary>
    private struct OpenGlPoint
    {
        public float X; // X-координата
        public float Y; // Y-координата
        public float Z; // Z-координата
        public float R; // Красная компонента цвета
        public float G; // Зелёная компонента цвета
        public float B; // Синяя компонента цвета
        public float Nx, Ny, Nz; // Компоненты нормали

        public OpenGlPoint(float x, float y, float z, float r, float g, float b, float nx, float ny, float nz)
        {
            X = x; Y = y; Z = z;
            R = r; G = g; B = b;
            Nx = nx; Ny = ny; Nz = nz;
        }
    }

    // Идентификаторы OpenGL объектов
    private int _vbo; // Vertex Buffer Object (буфер вершин)
    private int _vao; // Vertex Array Object (массив вершин)
    private int _ebo; // Element Buffer Object (буфер индексов)
    private int _shaderProgram; // Шейдерная программа
    private int _fragmentShader; // Фрагментный шейдер
    private int _vertexShader; // Вершинный шейдер
    private int _lightPos;
    private int _viewPos;

    // Uniform-переменные шейдеров
    private int _model; // Матрица модели
    private int _view; // Матрица вида
    private int _projection; // Матрица проекции

    private string _glShaderVersion = string.Empty; // Версия GLSL шейдеров
    private float _rotation; // Угол вращения камеры
    private double[,]? _heightMap; // Карта высот для рендеринга поверхности

    // Добавляем новые параметры камеры
    private static readonly Vector3 CameraStartPosition = new(0, 0, 75);
    private Vector3 _cameraPosition = CameraStartPosition;
    private Vector3 _cameraTarget = Vector3.Zero;

    private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
    private float _modelYaw;
    private float _modelPitch = 1.5f;

    /// <summary>
    /// Устанавливает карту высот для рендеринга
    /// </summary>
    /// <param name="heightMap">2D массив высот</param>
    public void SetHeightMap(double[,] heightMap)
    {
        _heightMap = heightMap;
        
        // Можно раскомментировать для автоматического обновления:
        RequestNextFrameRendering();
    }
    
    /// <summary>
    /// Создаёт буферы вершин и индексов на основе карты высот
    /// </summary>
    /// <param name="gl">Интерфейс OpenGL</param>
    private void RecreateBuffersFromHeightMap(GlInterface gl)
    {
        if (_heightMap == null) return;

        int rows = _heightMap.GetLength(0);
        int cols = _heightMap.GetLength(1);

        // Параметры поверхности (оставляем как есть)
        float sizeX = 10f;
        float sizeZ = 10f;
        float maxHeight = 2f;

        // Шаги между вершинами
        float stepX = sizeX / (cols - 1);
        float stepZ = sizeZ / (rows - 1);

        // Находим min/max высот
        float minH = float.MaxValue;
        float maxH = float.MinValue;
        foreach (float h in _heightMap)
        {
            if (h < minH) minH = h;
            if (h > maxH) maxH = h;
        }

        float rangeH = Math.Max(maxH - minH, 0.001f);

        
        var normals = new Vector3[rows, cols];

        // Для каждой точки (x, z) вычисляем нормаль как кросс-продукт соседних высот
        for (int z = 1; z < rows - 1; z++)
        {
            for (int x = 1; x < cols - 1; x++)
            {
                Vector3 dx = new Vector3(2 * stepX, (float)(_heightMap[z, x + 1] - _heightMap[z, x - 1]) * maxHeight, 0);
                Vector3 dz = new Vector3(0, (float)(_heightMap[z + 1, x] - _heightMap[z - 1, x]) * maxHeight, 2 * stepZ);
                var normal = Vector3.Normalize(Vector3.Cross(dz, dx));
                normals[z, x] = normal;
            }
        }
        
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

        
        // Создаем вершины с центром в (0,0,0)
        List<OpenGlPoint> vertices = new();
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                float normH = (float)((_heightMap[z, x] - minH) / rangeH);
            
                // Центрируем модель:
                float posX = (x * stepX) - sizeX / 2;
                float posZ = (z * stepZ) - sizeZ / 2;
                float posY = normH * maxHeight - maxHeight/2; // Центрируем по Y

                
                // Цвет в зависимости от высоты
                int colorIndex = (int)(normH * (_colorTable.Colors.Count - 1));
                colorIndex = Math.Clamp(colorIndex, 0, _colorTable.Colors.Count - 1);
                var color = _colorTable.Colors[colorIndex];
                
                var normal = normals[z, x];
                vertices.Add(new OpenGlPoint(
                    posX, posY, posZ, 
                    color.Red / 255f, color.Green / 255f, color.Blue / 255f, 
                    normal.X, normal.Y, normal.Z));
            }
        }
        
        // Создаем индексы
        List<uint> indices = new();
        for (int z = 0; z < rows - 1; z++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                uint i0 = (uint)(z * cols + x);
                uint i1 = (uint)(z * cols + x + 1);
                uint i2 = (uint)((z + 1) * cols + x);
                uint i3 = (uint)((z + 1) * cols + x + 1);

                // Два треугольника на квад
                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        UploadBuffers(gl, vertices.ToArray(), indices.ToArray());
    }

    /// <summary>
    /// Загружает данные вершин и индексов в видеопамять
    /// </summary>
    private void UploadBuffers(GlInterface gl, OpenGlPoint[] vertices, uint[] indices)
    {
        // Размер структуры GlPoint в байтах
        int glPointBitSize = Marshal.SizeOf<OpenGlPoint>();

        // Создаем и настраиваем буфер вершин (VBO)
        if (_vbo == 0) _vbo = gl.GenBuffer();
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
        unsafe
        {
            fixed (void* pVertices = vertices)
            {
                gl.BufferData(GL_ARRAY_BUFFER, glPointBitSize * vertices.Length, (nint)pVertices, GL_STATIC_DRAW);
            }
        }

        // Создаем и настраиваем буфер индексов (EBO)
        if (_ebo == 0) _ebo = gl.GenBuffer();
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
        unsafe
        {
            fixed (void* pIndices = indices)
            {
                gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indices.Length, (nint)pIndices, GL_STATIC_DRAW);
            }
        }

        // Настраиваем атрибуты вершин в VAO
        gl.BindVertexArray(_vao);

        // Атрибут 0: позиция вершины (3 компоненты float)
        gl.VertexAttribPointer(0, 3, GL_FLOAT, 0, glPointBitSize, 0);                       // aPos
        gl.EnableVertexAttribArray(0);
        
        // Атрибут 1: цвет вершины (3 компоненты float, смещение 3*sizeof(float))
        gl.VertexAttribPointer(1, 3, GL_FLOAT, 0, glPointBitSize, 3 * sizeof(float));       // aColor
        gl.EnableVertexAttribArray(1);
        
        gl.VertexAttribPointer(2, 3, GL_FLOAT, 0, glPointBitSize, 6 * sizeof(float));       // aNormal
        gl.EnableVertexAttribArray(2);
    }

    // Свойства для управления фоном
    private SolidColorBrush _background;

    public static readonly DirectProperty<SurfaceOpenGlControl, SolidColorBrush> BackgroundProperty = AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, SolidColorBrush>(
        nameof(Background), o => o.Background, (o, v) => o.Background = v);

    public SolidColorBrush Background
    {
        get => _background;
        set => SetAndRaise(BackgroundProperty, ref _background, value);
    }

    private PaletteColorTable _colorTable;

    
    /// <summary>
    /// Инициализация OpenGL
    /// </summary>
    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // Определяем версию GLSL
        string? versionString = gl.GetString(GL_VERSION);
        _glShaderVersion = DetermineShaderVersion(versionString, gl);

        // Настраиваем шейдеры
        ConfigureShaders(gl);

        // Создаем буферы
        RecreateBuffersFromHeightMap(gl);

        // Включаем тест глубины с правильными параметрами
        gl.Enable(GL_DEPTH_TEST);

        GlCheckError(gl, "Init");
    }

    /// <summary>
    /// Деинициализация OpenGL (очистка ресурсов)
    /// </summary>
    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);

        // Отвязываем буферы
        gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        gl.BindVertexArray(0);
        gl.UseProgram(0);

        // Удаляем созданные объекты
        gl.DeleteBuffer(_vbo);
        gl.DeleteBuffer(_ebo);
        gl.DeleteVertexArray(_vao);
        gl.DeleteProgram(_shaderProgram);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteShader(_vertexShader);
    }

    /// <summary>
    /// Основной цикл рендеринга
    /// </summary>
    // Обновим метод OnOpenGlRender
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        int width = (int)(Bounds.Width * 1.5f);
        int height = (int)(Bounds.Height * 1.5f);

        gl.Viewport(0, 0, width, height);
        gl.ClearColor(0, 0, 0, 0);
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        
        gl.UseProgram(_shaderProgram);

        // Матрица модели с вращением и центрированием
        _modelMatrix = Matrix4x4.CreateRotationY(_modelYaw) * 
                       Matrix4x4.CreateRotationX(_modelPitch) * 
                       Matrix4x4.CreateScale(5f) *
                       Matrix4x4.CreateTranslation(0, 0, 0); // Явное центрирование

        // Камера смотрит строго в центр
        float aspectRatio = (float)width / height;
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, aspectRatio, 0.1f, 1000f);
        Matrix4x4 view = Matrix4x4.CreateLookAt(
            _cameraPosition,  // Камера сверху и сзади
            _cameraTarget,            // Смотрим точно в центр
            Vector3.UnitY);          // Верх камеры

        var modelMatrix = _modelMatrix;
        
        unsafe
        {
            gl.UniformMatrix4fv(_model, 1, false, &modelMatrix);
            gl.UniformMatrix4fv(_view, 1, false, &view);
            gl.UniformMatrix4fv(_projection, 1, false, &projection);
            
            // Передаем компоненты по отдельности
            gl.Uniform1f(gl.GetUniformLocationString(_shaderProgram, "lightPosX"), 30f);
            gl.Uniform1f(gl.GetUniformLocationString(_shaderProgram, "lightPosY"), 50f);
            gl.Uniform1f(gl.GetUniformLocationString(_shaderProgram, "lightPosZ"), 30f);

            gl.Uniform1f(gl.GetUniformLocationString(_shaderProgram, "cameraPositionX"), _cameraPosition.X);
            gl.Uniform1f(gl.GetUniformLocationString(_shaderProgram, "cameraPositionY"), _cameraPosition.Y);
            gl.Uniform1f(gl.GetUniformLocationString(_shaderProgram, "cameraPositionZ"), _cameraPosition.Z);
        }
        
        gl.BindVertexArray(_vao);
        gl.DrawElements(GL_TRIANGLES, 6 * (_heightMap!.GetLength(0) - 1) * (_heightMap!.GetLength(1) - 1),
            GlConsts.GL_UNSIGNED_INT, 0);

        GlCheckError(gl, "OnOpenGlRender");
    }

    public void RotateModel(float deltaYaw, float deltaPitch)
    {
        _modelYaw += deltaYaw;
        _modelPitch += deltaPitch;
        RequestNextFrameRendering();
    }

    public void ZoomCamera(float delta)
    {
        // Приближение/отдаление
        Vector3 direction = Vector3.Normalize(_cameraTarget - _cameraPosition);
        float distance = Vector3.Distance(_cameraPosition, _cameraTarget);
        float newDistance = Math.Clamp(distance * (1 - delta * 0.1f), 1.0f, 300.0f);

        _cameraPosition = _cameraTarget - direction * newDistance;
        RequestNextFrameRendering();
    }
    
    /// <summary>
    /// Настраивает шейдеры
    /// </summary>
    void ConfigureShaders(GlInterface gl)
    {
        // Создаем шейдерную программу
        _shaderProgram = gl.CreateProgram();

        // Вершинный шейдер
        _vertexShader = gl.CreateShader(GL_VERTEX_SHADER);
        GlCheckError(gl, "Create vertex shader");

        var res = gl.CompileShaderAndGetError(_vertexShader, VertexShaderSource);
        if (res != null) throw new Exception("Vertex shader compile error: " + res);

        gl.AttachShader(_shaderProgram, _vertexShader);

        // Фрагментный шейдер
        _fragmentShader = gl.CreateShader(GL_FRAGMENT_SHADER);
        GlCheckError(gl, "Create fragment shader");

        res = gl.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource);
        if (res != null)
            throw new Exception("Fragment shader compile error: " + res);

        gl.AttachShader(_shaderProgram, _fragmentShader);
        GlCheckError(gl, "Attach fragment shader");

        // Линкуем программу
        gl.LinkProgram(_shaderProgram);
        GlCheckError(gl, "Linking shader program");

        // Получаем расположение uniform-переменных
        _model = gl.GetUniformLocationString(_shaderProgram, "model");
        GlCheckError(gl, "Getting uniform model variable");

        _view = gl.GetUniformLocationString(_shaderProgram, "view");
        GlCheckError(gl, "Getting uniform view variable");

        _projection = gl.GetUniformLocationString(_shaderProgram, "projection");
        GlCheckError(gl, "Getting uniform projection variable");
        
        _lightPos = gl.GetUniformLocationString(_shaderProgram, "lightPos");
        _viewPos = gl.GetUniformLocationString(_shaderProgram, "viewPos");

    }

    /// <summary>
    /// Проверяет ошибки OpenGL
    /// </summary>
    void GlCheckError(GlInterface gl, string what = "no info")
    {
        int error = gl.GetError();
        if (error != GL_NO_ERROR) throw new Exception("GL task failed: " + what + $", ErrorCode {error}");
    }

    /// <summary>
    /// Определяет версию GLSL в зависимости от платформы
    /// </summary>
    private string DetermineShaderVersion(string? versionString, GlInterface gl)
    {
        var isOpenGlEs = !string.IsNullOrEmpty(versionString) && versionString.Contains("OpenGL ES");
        var major = 3;
        var minor = 3;

        // Парсинг основной и минорной версии
        var match = System.Text.RegularExpressions.Regex.Match(
            string.IsNullOrEmpty(versionString)
                ? string.Empty
                : versionString,
            @"(\d+)(?:\.(\d+))?");

        if (match.Success)
        {
            major = int.Parse(match.Groups[1].Value);
            minor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        }

        // Обработка для macOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !isOpenGlEs)
        {
            // Fallback для старых версий
            if (major < 3 || (major == 3 && minor < 2))
                return "#version 150 core";

            // Проверяем Core Profile
            var profile = gl.GetString(GlConsts.GL_CONTEXT_PROFILE_MASK);
            var isCoreProfile = !string.IsNullOrEmpty(profile) && profile.Contains("CORE");

            return $"#version {major}{minor}0{(isCoreProfile ? " core" : "")}";
        }

        return isOpenGlEs
            ? $"#version {major}{minor}0 es"
            : $"#version {major}{minor}0";
    }

    // Вершинный шейдер
    string VertexShaderSource => _glShaderVersion + @"
    precision mediump float;

    layout(location = 0) in vec3 aPos;
    layout(location = 1) in vec3 aColor;
    layout(location = 2) in vec3 aNormal;

    out vec3 FragPos;
    out vec3 Normal;
    out vec3 VertexColor;

    uniform mat4 model;
    uniform mat4 view;
    uniform mat4 projection;

    void main()
    {
        FragPos = vec3(model * vec4(aPos, 1.0));
        Normal = mat3(transpose(inverse(model))) * aNormal;
        VertexColor = aColor;
        gl_Position = projection * view * vec4(FragPos, 1.0);
    }";


    // Фрагментный шейдер
    string FragmentShaderSource => _glShaderVersion + @"
    precision mediump float;

    in vec3 FragPos;
    in vec3 Normal;
    in vec3 VertexColor;

    out vec4 FragColor;

    uniform float lightPosX;
    uniform float lightPosY;
    uniform float lightPosZ;

    uniform float cameraPositionX;
    uniform float cameraPositionY;
    uniform float cameraPositionZ;

    void main()
    {
        vec3 lightPos = vec3(lightPosX, lightPosY, lightPosZ);
        vec3 viewPos = vec3(cameraPositionX, cameraPositionY, cameraPositionZ);

        // Ambient
        float ambientStrength = 0.3;
        vec3 ambient = ambientStrength * VertexColor;

        // Diffuse
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(lightPos - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * VertexColor;

        // Specular
        float specularStrength = 0.4;
        vec3 viewDir = normalize(viewPos - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
        vec3 specular = specularStrength * spec * vec3(1.0); // Белый блик

        vec3 result = ambient + diffuse + specular;
        FragColor = vec4(result, 1.0);
    }";


    // Реализация INotifyPropertyChanged
    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SetColorTable(PaletteColorTable colorTable)
    {
        _colorTable = colorTable;
    }

    public void SetCameraPreset(AxisViewType view)
    {
        _cameraPosition = CameraStartPosition;
        switch (view)
        {
            case AxisViewType.Top:
                _modelYaw = 0;
                _modelPitch = 1.5f;
                break;
            case AxisViewType.Front:
                _modelYaw = 0;
                _modelPitch = 0;
                break;
            case AxisViewType.Side:
                _modelYaw = 1.75f;
                _modelPitch = 0;
                break;
            case AxisViewType.Isometric:
            default:
                _modelYaw = 0.75f;
                _modelPitch = 0.65f;
                break;
        }
        RequestNextFrameRendering();
    }
}