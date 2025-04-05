using System.Buffers.Binary;
using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Pallete;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Parsers.MDT.Helpers;

public static class FramesPipeline
{
    public static FrameImageProcessor CreateFromMdaFrame(this MdaFrame frame)
    {
        if (frame.Dimensions.Count < 2 || frame.ImageBuffer.Length == 0)
            return null;

        return new FrameImageProcessor(frame);
    }
}

public class FrameImageProcessor
{
   private readonly MdaFrame _frame;
    private MinMax _originalRange;
    private MinMax _currentRange;
    private Image<Rgba32> _image;
    private Rgba32 _belowThresholdColor = new(0, 0, 0, 255);    // Черный для значений ниже минимума
    private Rgba32 _aboveThresholdColor = new(255, 255, 255, 255); // Белый для значений выше максимума

    public FrameImageProcessor(MdaFrame frame)
    {
        _frame = frame;
        _originalRange = CalculateDataRange();
        _currentRange = _originalRange; // По умолчанию используем полный диапазон
        _image = CreateBaseImage();
    }
    
    public FrameImageProcessor WithRange(double min, double max)
    {
        _currentRange = new MinMax(
            Math.Max(min, _originalRange.MinValue),
            Math.Min(max, _originalRange.MaxValue));
        return this;
    }
    
    public Image<Rgba32> ApplyColorMap(PaletteCollorTable colorTable)
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
        var dataType = _frame.Mesurands[0].DataType;
        var range = _currentRange.MaxValue - _currentRange.MinValue;
        
        colorImage.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    double value = ReadValue(buffer, dataType, (y * accessor.Width + x));
                    
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
        int width = (int)(_frame.Dimensions[0].MaxIndex - _frame.Dimensions[0].MinIndex + 1);
        int height = (int)(_frame.Dimensions[1].MaxIndex - _frame.Dimensions[1].MinIndex + 1);
        return new Image<Rgba32>(width, height);
    }
    
    private MinMax CalculateDataRange()
    {
        var buffer = _frame.ImageBuffer;
        var dataType = _frame.Mesurands[0].DataType;

        double min = 0;
        double max = 0;

        for (var i = 0; i < buffer.Length; i++)
        {
            if (i * GetTypeSize(dataType) == buffer.Length)
                break;
            
            var value = ReadValue(buffer, dataType, i);
            
            if (i == 0) 
                min = max = value;
            
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new MinMax(min, max);
    }

    private double ReadValue(byte[] buffer, MdaDataType dataType, int index)
    {
        var offset = index * GetTypeSize(dataType);
        return dataType switch
        {
            MdaDataType.Int8 => (sbyte)buffer[offset],
            MdaDataType.UInt8 => buffer[offset],
            MdaDataType.Int16 => BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(offset)),
            MdaDataType.UInt16 => BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset)),
            MdaDataType.Int32 => BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(offset)),
            MdaDataType.UInt32 => BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset)),
            MdaDataType.Int64 => BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(offset)),
            MdaDataType.UInt64 => BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan(offset)), 
            MdaDataType.Float32 => BinaryPrimitives.ReadSingleLittleEndian(buffer.AsSpan(offset)),
            MdaDataType.Float64 => BinaryPrimitives.ReadDoubleLittleEndian(buffer.AsSpan(offset)),
            _ => throw new NotSupportedException($"Data type {dataType} is not supported")
        };
    }

    private int GetTypeSize(MdaDataType dataType)
    {
        return dataType switch
        {
            MdaDataType.Int8 or MdaDataType.UInt8 => 1,
            MdaDataType.Int16 or MdaDataType.UInt16 => 2,
            MdaDataType.Int32 or MdaDataType.UInt32 or MdaDataType.Float32 => 4,
            MdaDataType.Float64 => 8,
            _ => throw new NotSupportedException($"Data type {dataType} is not supported")
        };
    }
}