using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using static Avalonia.OpenGL.GlConsts;

namespace ModelRendering.Surface;

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

        /// <summary>
        /// Конструктор из вектора позиции и цвета
        /// </summary>
        public OpenGlPoint(Vector3 p, Color c) : this(p.X, p.Y, p.Z, c.R, c.G, c.B)
        {
        }

        /// <summary>
        /// Конструктор с явным заданием всех параметров
        /// </summary>
        public OpenGlPoint(float x, float y, float z, float r, float g, float b)
        {
            X = x;
            Y = y;
            Z = z;
            R = r;
            G = g;
            B = b;
        }
    }

    // Идентификаторы OpenGL объектов
    private int _vbo; // Vertex Buffer Object (буфер вершин)
    private int _vao; // Vertex Array Object (массив вершин)
    private int _ebo; // Element Buffer Object (буфер индексов)
    private int _shaderProgram; // Шейдерная программа
    private int _fragmentShader; // Фрагментный шейдер
    private int _vertexShader; // Вершинный шейдер

    // Uniform-переменные шейдеров
    private int _model; // Матрица модели
    private int _view; // Матрица вида
    private int _projection; // Матрица проекции

    private string _glShaderVersion = string.Empty; // Версия GLSL шейдеров
    private float _rotation; // Угол вращения камеры
    private float[,]? _heightMap; // Карта высот для рендеринга поверхности

    // Добавляем новые параметры камеры
    private float _cameraDistance = 5.0f;
    private float _cameraYaw;
    private float _cameraPitch;
    private Vector3 _cameraTarget = Vector3.Zero;
    private Vector3 _panOffset = Vector3.Zero;

    public float CameraDistance
    {
        get => _cameraDistance;
        set
        {
            _cameraDistance = Math.Max(1.0f, value);
            RequestNextFrameRendering();
        }
    }

    public float CameraYaw
    {
        get => _cameraYaw;
        set
        {
            _cameraYaw = value;
            RequestNextFrameRendering();
        }
    }

    public float CameraPitch
    {
        get => _cameraPitch;
        set
        {
            // Ограничиваем угол наклона, чтобы не переворачивать камеру
            _cameraPitch = Math.Clamp(value, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f);
            RequestNextFrameRendering();
        }
    }

    public Vector3 PanOffset
    {
        get => _panOffset;
        set
        {
            _panOffset = value;
            RequestNextFrameRendering();
        }
    }

    /*protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        int width = (int)Bounds.Width * 2;
        int height = (int)Bounds.Height * 2;

        // Устанавливаем viewport на всю область контрола
        gl.Viewport(0, 0, width, height);

        gl.ClearColor(Background.R, Background.G, Background.B, Background.A);
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        gl.UseProgram(_shaderProgram);

        // Рассчитываем позицию камеры на сфере
        float camX = _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Cos(_cameraYaw);
        float camY = _cameraDistance * MathF.Sin(_cameraPitch);
        float camZ = _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Sin(_cameraYaw);

        Vector3 cameraPos = new Vector3(camX, camY, camZ);
        Vector3 cameraTarget = _cameraTarget + _panOffset;
        Vector3 cameraUp = Vector3.UnitY;

        // Матрица модели (преобразования объекта)
        Matrix4x4 model = Matrix4x4.Identity;

        // Перспективная проекция (исправляет искажения)
        float aspectRatio = (float)width / height;
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 4, // Угол обзора 45 градусов
            aspectRatio,
            0.1f,         // Ближняя плоскость отсечения
            1000.0f        // Дальняя плоскость отсечения
        );

        // Матрица вида (камеры)
        Matrix4x4 view = Matrix4x4.CreateLookAt(
            cameraPos,
            cameraTarget,
            cameraUp
        );

        // Передаем матрицы в шейдеры
        unsafe
        {
            gl.UniformMatrix4fv(_model, 1, false, &model);
            gl.UniformMatrix4fv(_view, 1, false, &view);
            gl.UniformMatrix4fv(_projection, 1, false, &projection);
        }

        // Рисуем объект
        gl.BindVertexArray(_vao);
        gl.DrawElements(GL_TRIANGLES, 36, GlConsts.GL_UNSIGNED_INT, 0);

        GlCheckError(gl, "OnOpenGlRender");
    }
    */

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        int width = (int)Bounds.Width * 2;
        int height = (int)Bounds.Height * 2;

        gl.Viewport(0, 0, width, height);
        gl.ClearColor(Background.R, Background.G, Background.B, Background.A);
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        gl.UseProgram(_shaderProgram);

        // Камера (вращение вокруг _center)
        float camX = _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Cos(_cameraYaw);
        float camY = _cameraDistance * MathF.Sin(_cameraPitch);
        float camZ = _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Sin(_cameraYaw);

        Vector3 cameraPos = _cameraTarget + new Vector3(camX, camY, camZ);
        Vector3 cameraTarget = _cameraTarget + _panOffset;
        Vector3 cameraUp = Vector3.UnitY;

        Matrix4x4 model = Matrix4x4.CreateScale(5f); // масштабируем модель
        float aspectRatio = (float)width / height;
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, aspectRatio, 0.1f, 1000f);
        Matrix4x4 view = Matrix4x4.CreateLookAt(cameraPos, cameraTarget, cameraUp);

        unsafe
        {
            gl.UniformMatrix4fv(_model, 1, false, &model);
            gl.UniformMatrix4fv(_view, 1, false, &view);
            gl.UniformMatrix4fv(_projection, 1, false, &projection);
        }
        
        gl.BindVertexArray(_vao);
        gl.DrawElements(GL_TRIANGLES, 6 * (_heightMap!.GetLength(0) - 1) * (_heightMap!.GetLength(1) - 1),
            GlConsts.GL_UNSIGNED_INT, 0);

        GlCheckError(gl, "OnOpenGlRender");
    }

    /// <summary>
    /// Устанавливает карту высот для рендеринга
    /// </summary>
    /// <param name="heightMap">2D массив высот</param>
    public void SetHeightMap(float[,] heightMap)
    {
        _heightMap = heightMap;
        // Можно раскомментировать для автоматического обновления:
        // RequestNextFrameRendering();
    }

    /// <summary>
    /// Создаёт буферы вершин и индексов на основе карты высот
    /// </summary>
    /// <param name="gl">Интерфейс OpenGL</param>
    /*private void RecreateBuffersFromHeightMap(GlInterface gl)
    {
        if (_heightMap == null) return;

        // Получаем размеры карты высот
        int rows = _heightMap.GetLength(0);
        int cols = _heightMap.GetLength(1);

        // Коэффициенты масштабирования
        float scaleXZ = 5f; // Масштаб по осям X и Z (чем больше - шире поверхность)
        float scaleY = 2.0f; // Масштаб по оси Y (чем больше - выше перепады высот)

        // Находим минимальную и максимальную высоту для нормализации
        float min = float.MaxValue, max = float.MinValue;
        foreach (float h in _heightMap)
        {
            if (h < min) min = h;
            if (h > max) max = h;
        }

        float range = (max - min > 0) ? (max - min) : 1;

        // Генерируем вершины
        List<OpenGlPoint> vertices = new();
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Нормализуем высоту и применяем масштаб
                float rawHeight = _heightMap[z, x];
                float normalized = (rawHeight - min) / range;
                float y = normalized * scaleY;

                // Позиция вершины
                float posX = x * scaleXZ;
                float posZ = z * scaleXZ;

                // Цвет вершины (градиент от черного к белому в зависимости от высоты)
                float r = normalized;
                float g = normalized;
                float b = normalized;

                vertices.Add(new OpenGlPoint(posX, y, posZ, r, g, b));
            }
        }

        // Генерируем индексы для треугольников
        List<uint> indices = new();
        for (int z = 0; z < rows - 1; z++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                // Индексы 4 вершин текущего квада
                uint i0 = (uint)(z * cols + x);
                uint i1 = (uint)(z * cols + x + 1);
                uint i2 = (uint)((z + 1) * cols + x);
                uint i3 = (uint)((z + 1) * cols + x + 1);

                // Разбиваем квад на 2 треугольника
                indices.AddRange([i0, i1, i2, i2, i1, i3]);
            }
        }

        // Загружаем данные в буферы
        UploadBuffers(gl, vertices.ToArray(), indices.ToArray());
    }
    */
    private void RecreateBuffersFromHeightMap(GlInterface gl)
    {
        if (_heightMap == null) return;

        int rows = _heightMap.GetLength(0);
        int cols = _heightMap.GetLength(1);

        // Параметры поверхности
        float sizeX = 10f; // общий размер по X
        float sizeZ = 10f; // общий размер по Z
        float maxHeight = 2f; // максимальная высота

        // Вычисляем шаг между вершинами
        float stepX = sizeX / (cols - 1);
        float stepZ = sizeZ / (rows - 1);

        // Находим min/max высот для нормализации
        float minH = float.MaxValue;
        float maxH = float.MinValue;
        foreach (float h in _heightMap)
        {
            if (h < minH) minH = h;
            if (h > maxH) maxH = h;
        }

        float rangeH = Math.Max(maxH - minH, 0.001f);

        // Создаем вершины
        List<OpenGlPoint> vertices = new();
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Нормализованная высота [0..1]
                float normH = (_heightMap[z, x] - minH) / rangeH;

                // Позиция вершины с центром в (0,0,0)
                float posX = (x * stepX) - sizeX / 2;
                float posZ = (z * stepZ) - sizeZ / 2;
                float posY = normH * maxHeight;

                // Цвет в зависимости от высоты
                float r = normH;
                float g = 0.5f - normH * 0.3f;
                float b = 1f - normH;

                vertices.Add(new OpenGlPoint(posX, posY, posZ, r, g, b));
            }
        }

        // Центр модели (для камеры)
        _cameraTarget = new Vector3(0, maxHeight / 2, 0);

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
        gl.VertexAttribPointer(0, 3, GL_FLOAT, 0, glPointBitSize, nint.Zero);
        gl.EnableVertexAttribArray(0);

        // Атрибут 1: цвет вершины (3 компоненты float, смещение 3*sizeof(float))
        gl.VertexAttribPointer(1, 3, GL_FLOAT, 0, glPointBitSize, 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);
    }

    // Свойства для управления фоном

    public Color Background { get; set; }

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

        // Включаем тест глубины
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
        gl.DeleteVertexArray(_vao);
        gl.DeleteProgram(_shaderProgram);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteShader(_vertexShader);
    }

    /// <summary>
    /// Основной цикл рендеринга
    /// </summary>
    /*protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        // Получаем размеры области рисования
        int width = (int)Bounds.Width;
        int height = (int)Bounds.Height;

        // Рассчитываем соотношение сторон
        float aspectRatio = (float)width / height;
        float projWidth = 6;
        float projHeight = projWidth / aspectRatio; // Корректируем высоту относительно ширины

        // Очищаем буферы цвета и глубины
        gl.ClearColor(Background.R, Background.G, Background.B, Background.A);
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        // Активируем шейдерную программу
        gl.UseProgram(_shaderProgram);
        gl.Viewport(0, 0, width, height);

        // Позиция камеры (вращается по окружности)
        float radius = 5;
        float camX = radius * MathF.Cos(_rotation);
        float camY = 1f;
        float camZ = radius * MathF.Sin(_rotation);

        // Настраиваем матрицы
        Vector3 cameraPos = new Vector3(camX, camY, camZ);
        Vector3 cameraTarget = new Vector3(0, 0, 0);
        Vector3 cameraUpVector = Vector3.UnitY;

        // Матрица модели (пока единичная)
        Matrix4x4 model = Matrix4x4.Identity;

        // Ортографическая проекция
        Matrix4x4 projection = Matrix4x4.CreateOrthographic(
            projWidth,
            projHeight,
            0.1f,
            10.0f
        );

        // Матрица вида (камера)
        Matrix4x4 view = Matrix4x4.CreateLookAt(
            cameraPos,
            cameraTarget,
            cameraUpVector
        );

        // Передаем матрицы в шейдеры
        unsafe
        {
            gl.UniformMatrix4fv(_model, 1, false, &model);
            gl.UniformMatrix4fv(_view, 1, false, &view);
            gl.UniformMatrix4fv(_projection, 1, false, &projection);
        }

        // Рисуем поверхность
        gl.BindVertexArray(_vao);
        gl.DrawElements(GL_TRIANGLES, 36, GlConsts.GL_UNSIGNED_INT, 0);

        GlCheckError(gl, "OnOpenGlRender");
    }
    */
    // Модифицируйте метод OnOpenGlRender
    /*protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        int width = (int)Bounds.Width;
        int height = (int)Bounds.Height;

        // Рассчитываем соотношение сторон с учетом зума
        float aspectRatio = (float)width / height;
        float projWidth = 6.0f * _zoom;
        float projHeight = projWidth / aspectRatio;

        gl.ClearColor(Background.R, Background.G, Background.B, Background.A);
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        gl.UseProgram(_shaderProgram);
        gl.Viewport(0, 0, width, height);

        float radius = 5;
        float camX = radius * MathF.Cos(_rotation);
        float camY = _cameraY; // Используем свойство CameraY вместо фиксированного значения
        float camZ = radius * MathF.Sin(_rotation);

        Vector3 cameraPos = new Vector3(camX, camY, camZ);
        Vector3 cameraTarget = _panOffset; // Добавляем смещение панорамирования
        Vector3 cameraUpVector = Vector3.UnitY;

        Matrix4x4 model = Matrix4x4.Identity;
        Matrix4x4 projection = Matrix4x4.CreateOrthographic(
            projWidth,
            projHeight,
            0.1f,
            10.0f
        );
        Matrix4x4 view = Matrix4x4.CreateLookAt(
            cameraPos,
            cameraTarget,
            cameraUpVector
        );

        unsafe
        {
            gl.UniformMatrix4fv(_model, 1, false, &model);
            gl.UniformMatrix4fv(_view, 1, false, &view);
            gl.UniformMatrix4fv(_projection, 1, false, &projection);
        }

        gl.BindVertexArray(_vao);
        gl.DrawElements(GL_TRIANGLES, 36, GlConsts.GL_UNSIGNED_INT, 0);

        GlCheckError(gl, "OnOpenGlRender");
    }*/
    /// <summary>
    /// Создает буферы для тестового куба (не используется для поверхности)
    /// </summary>
    protected void CreateVertexBuffer(GlInterface gl)
    {
        // Создаем VAO (Vertex Array Object)
        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);
        GlCheckError(gl, "Create VAO 1");

        // Вершины куба (8 точек с цветами)
        OpenGlPoint[] vertices =
        [
            new OpenGlPoint(-1.0f, -1.0f, -1.0f, 1.0f, 0.0f, 0.0f), // 0
            new OpenGlPoint(1.0f, -1.0f, -1.0f, 0.0f, 1.0f, 0.0f), // 1
            new OpenGlPoint(1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 1.0f), // 2
            new OpenGlPoint(-1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 0.0f), // 3
            new OpenGlPoint(-1.0f, -1.0f, 1.0f, 1.0f, 0.0f, 1.0f), // 4
            new OpenGlPoint(1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 1.0f), // 5
            new OpenGlPoint(1.0f, 1.0f, 1.0f, 0.5f, 0.5f, 0.5f), // 6
            new OpenGlPoint(-1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.2f) // 7
        ];

        // Индексы для 12 треугольников (6 граней куба)
        uint[] indices =
        {
            // Передняя грань
            0, 1, 2, 2, 3, 0,
            // Задняя грань
            4, 5, 6, 6, 7, 4,
            // Верхняя грань
            3, 2, 6, 6, 7, 3,
            // Нижняя грань
            0, 1, 5, 5, 4, 0,
            // Левая грань
            0, 3, 7, 7, 4, 0,
            // Правая грань
            1, 2, 6, 6, 5, 1
        };

        // Создаем и заполняем VBO (Vertex Buffer Object)
        int glPointBitSize = Marshal.SizeOf<OpenGlPoint>();
        int verticesBitSize = glPointBitSize * vertices.Length;

        _vbo = gl.GenBuffer();
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);

        unsafe
        {
            fixed (void* pVertices = vertices)
            {
                gl.BufferData(GL_ARRAY_BUFFER, verticesBitSize, (nint)pVertices, GL_STATIC_DRAW);
            }
        }

        GlCheckError(gl, "Create VBO");

        // Создаем и заполняем EBO (Element Buffer Object)
        int indicesBitSize = sizeof(uint) * indices.Length;
        _ebo = gl.GenBuffer();
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
        unsafe
        {
            fixed (void* pIndices = indices)
            {
                gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, indicesBitSize, (nint)pIndices, GL_STATIC_DRAW);
            }
        }

        GlCheckError(gl, "Create EBO");

        // Настраиваем атрибуты вершин
        gl.VertexAttribPointer(0, 3, GL_FLOAT, 0, glPointBitSize, nint.Zero);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 3, GL_FLOAT, 0, glPointBitSize, 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        GlCheckError(gl, "Create VAO 2");
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
    layout (location = 0) in vec3 aPos;    // Атрибут позиции
    layout (location = 1) in vec3 aColor;  // Атрибут цвета
    out vec3 ourColor;                     // Выходящая переменная цвета
    uniform mat4 model;                    // Матрица модели
    uniform mat4 view;                     // Матрица вида
    uniform mat4 projection;               // Матрица проекции
    
    void main()
    {
        // Преобразуем позицию вершины
        gl_Position = projection * view * model * vec4(aPos, 1.0);
        // Передаем цвет во фрагментный шейдер
        ourColor = aColor;
    }";

    // Фрагментный шейдер
    string FragmentShaderSource => _glShaderVersion + @"
    precision mediump float;
    in vec3 ourColor;    // Входная переменная цвета из вершинного шейдера
    out vec4 FragColor;  // Выходной цвет фрагмента
    
    void main()
    {
        // Просто используем переданный цвет
        FragColor = vec4(ourColor, 1.0);
    }";

    /// <summary>
    /// Угол вращения камеры
    /// При изменении вызывает обновление отображения
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        set
        {
            if (Math.Abs(_rotation - value) > Double.Epsilon)
            {
                _rotation = (float)value;
                OnPropertyChanged(nameof(Rotation));
                RequestNextFrameRendering();
            }
        }
    }

    // Реализация INotifyPropertyChanged
    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}