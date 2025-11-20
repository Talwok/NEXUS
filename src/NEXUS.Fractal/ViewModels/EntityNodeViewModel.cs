using System.Collections.ObjectModel;
using NEXUS.Converters;
using NEXUS.Fractal.Services;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
using NEXUS.Fractal.Models;
using NEXUS.Parsers.BCR;
using NEXUS.Parsers.BCR.Helpers;
using NEXUS.Parsers.MDT;
using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Frames.Scanned;
using NEXUS.Parsers.MDT.Models.Frames.Spectroscopy;
using NEXUS.Parsers.Ovito;
using NEXUS.Parsers.Ovito.Helpers;
using NEXUS.Parsers.Ovito.Models.XYZFile;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Fractal.ViewModels;

public class EntityNodeViewModel : ReactiveObject
{
    private readonly ColorTableService _svc;

    public EntityNodeViewModel()
    {
        if (!IsDirectory)
        {
            if (App.ServiceProvider
                    .GetServices<StatefulServiceBase>()
                    .FirstOrDefault<ColorTableService>() is { } svc)
            {
                _svc = svc;

                _svc.WhenAnyValue(s => s.SelectedColorTable)
                    .Subscribe(colorTable => ColorTable = colorTable);
            }

            this.WhenAnyValue(
                    vm => vm.ColorTableMinimum,
                    vm => vm.ColorTableMaximum,
                    vm => vm.ColorTableLowerSelection,
                    vm => vm.ColorTableUpperSelection,
                    vm => vm.ColorTable)
                .Subscribe(range =>
                {
                    var (min, max, lower, upper, table) = range;
                    ColorTableRange = new(max, min, upper, lower, table.Colors);
                    UpdateImage();
                });
        }
        RenameCommand = ReactiveCommand.Create(Rename);
        DeleteCommand = ReactiveCommand.Create(Delete);
        ExpandToggleCommand = ReactiveCommand.Create<bool>(ExpandToggle);
    }

    public ICommand RenameCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ExpandToggleCommand { get; }

    [Reactive]
    public PaletteColorTable? ColorTable { get; set; }

    [Reactive]
    public string Name { get; set; }

    [Reactive]
    public bool IsDirectory { get; set; }

    [Reactive]
    public ObservableCollection<EntityNodeViewModel> Children { get; set; } = [];

    [Reactive]
    public string FullPath { get; set; }

    [Reactive]
    public bool IsExpanded { get; set; }

    [Reactive]
    public string Extension { get; set; }

    [Reactive]
    public float ColorTableMaximum { get; set; }

    [Reactive]
    public float ColorTableMinimum { get; set; }

    [Reactive]
    public float ColorTableUpperSelection { get; set; }

    [Reactive]
    public float ColorTableLowerSelection { get; set; }

    [Reactive]
    public ColorTableRange ColorTableRange { get; set; }

    [Reactive]
    public float[,]? HeightMap { get; set; }

    [Reactive]
    public Bitmap? FrameImage { get; set; }

    [Reactive] public float AmbientStrength { get; set; } = 0.3f;
    [Reactive] public float SpecularStrength { get; set; } = 0.4f;
    [Reactive] public float HeightMultiplier { get; set; } = 1;
    [Reactive] public float LightPositionX { get; set; } = 0;
    [Reactive] public float LightPositionY { get; set; } = 0;
    [Reactive] public float LightPositionZ { get; set; } = 100;
    [Reactive] public float Zoom { get; set; } = 100;
    [Reactive] public float ModelYaw { get; set; }
    [Reactive] public float ModelPitch { get; set; } = 1.5f;
    [Reactive] public bool ShowFoundation { get; set; } = true;
    [Reactive] public bool IsChanged { get; set; } = true;

    public void Rename()
    {

    }

    public void Delete()
    {

    }

    public void ExpandToggle(bool expand)
    {
        IsExpanded = expand;
    }

