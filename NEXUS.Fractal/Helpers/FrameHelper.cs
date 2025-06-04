using System;
using System.Collections.Generic;
using System.Linq;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Helpers;

public static class FrameHelper
{
    public static IEnumerable<FrameViewModel> BuildTree(this IEnumerable<FrameModel> frames)
    {
        var frameModels = frames.ToArray();
        var lookup = frameModels.ToDictionary(f => f.Id, f => new FrameViewModel(f));
        var rootNodes = new List<FrameViewModel>();
    
        foreach (var frame in frameModels)
        {
            if (frame.ParentId == null)
            {
                rootNodes.Add(lookup[frame.Id]);
            }
            else if (lookup.TryGetValue(frame.ParentId.Value, out var parentNode))
            {
                parentNode.Children.Add(lookup[frame.Id]);
            }
        }
    
        return rootNodes;
    }
    
    public static IEnumerable<FrameModel> FlattenTree(this IEnumerable<FrameViewModel> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node.GetModel();
        
            foreach (var child in FlattenTree(node.Children))
            {
                yield return child;
            }
        }
    }
    
    public static float[,] Normalize(this float[,] data)
    {
        if (data.Length == 0)
            return data;

        int rows = data.GetLength(0);
        int cols = data.GetLength(1);

        var (min, max) = GetMinMax(data);
    
        // Normalize the data
        float[,] normalized = new float[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                normalized[i, j] = (data[i, j] - min) / (max - min);
            }
        }
    
        return normalized;
    }
    public static float Denormalize(float value, float min, float max) 
        => value * (max - min) + min;

    public static float Normalize(float value, float min, float max) 
        => (value - min) / (max - min);
    
    public static (float min, float max) GetMinMax(this float[,] data)
    {
        int rows = data.GetLength(0);
        int cols = data.GetLength(1);
        
        // Find min and max values in the array
        float min = data[0, 0];
        float max = data[0, 0];
    
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (data[i, j] < min) min = data[i, j];
                if (data[i, j] > max) max = data[i, j];
            }
        }
        
        return (min, max);
    }
    
    public static float GetAverage(this float[,] array)
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Массив пуст или не инициализирован");

        float sum = 0f;
        int totalElements = array.GetLength(0) * array.GetLength(1);

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                sum += array[i, j];
            }
        }

        return sum / totalElements;
    }
    
    public static float[,] IncreaseContrast(this float[,] input, float power)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        float[,] output = new float[width, height];

        // Находим min и max в исходном массиве
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (input[i, j] < min) min = input[i, j];
                if (input[i, j] > max) max = input[i, j];
            }
        }

        // Нормализуем в [0, 1], затем применяем степенное преобразование, и денормализуем
        float range = max - min;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Нормализация
                float normalized = (input[i, j] - min) / range;
                // Увеличение контраста (степенное преобразование)
                float contrasted = MathF.Pow(normalized, power);
                // Денормализация
                output[i, j] = min + contrasted * range;
            }
        }

        return output;
    }
    
    public static float[,] HistogramEqualization(this float[,] input, int bins = 256)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        float[,] output = new float[width, height];

        // 1. Находим min и max для нормализации в [0, 1]
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (input[i, j] < min) min = input[i, j];
                if (input[i, j] > max) max = input[i, j];
            }
        }

        // 2. Нормализуем данные в [0, 1]
        float range = max - min;
        float[] normalizedData = new float[width * height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                normalizedData[i * height + j] = (input[i, j] - min) / range;
            }
        }

        // 3. Строим гистограмму
        int[] histogram = new int[bins];
        foreach (float value in normalizedData)
        {
            int bin = (int)(value * (bins - 1));
            histogram[bin]++;
        }

        // 4. Считаем CDF (кумулятивную гистограмму)
        int[] cdf = new int[bins];
        cdf[0] = histogram[0];
        for (int i = 1; i < bins; i++)
        {
            cdf[i] = cdf[i - 1] + histogram[i];
        }

        // 5. Нормализуем CDF в [0, 1]
        float cdfMin = cdf.Min();
        float cdfMax = cdf.Max();
        float[] cdfNormalized = new float[bins];
        for (int i = 0; i < bins; i++)
        {
            cdfNormalized[i] = (cdf[i] - cdfMin) / (cdfMax - cdfMin);
        }

        // 6. Применяем преобразование к данным
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float value = normalizedData[i * height + j];
                int bin = (int)(value * (bins - 1));
                output[i, j] = cdfNormalized[bin]; // Новое значение
            }
        }

        return output;
    }
}