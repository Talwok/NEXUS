using System.Numerics;
using MIConvexHull;
using NEXUS.Parsers.MDT.Models.Pallete;
using NEXUS.Parsers.Ovito.Models.XYZFile;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Parsers.Ovito.Helpers;

public record MinMaxLimits(
    double MinValue,
    double MaxValue,
    double MinLimit,
    double MaxLimit);

public record MinMax(
    double MinValue,
    double MaxValue);

public static class XyzFramesPipeline
{
    public static XyzFrameImageProcessor CreateFromXyzFrame(this XYZFrame file)
        => new XyzFrameImageProcessor(file);
}

public class XyzFrameImageProcessor
{
    private readonly XYZFrame _frame;
    private MinMax _originalRange;
    private MinMax _currentRange;
    private Image<Rgba32> _image;
    private Rgba32 _belowThresholdColor = new(0, 0, 0, 255); // Черный для значений ниже минимума
    private Rgba32 _aboveThresholdColor = new(255, 255, 255, 255); // Белый для значений выше максимума
    private float[,]? _heightMap;

    public XyzFrameImageProcessor(XYZFrame frame)
    {
        _frame = frame;
        _heightMap = ConvertToFloatHeightMap(frame.Particles);
        _originalRange = CalculateDataRange();
        _currentRange = _originalRange; // По умолчанию используем полный диапазон
        _image = CreateBaseImage();
    }

    // MIConvexHull vertex
    private class Vertex : IVertex
    {
        public double[] Position { get; set; }
        public Vertex(Particle v) => Position = [(double)v.X, (double)v.Y, (double)v.Z];
    }

    /// <summary>
    /// Создаёт плотную "накинутую ткань" сверху → float[,] heightmap
    /// </summary>
    /// <summary>
    /// Создаёт плотную "накинутую ткань" сверху → float[,] heightmap
    /// ВНИМАНИЕ: В твоих данных Z = высота (Y), Y = глубина (Z)
    /// </summary>
    public static float[,] ConvertToFloatHeightMap(List<Particle> points, float? cellSize = null)
    {
        if (points.Count < 4) throw new ArgumentException("Нужно минимум 4 точки.");

        float minX = points.Min(p => p.X), maxX = points.Max(p => p.X);
        float minY = points.Min(p => p.Y), maxY = points.Max(p => p.Y);

        float autoCellSize = (float)Math.Sqrt((maxX - minX) * (maxY - minY) / points.Count) * 2f;
        cellSize ??= Math.Max(0.1f, autoCellSize);

        var vertices = points.Select(p => new Vertex(p)).ToArray();

        // 1. Delaunay триангуляция
        var triangulation = Triangulation.CreateDelaunay(vertices);
        var upperFaces = new List<(Vector3 a, Vector3 b, Vector3 c)>();

        foreach (var cell in triangulation.Cells)
        {
            var v0 = new Vector3(
                (float)cell.Vertices[0].Position[0], // X
                (float)cell.Vertices[0].Position[2], // Z → высота
                (float)cell.Vertices[0].Position[1] // Y → глубина
            );
            var v1 = new Vector3(
                (float)cell.Vertices[1].Position[0],
                (float)cell.Vertices[1].Position[2],
                (float)cell.Vertices[1].Position[1]
            );
            var v2 = new Vector3(
                (float)cell.Vertices[2].Position[0],
                (float)cell.Vertices[2].Position[2],
                (float)cell.Vertices[2].Position[1]
            );

            // Фильтр по радиусу описанной окружности
            double r = Circumradius(v0, v1, v2);
            if (r > cellSize * 6) continue;

            upperFaces.Add((v0, v1, v2));
        }

        if (!upperFaces.Any())
            throw new InvalidOperationException(
                "Нет верхних граней. Попробуй увеличить cellSize или проверить данные.");

        // 2. Границы по X и Y

        float width = maxX - minX;
        float depth = maxY - minY;

        int gridX = Math.Max(1, (int)Math.Ceiling(width / cellSize.Value));
        int gridY = Math.Max(1, (int)Math.Ceiling(depth / cellSize.Value));

        var heightmap = new float[gridX, gridY];
        var hits = new int[gridX, gridY];

        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridY; z++)
                heightmap[x, z] = float.NegativeInfinity;

        // 3. Растеризация
        foreach (var (a, b, c) in upperFaces)
        {
            RasterizeTriangle(a, b, c, minX, minY, cellSize.Value, gridX, gridY, ref heightmap, ref hits);
        }

        // 4. Заполнение + сглаживание
        FillAndSmooth(ref heightmap, hits);

