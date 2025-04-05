using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.ViewModels;
using ReactiveUI;
using System;
using System.IO;
using Avalonia.Media.Imaging;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Fractal.ViewModels;

public class MdaFrameViewModel : ViewModelBase
{
    private readonly FrameImageProcessor _frameProcessor;

    public MdaFrameViewModel(MdaFrame frame, PaletteCollorTable table)
    {
        Title = frame.Title;
        
        _frameProcessor = frame.CreateFromMdaFrame();

        var range = _frameProcessor.GetOriginalRange();

        MinZLimit = MinZValue = range.MinValue;
        MaxZLimit = MaxZValue = range.MaxValue;

        this.WhenAnyValue(
                vm => vm.MinZValue,
                vm => vm.MaxZValue)
            .Subscribe(props =>
            {
                var (minZValue, maxZValue) = props;
                
                Image = _frameProcessor
                    .WithRange(minZValue, maxZValue)
                    .ApplyColorMap(table)
                    .ConvertToBitmap();
            });
    }
    
    public string Title { get; }
    
    public double MinZLimit { get; set; }
    public double MaxZLimit { get; set; }
    public double MinZValue { get; set; }
    public double MaxZValue { get; set; }
    
    [Reactive]
    public Bitmap Image { get; set; }
}

public static class MdaFrameHelper
{
    public static Bitmap ConvertToBitmap(this Image<Rgba32> image)
    {
        var stream = new MemoryStream();
        image.SaveAsBmp(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var bitmap = new Bitmap(stream);
        stream.Close();
        return bitmap;
    }
}