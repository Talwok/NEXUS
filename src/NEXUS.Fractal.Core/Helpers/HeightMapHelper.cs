namespace NEXUS.Fractal.Core.Helpers;

public static class HeightMapHelper
{
    public static void NormalizeHeightmap(ref float[,] array)
    {
        if (array == null || array.Length == 0)
        {
            throw new ArgumentException("Array cannot be null or empty.");
        }

        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var totalElements = rows * cols;

        // First pass: calculate sum and sum of squares
        var sum = 0.0;
        var sumOfSquares = 0.0;

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                var value = array[i, j];
                sum += value;
                sumOfSquares += value * value;
            }
        }

        var mean = sum / totalElements;

        // Variance = (sumOfSquares / n) - mean^2
        var variance = (sumOfSquares / totalElements) - (mean * mean);
        var stdDev = Math.Sqrt(variance);

        // Handle case where stdDev is zero (all values are the same)
        if (stdDev < double.Epsilon)
        {
            // Set all to 0 (or choose another constant)
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    array[i, j] = 0f;
                }
            }
            return;
        }

        // Second pass: normalize each element
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                array[i, j] = (float)((array[i, j] - mean) / stdDev);
            }
        }
    }
}