        return heightmap;
    }

    // Вспомогательные
    private static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X
    );

    private static double Circumradius(Vector3 a, Vector3 b, Vector3 c)
    {
        double ab = DistSq(b, c), bc = DistSq(c, a), ca = DistSq(a, b);
        double area = 0.5 * Cross(b - a, c - a).Length();
        if (area < 1e-6) return double.MaxValue;
        return (ab * bc * ca) / (16 * area * area);
    }

    private static double DistSq(Vector3 a, Vector3 b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
        return dx * dx + dy * dy + dz * dz;
    }

    private static void RasterizeTriangle(
        Vector3 a, Vector3 b, Vector3 c,
        float minX, float minY, float cellSize,
        int gridX, int gridZ,
        ref float[,] heightmap, ref int[,] hits)
    {
        float minTx = Math.Min(a.X, Math.Min(b.X, c.X));
        float maxTx = Math.Max(a.X, Math.Max(b.X, c.X));
        float minTz = Math.Min(a.Z, Math.Min(b.Z, c.Z));
        float maxTz = Math.Max(a.Z, Math.Max(b.Z, c.Z));

        int startX = Math.Max(0, (int)((minTx - minX) / cellSize));
        int endX = Math.Min(gridX - 1, (int)((maxTx - minX) / cellSize));
        int startZ = Math.Max(0, (int)((minTz - minY) / cellSize));
        int endZ = Math.Min(gridZ - 1, (int)((maxTz - minY) / cellSize));

        for (int x = startX; x <= endX; x++)
            for (int z = startZ; z <= endZ; z++)
            {
                float wx = minX + x * cellSize + cellSize * 0.5f;
                float wz = minY + z * cellSize + cellSize * 0.5f;

                var bary = Barycentric(a, b, c, new Vector3(wx, 0, wz));
                if (bary.X < 0 || bary.Y < 0 || bary.Z < 0) continue;

                float height = bary.X * a.Y + bary.Y * b.Y + bary.Z * c.Y; // Y — высота!

                if (height > heightmap[x, z])
                {
                    heightmap[x, z] = height;
                    hits[x, z]++;
                }
            }
    }

    private static (float X, float Y, float Z) Barycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
        var v0 = b - a;
        var v1 = c - a;
        var v2 = p - a;
        float d00 = v0.X * v0.X + v0.Z * v0.Z;
        float d01 = v0.X * v1.X + v0.Z * v1.Z;
        float d11 = v1.X * v1.X + v1.Z * v1.Z;
        float d20 = v2.X * v0.X + v2.Z * v0.Z;
        float d21 = v2.X * v1.X + v2.Z * v1.Z;
        float denom = d00 * d11 - d01 * d01;
        if (Math.Abs(denom) < 1e-6) return (0, 0, 0);
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1 - v - w;
        return (u, v, w);
    }

    private static void FillAndSmooth(ref float[,] map, int[,] hits)
    {
        int w = map.GetLength(0), h = map.GetLength(1);

        // Заполняем пустоты
        for (int x = 0; x < w; x++)
            for (int z = 0; z < h; z++)
            {
                if (hits[x, z] > 0) continue;
                float sum = 0;
                int count = 0;
                for (int dx = -2; dx <= 2; dx++)
                    for (int dz = -2; dz <= 2; dz++)
                    {
                        int nx = x + dx, nz = z + dz;
                        if (nx >= 0 && nx < w && nz >= 0 && nz < h && hits[nx, nz] > 0)
                        {
                            sum += map[nx, nz];
                            count++;
                        }
                    }

                if (count > 0) map[x, z] = sum / count;
            }

        // Сглаживание
        var temp = (float[,])map.Clone();
        for (int x = 1; x < w - 1; x++)
            for (int z = 1; z < h - 1; z++)
            {
                float sum = 0;
                for (int dx = -1; dx <= 1; dx++)
                    for (int dz = -1; dz <= 1; dz++)
                        sum += temp[x + dx, z + dz];
                map[x, z] = sum / 9f;
            }
    }

    public XyzFrameImageProcessor WithRange(double min, double max)
    {
        _currentRange = new MinMax(
            Math.Max(min, _originalRange.MinValue),
            Math.Min(max, _originalRange.MaxValue));
        return this;
    }

    public Image<Rgba32> ApplyColorMap(PaletteColorTable colorTable)
    {
        var colors = colorTable.Colors
            .Select(c => new Rgba32(c.Red, c.Green, c.Blue, 255))
            .ToArray();

        return ApplyColorMap(colors);
    }

    private Image<Rgba32> ApplyColorMap(Rgba32[] colorMap)
    {
        _belowThresholdColor = colorMap.First();
        _aboveThresholdColor = colorMap.Last();
        var colorImage = _image.Clone();
        var range = _currentRange.MaxValue - _currentRange.MinValue;

        colorImage.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    double value = _heightMap[y, x];

                    // Обработка значений за границами диапазона
                    if (value < _currentRange.MinValue)
                    {
                        row[x] = _belowThresholdColor;
                        continue;
                    }

                    if (value > _currentRange.MaxValue)
                    {
                        row[x] = _aboveThresholdColor;
                        continue;
                    }

                    // Нормализация в текущем диапазоне
                    double normalized = (value - _currentRange.MinValue) / range;
                    int colorIndex = (int)(normalized * (colorMap.Length - 1));
                    colorIndex = Math.Clamp(colorIndex, 0, colorMap.Length - 1);
                    row[x] = colorMap[colorIndex];
                }
            }
        });

        return colorImage;
    }

    public MinMax GetOriginalRange() => _originalRange;
    public MinMax GetCurrentRange() => _currentRange;

    private Image<Rgba32> CreateBaseImage()
    {
        return new Image<Rgba32>(_heightMap.GetLength(0), _heightMap.GetLength(1));
    }

    private MinMax CalculateDataRange()
    {
        double min = 0;
        double max = 0;

        for (int i = 0; i < _heightMap.GetLength(0); i++)
        {
            for (int j = 0; j < _heightMap.GetLength(1); j++)
            {
                var value = _heightMap[i, j];

                if (i == 0)
                    min = max = value;

                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        return new MinMax(min, max);
    }

    public float[,] GetHeightMap() => _heightMap ??= ConvertToFloatHeightMap(_frame.Particles);
}