using System.Buffers.Binary;
using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Pallete;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Parsers.MDT.Helpers;

public record MinMaxLimits(
    double MinValue,
    double MaxValue,
    double MinLimit,
    double MaxLimit);

public record MinMax(
    double MinValue,
    double MaxValue);

public static class FramesHelper
{
    public static MinMaxLimits UpdateMinMaxValues(MdaDataType dataType, byte[] imageData, int width, int height)
    {
        switch (dataType)
        {
            case MdaDataType.Int8:
            case MdaDataType.UInt8:
                return Calculate8BitMinMax(imageData, width, height);
            case MdaDataType.Int16:
            case MdaDataType.UInt16:
                return Calculate16BitMinMax(imageData, width, height);
            case MdaDataType.Int32:
            case MdaDataType.UInt32:
                return Calculate32BitMinMax(imageData, width, height);
            case MdaDataType.Float32:
                return CalculateFloat32MinMax(imageData, width, height);
            case MdaDataType.Float64:
                return CalculateFloat64MinMax(imageData, width, height);
        }

        return new MinMaxLimits(0, 0, 0, 0);
    }

    #region Min/Max Calculation Methods

    private static MinMaxLimits Calculate8BitMinMax(byte[] source, int width, int height)
    {
        byte min = byte.MaxValue;
        byte max = byte.MinValue;

        for (int i = 0; i < width * height; i++)
        {
            byte value = source[i];
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new MinMaxLimits(min, max, min, max);
    }

    private static MinMaxLimits Calculate16BitMinMax(byte[] source, int width, int height)
    {
        ushort min = ushort.MaxValue;
        ushort max = ushort.MinValue;

        for (int i = 0; i < width * height * 2; i += 2)
        {
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(i));
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new MinMaxLimits(min, max, min, max);
    }

    private static MinMaxLimits Calculate32BitMinMax(byte[] source, int width, int height)
    {
        uint min = uint.MaxValue;
        uint max = uint.MinValue;

        for (int i = 0; i < width * height * 4; i += 4)
        {
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(source.AsSpan(i));
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new MinMaxLimits(min, max, min, max);
    }

    private static MinMaxLimits CalculateFloat32MinMax(byte[] source, int width, int height)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int i = 0; i < width * height * 4; i += 4)
        {
            float value = BinaryPrimitives.ReadSingleLittleEndian(source.AsSpan(i));
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new MinMaxLimits(min, max, min, max);
    }

    private static MinMaxLimits CalculateFloat64MinMax(byte[] source, int width, int height)
    {
        double min = double.MaxValue;
        double max = double.MinValue;

        for (int i = 0; i < width * height * 8; i += 8)
        {
            double value = BinaryPrimitives.ReadDoubleLittleEndian(source.AsSpan(i));
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new MinMaxLimits(min, max, min, max);
    }

    #endregion

    private static Image<Rgba32> ConvertToImageSharp(MdaFrame mdaFrame)
    {
        if (mdaFrame.Dimensions.Count < 2 || mdaFrame.ImageBuffer.Length == 0)
            return null;

        // Получаем размерности
        int width = (int)(mdaFrame.Dimensions[0].MaxIndex - mdaFrame.Dimensions[0].MinIndex + 1);
        int height = (int)(mdaFrame.Dimensions[1].MaxIndex - mdaFrame.Dimensions[1].MinIndex + 1);

        // Создаем изображение
        var image = new Image<Rgba32>(width, height);

        // Обрабатываем данные
        ProcessImageData(mdaFrame.Mesurands[0].DataType, mdaFrame.ImageBuffer, image);

        return image;
    }

    private static void ProcessImageData(MdaDataType dataType, byte[] imageData, Image<Rgba32> image)
    {
        switch (dataType)
        {
            case MdaDataType.Int8:
            case MdaDataType.UInt8:
                Process8BitData(imageData, image);
                break;
            case MdaDataType.Int16:
            case MdaDataType.UInt16:
                Process16BitData(imageData, image);
                break;
            case MdaDataType.Int32:
            case MdaDataType.UInt32:
                Process32BitData(imageData, image);
                break;
            case MdaDataType.Float32:
                Process32SBitData(imageData, image);
                break;
            case MdaDataType.Float64:
                Process64SBitData(imageData, image);
                break;
            default:
                throw new NotSupportedException($"Тип данных {dataType} не поддерживается");
        }
    }

    #region Processing

    private static void Process8BitData(byte[] source, Image<Rgba32> image)
    {
        byte min = 0; //(byte)image.Metadata.GetMinValue();
        byte max = 255; //(byte)image.Metadata.GetMaxValue();
        double range = max - min;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                int srcOffset = y * accessor.Width;

                for (int x = 0; x < accessor.Width; x++)
                {
                    byte value = source[srcOffset + x];
                    byte normalized = range > 0 ? (byte)(((value - min) / range) * 255) : (byte)128;
                    row[x] = new Rgba32(normalized, normalized, normalized, 255);
                }
            }
        });
    }

    private static void Process16BitData(byte[] source, Image<Rgba32> image)
    {
        ushort min = 0; //(ushort)image.Metadata.GetMinValue();
        ushort max = 255; //(ushort)image.Metadata.GetMaxValue();
        double range = max - min;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                int srcOffset = y * accessor.Width * 2;

                for (int x = 0; x < accessor.Width; x++)
                {
                    ushort value = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(srcOffset + x * 2));
                    byte normalized = range > 0 ? (byte)(((value - min) / range) * 255) : (byte)128;
                    row[x] = new Rgba32(normalized, normalized, normalized, 255);
                }
            }
        });
    }

    private static void Process32BitData(byte[] source, Image<Rgba32> image)
    {
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                int srcOffset = y * accessor.Width * 2;

                for (int x = 0; x < accessor.Width; x++)
                {
                    uint value = BinaryPrimitives.ReadUInt32LittleEndian(source.AsSpan(srcOffset + x * 4));
                    byte byteValue = (byte)Math.Clamp(value * 255, 0, 255);
                    row[x] = new Rgba32(byteValue, byteValue, byteValue, 255);
                }
            }
        });
    }

    private static void Process64BitData(byte[] source, Image<Rgba32> image)
    {
        
    }
    
    private static void Process32SBitData(byte[] source, Image<Rgba32> image)
    {
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                int srcOffset = y * accessor.Width * 4;

                for (int x = 0; x < accessor.Width; x++)
                {
                    float value = BinaryPrimitives.ReadSingleLittleEndian(source.AsSpan(srcOffset + x * 4));
                    byte byteValue = (byte)Math.Clamp(value * 255, 0, 255);
                    row[x] = new Rgba32(byteValue, byteValue, byteValue, 255);
                }
            }
        });
    }
    
    private static void Process64SBitData(byte[] source, Image<Rgba32> image)
    {
        // Автоматическое определение диапазона значений
        (double min, double max) = FindMinMax(source, image.Width, image.Height);
        double range = max - min;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < image.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                int srcOffset = y * image.Width * 8; // 8 bytes per double

                for (int x = 0; x < image.Width; x++)
                {
                    var value = BinaryPrimitives.ReadDoubleLittleEndian(
                        source.AsSpan(srcOffset + x * 8));

                    // Нормализация и преобразование в byte
                    byte pixelValue = (byte)(((value - min) / range) * 255);
                    row[x] = new Rgba32(pixelValue, pixelValue, pixelValue, 255);
                }
            }
        });
    }

    private static MinMax FindMinMax(byte[] source, int width, int height)
    {
        double min = double.MaxValue;
        double max = double.MinValue;

        for (int i = 0; i < width * height * 8; i += 8)
        {
            double value = BinaryPrimitives.ReadDoubleLittleEndian(source.AsSpan(i));
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        // Защита от случая, когда все значения одинаковые
        if (Math.Abs(max - min) < double.Epsilon)
        {
            max = min + 1.0;
        }

        return new MinMax(min, max);
    }

    #endregion
    
    public static Image<Rgba32> ApplyColorMap(Image<Rgba32> image, Color[] colorMap)
    {
        var colorImage = image.Clone();
        colorImage.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < accessor.Width; x++)
                {
                    var intensity = row[x].R;
                    row[x] = colorMap[intensity];
                }
            }
        });
        return colorImage;
    }

    public static Color[] ToImageSharpColors(PaletteCollorTable table) 
        => table.Colors
            .Select(c => 
                new Color(new Rgba32(c.Red, c.Green, c.Blue)))
            .ToArray();
}