    public async Task LoadData()
    {
        if (IsDirectory) return;

        switch (Extension)
        {
            case ".jpg" or ".jpeg" or ".png" or ".bmp":
                await ProcessImageFile();
                break;
            case ".mdt":
                ProcessMdtFile();
                break;
            case ".bcr":
                ProcessBcrFile();
                break;
            case ".xyz":
                await ProcessXyzFile();
                break;
            default:
                return;
        }
    }

    private async Task ProcessImageFile()
    {
        using var image = await Image.LoadAsync<Rgba32>(FullPath);

        int width = image.Width;
        int height = image.Height;

        float[,] heightMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = image[x, y];
                var grayValue = (pixel.R * 0.3f + pixel.G * 0.59f + pixel.B * 0.11f) / 255f;
                heightMap[x, y] = grayValue;
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            HeightMap = heightMap;
            UpdateImage();
        });
    }

    private void ProcessMdtFile()
    {
        var mdt = MdtParser.Parse(FullPath);

        foreach (var frame in mdt.Frames)
        {
            switch (frame)
            {
                case MdaFrame mdaFrame when mdaFrame.CreateFromMdaFrame() is { } mdaProcessor:
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        HeightMap = mdaProcessor.GetHeightMap().Normalize();
                    });
                    break;
                case ScannedFrame scannedFrame when
                    scannedFrame.CreateFromScannedFrame() is { } scannedProcessor:
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        HeightMap = scannedProcessor.GetHeightMap().Normalize();
                    });

                    break;
                case SpectroscopyFrame spectroscopyFrame:
                    break;
            }
        }

        Dispatcher.UIThread.Invoke(UpdateImage);

    }

    private void ProcessBcrFile()
    {
        var bcr = BcrParser.Parse(FullPath);
        var processor = bcr.CreateFromBcrFrame();

        Dispatcher.UIThread.Invoke(() =>
        {
            HeightMap = processor.GetHeightMap().Normalize();
            UpdateImage();
        });
    }

    private async Task ProcessXyzFile()
    {
        var frames = await XYZParser.Parse(FullPath);

        foreach (var processor in frames.Select(frame => frame.CreateFromXyzFrame()))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                HeightMap = processor.GetHeightMap().Normalize();
            });
        }
        await Dispatcher.UIThread.InvokeAsync(UpdateImage);
    }

    private void UpdateImage()
    {
        if (HeightMap != null)
        {
            if (ColorTableMaximum == 0 && ColorTableMinimum == 0)
            {
                var (min, max) = HeightMap.GetMinMax();
                ColorTableMaximum = ColorTableUpperSelection = max;
                ColorTableMinimum = ColorTableLowerSelection = min;
            }

            var image = new Image<Rgba32>(HeightMap.GetLength(1), HeightMap.GetLength(0));
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        var value = HeightMap[y, x];

                        if (ColorTable != null)
                        {
                            int colorIndex;
                            if (value > ColorTableUpperSelection)
                            {
                                colorIndex = ColorTable.Colors.Count - 1;
                            }
                            else if (value < ColorTableLowerSelection)
                            {
                                colorIndex = 0;
                            }
                            else
                            {
                                value = FrameHelper.Normalize(value, ColorTableLowerSelection, ColorTableUpperSelection);
                                colorIndex = (int)(value * (ColorTable.Colors.Count - 1));
                                colorIndex = Math.Clamp(colorIndex, 0, ColorTable.Colors.Count - 1);
                            }
                            row[x] = new Rgba32(ColorTable.Colors[colorIndex].Red, ColorTable.Colors[colorIndex].Green,
                                ColorTable.Colors[colorIndex].Blue);
                        }
                        else
                        {
                            row[x] = new Rgba32(value, value, value);
                        }
                    }
                }
            });
            var stream = new MemoryStream();
            image.SaveAsBmp(stream);
            stream.Seek(0, SeekOrigin.Begin);
            FrameImage = new Bitmap(stream);
            stream.Close();
        }
    }

    public void Clear()
    {
        HeightMap = null;
        FrameImage?.Dispose();
        FrameImage = null;
    }
}