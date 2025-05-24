using System;
using System.Collections.Generic;
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
    public static FractalDimensionModel? CalculateBoxCountingDimension(this float[,] data)
    {
        int sizeX = data.GetLength(0);
        int sizeY = data.GetLength(1);
        int minSize = Math.Min(sizeX, sizeY);
        List<float> logEpsilons = new();
        List<float> logCounts = new();

        for (int boxSize = 1; boxSize <= minSize / 2; boxSize *= 2)
        {
            int count = 0;

            for (int i = 0; i < sizeX; i += boxSize)
            {
                for (int j = 0; j < sizeY; j += boxSize)
                {
                    float min = float.MaxValue;
                    float max = float.MinValue;

                    for (int x = i; x < Math.Min(i + boxSize, sizeX); x++)
                    {
                        for (int y = j; y < Math.Min(j + boxSize, sizeY); y++)
                        {
                            float val = data[x, y];
                            min = Math.Min(min, val);
                            max = Math.Max(max, val);
                        }
                    }

                    if (max > min)
                        count++;
                }
            }

            if (count > 0)
            {
                logEpsilons.Add((float)Math.Log(1.0 / boxSize));
                logCounts.Add((float)Math.Log(count));
            }
        }

        return new FractalDimensionModel
        {
            Type = FractalDimensionType.BoxCountingFractalDimension,
            X = logEpsilons,
            Y = logCounts,
            Dimension = LinearRegressionSlope(logEpsilons, logCounts)
        };
        ;
    }

    /// <summary>
    /// Метод дисперсий (размаха)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateVarianceDimension(this float[,] data)
    {
        int sizeX = data.GetLength(0);
        int sizeY = data.GetLength(1);
        int minSize = Math.Min(sizeX, sizeY);
        List<float> logScales = new();
        List<float> logVariances = new();

        for (int scale = 2; scale <= minSize / 2; scale *= 2)
        {
            List<float> variances = new();

            for (int i = 0; i <= sizeX - scale; i += scale)
            {
                for (int j = 0; j <= sizeY - scale; j += scale)
                {
                    float sum = 0;
                    float sumSq = 0;
                    int count = 0;

                    for (int x = i; x < i + scale; x++)
                    {
                        for (int y = j; y < j + scale; y++)
                        {
                            float val = data[x, y];
                            sum += val;
                            sumSq += val * val;
                            count++;
                        }
                    }

                    float mean = sum / count;
                    float variance = (sumSq / count) - (mean * mean);
                    variances.Add(variance);
                }
            }

            float avgVariance = 0;
            foreach (var v in variances)
                avgVariance += v;
            avgVariance /= variances.Count;

            logScales.Add((float)Math.Log(1.0 / scale));
            logVariances.Add((float)Math.Log(avgVariance));
        }

        float slope = LinearRegressionSlope(logScales, logVariances);

        return new FractalDimensionModel
        {
            Type = FractalDimensionType.VarianceFractalDimension,
            X = logScales,
            Y = logVariances,
            Dimension = (float)(2 - slope / 2.0)
        };
    }

    /// <summary>
    /// Метод Масса-Масштаб
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateMassScaleDimension(this float[,] data)
    {
        int sizeX = data.GetLength(0);
        int sizeY = data.GetLength(1);
        int minSize = Math.Min(sizeX, sizeY);

        List<float> logEpsilons = new();
        List<float> logMasses = new();

        for (int scale = 1; scale <= minSize / 2; scale *= 2)
        {
            float totalMass = 0;

            for (int i = 0; i < sizeX; i += scale)
            {
                for (int j = 0; j < sizeY; j += scale)
                {
                    float sum = 0;

                    for (int x = i; x < Math.Min(i + scale, sizeX); x++)
                    {
                        for (int y = j; y < Math.Min(j + scale, sizeY); y++)
                        {
                            sum += data[x, y];
                        }
                    }

                    if (sum > 0)
                        totalMass += sum;
                }
            }

            logEpsilons.Add((float)Math.Log(1.0 / scale));
            logMasses.Add((float)Math.Log(totalMass));
        }

        return new FractalDimensionModel
        {
            Type = FractalDimensionType.MassScaleFractalDimension,
            X = logEpsilons,
            Y = logMasses,
            Dimension = LinearRegressionSlope(logEpsilons, logMasses)
        };
    }

    /// <summary>
    /// Метод Хигучи (адаптирован под двумерные данные как линейное приближение)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateHiguchiDimension(this float[,] data)
    {
        List<float> logL = new();
        List<float> logK = new();
        int N = data.GetLength(0) * data.GetLength(1);
        List<float> flat = new(data.Length);
        foreach (float f in data) flat.Add(f);

        for (int k = 1; k <= N / 4; k *= 2)
        {
            float Lk = 0;
            for (int m = 0; m < k; m++)
            {
                float length = 0;
                int n = 0;

                for (int i = m + k; i < flat.Count; i += k)
                {
                    length += Math.Abs(flat[i] - flat[i - k]);
                    n++;
                }

                if (n > 0)
                    Lk += (length * (flat.Count - 1)) / (n * k);
            }

            Lk /= k;
            logK.Add((float)Math.Log(1.0 / k));
            logL.Add((float)Math.Log(Lk));
        }

        return new FractalDimensionModel
        {
            Type = FractalDimensionType.HiguchiFractalDimension,
            X = logK,
            Y = logL,
            Dimension = LinearRegressionSlope(logK, logL)
        };
    }

    /// <summary>
    /// Метод Перма (структурная функция второго порядка)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateStructureFunctionDimension(this float[,] data)
    {
        int sizeX = data.GetLength(0);
        int sizeY = data.GetLength(1);
        int maxLag = Math.Min(sizeX, sizeY) / 4;

        List<float> logLags = new();
        List<float> logStructFunc = new();

        for (int lag = 1; lag < maxLag; lag *= 2)
        {
            float sum = 0;
            int count = 0;

            for (int i = 0; i < sizeX - lag; i++)
            {
                for (int j = 0; j < sizeY - lag; j++)
                {
                    float d = data[i, j] - data[i + lag, j + lag];
                    sum += d * d;
                    count++;
                }
            }

            if (count > 0)
            {
                float sf = sum / count;
                logLags.Add((float)Math.Log(lag));
                logStructFunc.Add((float)Math.Log(sf));
            }
        }

        float slope = LinearRegressionSlope(logLags, logStructFunc);
        return new FractalDimensionModel
        {
            Type = FractalDimensionType.StructureFunctionFractalDimension,
            X = logLags,
            Y = logStructFunc,
            Dimension = (float)(2 - slope / 2.0)
        };
    }

    /// <summary>
    /// Метод триангуляции
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static FractalDimensionModel? CalculateTriangulationDimension(this float[,] data)
    {
        int sizeX = data.GetLength(0);
        int sizeY = data.GetLength(1);
        int minSize = Math.Min(sizeX, sizeY);

        List<float> logEpsilons = new();
        List<float> logAreas = new();

        for (int step = 1; step <= minSize / 2; step *= 2)
        {
            float totalArea = 0;

            for (int i = 0; i < sizeX - step; i += step)
            {
                for (int j = 0; j < sizeY - step; j += step)
                {
                    // Четыре угла ячейки
                    var p00 = new Vector3(i, j, data[i, j]);
                    var p10 = new Vector3(i + step, j, data[i + step, j]);
                    var p01 = new Vector3(i, j + step, data[i, j + step]);
                    var p11 = new Vector3(i + step, j + step, data[i + step, j + step]);

                    // Два треугольника: (p00, p10, p11) и (p00, p11, p01)
                    float area1 = TriangleArea(p00, p10, p11);
                    float area2 = TriangleArea(p00, p11, p01);

                    totalArea += area1 + area2;
                }
            }

            logEpsilons.Add((float)Math.Log(1.0 / step));
            logAreas.Add((float)Math.Log(totalArea));
        }
        
        float slope = LinearRegressionSlope(logEpsilons, logAreas);
        return new FractalDimensionModel
        {
            Type = FractalDimensionType.TriangulationFractalDimension,
            X = logEpsilons,
            Y = logAreas,
            Dimension = 2 - slope
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