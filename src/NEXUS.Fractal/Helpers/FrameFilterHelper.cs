using System;
using System.Linq;

namespace NEXUS.Fractal.Helpers;

public static class FrameFilterHelper
{
    // Гауссов фильтр
    public static float[,] ApplyGaussianFilter(this float[,] input, int kernelSize = 3, double sigma = 1.0)
    {
        if (kernelSize % 2 == 0)
            throw new ArgumentException("Kernel size must be odd.");

        int width = input.GetLength(0);
        int height = input.GetLength(1);
        float[,] output = new float[width, height];

        double[,] kernel = CreateGaussianKernel(kernelSize, sigma);
        int radius = kernelSize / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double sum = 0.0;
                double weightSum = 0.0;

                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        int nx = x + i;
                        int ny = y + j;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            double weight = kernel[i + radius, j + radius];
                            sum += input[nx, ny] * weight;
                            weightSum += weight;
                        }
                    }
                }

                output[x, y] = (float)(sum / weightSum);
            }
        }

        return output;
    }

    private static double[,] CreateGaussianKernel(int size, double sigma)
    {
        double[,] kernel = new double[size, size];
        int radius = size / 2;
        double sigmaSquared = 2 * sigma * sigma;
        double sum = 0.0;

        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                double distance = i * i + j * j;
                double value = Math.Exp(-distance / sigmaSquared);
                kernel[i + radius, j + radius] = value;
                sum += value;
            }
        }

        // Нормализация ядра
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                kernel[i, j] /= sum;

        return kernel;
    }

    // Билатеральный фильтр
    public static float[,] ApplyBilateralFilter(this float[,] input, int kernelSize = 3, double sigmaSpace = 1.0, double sigmaIntensity = 0.1)
    {
        if (kernelSize % 2 == 0)
            throw new ArgumentException("Kernel size must be odd.");

        int width = input.GetLength(0);
        int height = input.GetLength(1);
        float[,] output = new float[width, height];
        int radius = kernelSize / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double sum = 0.0;
                double weightSum = 0.0;
                float centerValue = input[x, y];

                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        int nx = x + i;
                        int ny = y + j;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            float neighborValue = input[nx, ny];

                            // Пространственный вес (как в Гауссовом фильтре)
                            double spaceWeight = Math.Exp(-(i * i + j * j) / (2 * sigmaSpace * sigmaSpace));

                            // Вес по интенсивности (разница значений)
                            double intensityWeight = Math.Exp(-Math.Pow(neighborValue - centerValue, 2) / (2 * sigmaIntensity * sigmaIntensity));

                            double totalWeight = spaceWeight * intensityWeight;
                            sum += neighborValue * totalWeight;
                            weightSum += totalWeight;
                        }
                    }
                }

                output[x, y] = (float)(sum / weightSum);
            }
        }

        return output;
    }

    // Медианная фильтрация
    public static float[,] ApplyMedianFilter(this float[,] input, int kernelSize = 3)
    {
        if (kernelSize % 2 == 0)
            throw new ArgumentException("Kernel size must be odd.");

        int width = input.GetLength(0);
        int height = input.GetLength(1);
        float[,] output = new float[width, height];
        int radius = kernelSize / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float[] values = new float[kernelSize * kernelSize];
                int index = 0;

                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        int nx = x + i;
                        int ny = y + j;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            values[index++] = input[nx, ny];
                        else
                            values[index++] = 0; // Можно использовать отражение границ
                    }
                }

                Array.Sort(values);
                output[x, y] = values[values.Length / 2]; // Медиана
            }
        }

        return output;
    }
}