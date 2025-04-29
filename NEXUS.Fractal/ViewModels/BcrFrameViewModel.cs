using System;
using Avalonia.Media.Imaging;
using NEXUS.Parsers.BCR;
using NEXUS.Parsers.BCR.Helpers;
using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class BcrFrameViewModel
{
    private readonly BcrFrameImageProcessor _bcrFrameProcessor;

    public BcrFrameViewModel(BcrFile frame, PaletteColorTable table)
    {
        Frame = frame;

        Table = table;

        Title = "BcrFile";

        _bcrFrameProcessor = frame.CreateFromBcrFrame();
        
        if (_bcrFrameProcessor != null)
        {
            var range = _bcrFrameProcessor.GetOriginalRange();

            MinZLimit = MinZValue = range.MinValue;
            MaxZLimit = MaxZValue = range.MaxValue;
        }


        this.WhenAnyValue(
                vm => vm.MinZValue,
                vm => vm.MaxZValue)
            .Subscribe(props =>
            {
                var (minZValue, maxZValue) = props;

                if (_bcrFrameProcessor != null)
                    Image = _bcrFrameProcessor
                        .WithRange(minZValue, maxZValue)
                        .ApplyColorMap(table)
                        .ConvertToBitmap();
            });
    }

    public PaletteColorTable Table { get; }

    public BcrFile Frame { get; }

    public string Title { get; }

    public double MinZLimit { get; set; }
    public double MaxZLimit { get; set; }
    public double MinZValue { get; set; }
    public double MaxZValue { get; set; }

    [Reactive] public Bitmap Image { get; set; }
}