using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using NEXUS.Extensions;
using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Pallete;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NEXUS.Fractal.Converters;

/*public class FrameImageBufferToBitmapConverter : IMultiValueConverter
{
    private double _colorTableMaxLimit;
    private double _colorTableMinLimit;
    private double _colorTableMaxValue;
    private double _colorTableMinValue;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is { Count: > 0 })
        {
            var frame = values.FirstOrDefault<MdaFrame>();
            var colorTable = values.FirstOrDefault<PaletteCollorTable>();

            if (frame == null || colorTable == null)
                return default(Bitmap);

            var image = ConvertToImageSharp(frame, frame.ImageBuffer);
            image.Mutate(img => { img.Flip(FlipMode.Vertical); });

            // Обновляем минимальные/максимальные значения
            UpdateMinMaxValues(frame.Mesurands[0].DataType, frame.ImageBuffer, image.Width, image.Height);

            var coloredImage = ApplyColorMap(image, ToImageSharpColors(colorTable));

            var memoryStream = new MemoryStream();
            coloredImage.SaveAsBmp(memoryStream);
            memoryStream.Position = 0;

            return new Bitmap(memoryStream);
        }

        return default(Bitmap);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    
    public static FrameImageBufferToBitmapConverter Instance = new();
}*/