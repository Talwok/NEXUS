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
}