using NEXUS.Parsers.MDT.Models.Frames.Scanned;
using NEXUS.Parsers.MDT.Models.Pallete;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Parsers.MDT.Helpers;

public class ScannedFrameImageProcessor
{
    private readonly ScannedFrame _frame;
    private readonly MinMax _originalRange;
    private readonly Image<Rgba32> _image;
    private MinMax _currentRange;
    private Rgba32 _belowThresholdColor = new(0, 0, 0, 255);    // Черный для значений ниже минимума
    private Rgba32 _aboveThresholdColor = new(255, 255, 255, 255); // Белый для значений выше максимума

    public ScannedFrameImageProcessor(ScannedFrame frame)
    {
        _frame = frame;
        _originalRange = CalculateDataRange();
        _currentRange = _originalRange; // По умолчанию используем полный диапазон
        _image = CreateBaseImage();
    }
    
    public ScannedFrameImageProcessor WithRange(double min, double max)
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
        var buffer = _frame.ImageBuffer;
        var range = _currentRange.MaxValue - _currentRange.MinValue;
        
        colorImage.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    var value = buffer[y * accessor.Width + x];
                    
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
    
    private Image<Rgba32> CreateBaseImage()
    {
        int width = _frame.FrameXRes;
        int height = _frame.FrameYRes;
        return new Image<Rgba32>(width, height);
    }
    
    private MinMax CalculateDataRange()
    {
        var buffer = _frame.ImageBuffer;

        double min = 0;
        double max = 0;

        for (var i = 0; i < buffer.Length; i++)
        {
            if (i * sizeof(short) == buffer.Length)
                break;
            
            var value = buffer[i];
            
            if (i == 0) 
                min = max = value;
            
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new MinMax(min, max);
    }
    
    public float[,] GetHeightMap()
    {
        int width = _frame.FrameXRes;
        int height = _frame.FrameYRes;
    
        // Получаем диапазон для нормализации
        var range = _originalRange.MaxValue - _originalRange.MinValue;
        if (range == 0) range = 1; // защита от деления на ноль
    
        var map = new float[height, width]; // Обратите внимание на порядок height/width
    
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Правильный расчет индекса для двумерных данных
                int index = y * width + x;
            
                // Чтение и нормализация значения
                short value = _frame.ImageBuffer[index];
                float normalized = (float)((value - _originalRange.MinValue) / range);
            
                map[y, x] = normalized;
            }
        }
    
        return map;
    }
}