using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Models;

namespace NEXUS.Fractal.Helpers;

public static class FractalDimensionHelper
{
    /// <summary>
    /// Метод подсчёта кубов
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateBoxCountingDimension(this float[,] heightMap)
    {
        int height = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);
        if (width <= 0 || height <= 0) return null;

        int minBoxSize = 2;
        int maxBoxSize = Math.Min(width, height) / 2;

        // Предварительный расчёт log(1/eps) и log(N) без лишних аллокаций
        var logEps = new List<float>(EstimateCapacity(minBoxSize, maxBoxSize));
        var logN = new List<float>(logEps.Capacity);

        // Предварительно нормализуем высоты: ищем мин/макс, чтобы избежать избыточных ячеек по Z
        float minHeight = float.MaxValue, maxHeight = float.MinValue;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = heightMap[y, x];
                if (h < minHeight) minHeight = h;
                if (h > maxHeight) maxHeight = h;
            }
        }

        float heightRange = maxHeight - minHeight;
        bool constantHeight = heightRange == 0f;

        // Размер наибольшей стороны — для нормализации epsilon
        int maxDim = Math.Max(width, height);

        // Основной цикл по размерам ящиков
        for (int boxSize = minBoxSize; boxSize <= maxBoxSize; boxSize += 2)
        {
            float epsilon = (float)boxSize / maxDim;
            int boxesX = (width + boxSize - 1) / boxSize; // ceil(width / boxSize) без float
            int boxesY = (height + boxSize - 1) / boxSize;

            if (constantHeight)
            {
                // Поверхность плоская — все ячейки в одном слое Z
                logEps.Add((float)Math.Log(1.0 / epsilon));
                logN.Add((float)Math.Log(boxesX * boxesY));
                continue;
            }

            int zScale = (int)(1.0f / epsilon); // масштаб по Z: сколько ячеек в [0,1]
            // Но лучше основываться на реальном диапазоне:
            // Каждая Z-ячейка покрывает heightRange / zScale по высоте
            float zStep = heightRange / zScale;

            // Используем HashSet<long> вместо кортежа — эффективнее по памяти и хэшированию
            HashSet<long> occupied = new();

            for (int by = 0; by < boxesY; by++)
            {
                int yStart = by * boxSize;
                int yEnd = Math.Min(yStart + boxSize, height);

                for (int bx = 0; bx < boxesX; bx++)
                {
                    int xStart = bx * boxSize;
                    int xEnd = Math.Min(xStart + boxSize, width);

                    // Сканируем блок [yStart, yEnd) × [xStart, xEnd)
                    for (int y = yStart; y < yEnd; y++)
                    {
                        for (int x = xStart; x < xEnd; x++)
                        {
                            float h = heightMap[y, x];
                            int bz = (int)((h - minHeight) / zStep);
                            // Защита от граничных случаев (h == maxHeight)
                            if (bz >= zScale) bz = zScale - 1;

                            // Упаковываем (bx, by, bz) в long (предполагаем, что каждая компонента < 2^21)
                            long key = ((long)bx << 42) | ((long)by << 21) | (uint)bz;
                            occupied.Add(key);
                        }
                    }
                }
            }

            logEps.Add((float)Math.Log(1.0 / epsilon));
            logN.Add((float)Math.Log(occupied.Count));
        }

        return new FractalDimensionModel
        {
            Type = FractalDimensionType.BoxCountingFractalDimension,
            X = logEps,
            Y = logN,
        };
    }

    private static int EstimateCapacity(int min, int max) => (max - min) / 2 + 1;

    /// <summary>
    /// Метод дисперсий (размаха)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateVarianceDimension(this float[,] heightMap)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);
        int maxWindowSize = Math.Min(width, height) / 2;

        List<float> logEps = new();
        List<float> logRms = new();

        for (int window = 2; window <= maxWindowSize; window *= 2)
        {
            List<float> localVars = new();
            for (int y = 0; y <= height - window; y += window)
            {
                for (int x = 0; x <= width - window; x += window)
                {
                    float sum = 0, sumSq = 0;
                    for (int dy = 0; dy < window; dy++)
                        for (int dx = 0; dx < window; dx++)
                        {
                            float val = heightMap[y + dy, x + dx];
                            sum += val;
                            sumSq += val * val;
                        }

                    float n = window * window;
                    float mean = sum / n;
                    float var = sumSq / n - mean * mean;
                    localVars.Add(var);
                }
            }

            float rms = (float)Math.Sqrt(localVars.Average());
            float ε = (float)window / Math.Max(width, height);

            logEps.Add((float)Math.Log(1.0 / ε));
            logRms.Add((float)Math.Log(rms));
        }

        return new FractalDimensionModel
        {
            Type = FractalDimensionType.VarianceFractalDimension,
            X = logEps,
            Y = logRms,
        };
    }

    /// <summary>
    /// Метод триангуляции
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateTriangulationDimension(this float[,] heightMap)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);

        List<float> logEps = new();
        List<float> logArea = new();

        int maxStep = Math.Min(width, height) / 2;

        for (int step = 2; step <= maxStep; step *= 2)
        {
            float totalArea = 0;
            for (int y = 0; y < height - step; y += step)
            {
                for (int x = 0; x < width - step; x += step)
                {
                    // 4 вершины ячейки
                    Vector3 p1 = new(x, y, heightMap[y, x]);
                    Vector3 p2 = new(x + step, y, heightMap[y, x + step]);
                    Vector3 p3 = new(x, y + step, heightMap[y + step, x]);
                    Vector3 p4 = new(x + step, y + step, heightMap[y + step, x + step]);

                    // Два треугольника
                    totalArea += TriangleArea(p1, p2, p3);
                    totalArea += TriangleArea(p2, p4, p3);
                }
            }

            float ε = (float)step / Math.Max(width, height);
            logEps.Add((float)Math.Log(1.0 / ε));
            logArea.Add((float)Math.Log(totalArea));
        }

        return new FractalDimensionModel
        {
            Type = FractalDimensionType.TriangulationFractalDimension,
            X = logEps,
            Y = logArea,
            // EstimatedDimension = 2 + LinearRegressionSlope(logEps, logArea)
        };
    }


    public static float? CalculateDimension(List<float> x, List<float> y, FractalDimensionType type)
    {
        return type switch
        {
            FractalDimensionType.BoxCountingFractalDimension => LinearRegressionSlope(x, y),
            FractalDimensionType.VarianceFractalDimension => 2 - LinearRegressionSlope(x, y),
            FractalDimensionType.TriangulationFractalDimension => 2 + LinearRegressionSlope(x, y),
            _ => null
        };
    }

    public static string? GetDimensionName(FractalDimensionType type)
    {
        return type switch
        {
            FractalDimensionType.BoxCountingFractalDimension => "Метод подсчёта кубов",
            FractalDimensionType.VarianceFractalDimension => "Метод дисперсий",
            FractalDimensionType.TriangulationFractalDimension => "Метод триангуляции",
            _ => null
        };
    }

    /// <summary>
    /// Вычисление площади треугольника по трём 3D-точкам
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    private static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
    {
        // Векторные стороны
        float ux = b.X - a.X, uy = b.Y - a.Y, uz = b.Z - a.Z;
        float vx = c.X - a.X, vy = c.Y - a.Y, vz = c.Z - a.Z;

        // Векторное произведение
        float nx = uy * vz - uz * vy;
        float ny = uz * vx - ux * vz;
        float nz = ux * vy - uy * vx;

        // Площадь = 0.5 * |нормаль|
        return (float)(0.5 * Math.Sqrt(nx * nx + ny * ny + nz * nz));
    }

    /// <summary>
    /// Линейная регрессия
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static float LinearRegressionSlope(List<float> x, List<float> y)
    {
        int n = x.Count;
        float sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        for (int i = 0; i < n; i++)
        {
            sumX += x[i];
            sumY += y[i];
            sumXY += x[i] * y[i];
            sumX2 += x[i] * x[i];
        }

        return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
    }
}