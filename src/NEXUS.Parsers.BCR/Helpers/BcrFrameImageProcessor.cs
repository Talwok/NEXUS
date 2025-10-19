using System.Buffers.Binary;
using NEXUS.Parsers.MDT.Models.Pallete;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Parsers.BCR.Helpers;

public record MinMaxLimits(
    double MinValue,
    double MaxValue,
    double MinLimit,
    double MaxLimit);

public record MinMax(
    double MinValue,
    double MaxValue);

public static class BcrFramesPipeline
{
    public static BcrFrameImageProcessor CreateFromBcrFrame(this BcrFile file)
        => new BcrFrameImageProcessor(file);
}

public class BcrFrameImageProcessor
{
    private readonly BcrFile _frame;
    private MinMax _originalRange;
    private MinMax _currentRange;
    private Image<Rgba32> _image;
    private Rgba32 _belowThresholdColor = new(0, 0, 0, 255);    // Черный для значений ниже минимума
    private Rgba32 _aboveThresholdColor = new(255, 255, 255, 255); // Белый для значений выше максимума

    public BcrFrameImageProcessor(BcrFile frame)
    {
        _frame = frame;
        _originalRange = CalculateDataRange();
        _currentRange = _originalRange; // По умолчанию используем полный диапазон
        _image = CreateBaseImage();
    }

    public BcrFrameImageProcessor WithRange(double min, double max)
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
                    double value = _frame.Data[y, x];

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
        return new Image<Rgba32>(_frame.XPixels, _frame.YPixels);
    }

    private MinMax CalculateDataRange()
    {
        double min = 0;
        double max = 0;

        for (int i = 0; i < _frame.XPixels; i++)
        {
            for (int j = 0; j < _frame.YPixels; j++)
            {
                var value = _frame.Data[i, j];

                if (i == 0)
                    min = max = value;

                min = Math.Min(min, value);
                max = Math.Max(max, value);

            }
        }

        return new MinMax(min, max);
    }


    public float[,] GetHeightMap() => ConvertToFloatHeightMap(_frame.Data);

    private float[,] ConvertToFloatHeightMap(double[,] heightMap)
    {
        var floatHeightMap = new float[
            heightMap.GetLength(0),
            heightMap.GetLength(1)];

        for (int i = 0; i < heightMap.GetLength(0); i++)
            for (int j = 0; j < heightMap.GetLength(1); j++)
                floatHeightMap[i, j] = (float)heightMap[i, j];

        return floatHeightMap;
    }
}