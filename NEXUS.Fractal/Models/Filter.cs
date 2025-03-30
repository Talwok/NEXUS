using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Fractal.Models;

public static class Filter
{
    private static Image<Rgba32> ConvolutionFilter(this Image<Rgba32> sourceImage, double[,] xfilterMatrix, double[,] yfilterMatrix, double factor = 1,
        int bias = 0) 
        => ApplyConvolution(ApplyConvolution(sourceImage, xfilterMatrix, factor, bias), yfilterMatrix, factor, bias);

    private static Image<Rgba32> ConvolutionFilter(this Image<Rgba32> sourceImage, double[,] filterMatrix, double factor = 1,
        int bias = 0) 
        => ApplyConvolution(sourceImage, filterMatrix, factor, bias);

    private static Image<Rgba32> ApplyConvolution(Image<Rgba32> sourceImage, double[,] filterMatrix, double factor,
        int bias)
    {
        int filterWidth = filterMatrix.GetLength(1);
        int filterHeight = filterMatrix.GetLength(0);
        int filterOffset = (filterWidth - 1) / 2;

        var resultImage = sourceImage.Clone();

        sourceImage.ProcessPixelRows(accessor =>
        {
            for (int y = filterOffset; y < accessor.Height - filterOffset; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = filterOffset; x < row.Length - filterOffset; x++)
                {
                    double blue = 0.0, green = 0.0, red = 0.0;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            var pixel = accessor.GetRowSpan(y + filterY)[x + filterX];
                            double filterValue = filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            blue += pixel.B * filterValue;
                            green += pixel.G * filterValue;
                            red += pixel.R * filterValue;
                        }
                    }

                    // Применяем фактор и смещение
                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;

                    // Ограничиваем значения в диапазоне [0, 255]
                    blue = Math.Clamp(blue, 0, 255);
                    green = Math.Clamp(green, 0, 255);
                    red = Math.Clamp(red, 0, 255);

                    // Записываем результат
                    resultImage[x, y] = new Rgba32((byte)red, (byte)green, (byte)blue);
                }
            }
        });

        return resultImage;
    }
    
    public static Image<Rgba32> Gaussian3x3Filter_085(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Gaussian3x3_085, 1.0 / 16.0);
    public static Image<Rgba32> Gaussian3x3Filter_0391(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Gaussian3x3_0391, 1.0 / 32.0);
    public static Image<Rgba32> Gaussian5x5Filter_0625(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Gaussian5x5_0625, 1.0 / 114.0);
    public static Image<Rgba32> Gaussian5x5Filter_10(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Gaussian5x5_10, 1.0 / 324.0);
    public static Image<Rgba32> Uniform3x3Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Uniform3x3, 1.0 / 8.0);
    public static Image<Rgba32> Uniform5x5Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Uniform5x5, 1.0 / 8.0);
    public static Image<Rgba32> Sharpen3x3Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Sharpen3x3, 0.5);
    public static Image<Rgba32> Laplacian3x3Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Laplacian3x3, 3, 255);
    public static Image<Rgba32> Laplacian5x5Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Laplacian5x5, 0.1);
    public static Image<Rgba32> Highpass5x5Filter1(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.HighPass5x5Type1, 0.1);
    public static Image<Rgba32> Highpass5x5Filter2(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.HighPass5x5Type2, 0.05);
    public static Image<Rgba32> Sobel3x3Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Sobel3x3Horizontal, Matrixes.Sobel3x3Vertical, 3, 255);
    public static Image<Rgba32> Sobel3x3HorizontalFilter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Sobel3x3Horizontal, 3, 255);
    public static Image<Rgba32> Sobel3x3VerticalFilter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Sobel3x3Vertical, 3, 255);
    public static Image<Rgba32> Prewitt3x3Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Prewitt3x3Horizontal, Matrixes.Prewitt3x3Vertical, 3, 255);
    public static Image<Rgba32> Prewitt3x3HorizontalFilter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Prewitt3x3Horizontal, 3, 255);
    public static Image<Rgba32> Prewitt3x3VerticalFilter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Prewitt3x3Vertical, 3, 255);
    public static Image<Rgba32> Kirsch3x3Filter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Kirsch3x3Horizontal, Matrixes.Kirsch3x3Vertical, 3, 255);
    public static Image<Rgba32> Kirsch3x3HorizontalFilter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Kirsch3x3Horizontal, 3, 255);
    public static Image<Rgba32> Kirsch3x3VerticalFilter(this Image<Rgba32> sourceBitmap) => sourceBitmap.ConvolutionFilter(Matrixes.Kirsch3x3Vertical, 3, 255);
}