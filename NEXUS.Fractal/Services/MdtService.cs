using System;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DynamicData;
using FluentAvalonia.UI.Controls;
using NEXUS.Extensions;
using NEXUS.Fractal.ViewModels;
using NEXUS.Parsers.MDT;
using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models;
using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace NEXUS.Fractal.Services;

public class MdtService : ServiceBase
{
    private readonly SourceCache<PaletteCollorTable, string> _palleteSource;
    private readonly IStorageProvider _storageProvider;

    public MdtService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
        _palleteSource = new SourceCache<PaletteCollorTable, string>(pal => $"{pal.Parent.Path} {pal.Title}");
        
        foreach (var filePath in PaletteParser.GetStandardPalleteFiles()) 
            _palleteSource.AddOrUpdate(filePath.Tables);

        _palleteSource
            .Connect()
            .Bind(out var colorTables)
            .Subscribe();

        ColorTables = colorTables;
        
        SelectedColorTable = ColorTables.FirstOrDefault();

        this.WhenAnyValue(vm => vm.SelectedColorTable)
            .Subscribe(UpdateImageColor);

        Frames = [];

        SelectedFrames.CollectionChanged += SelectedFramesOnCollectionChanged;
    }

    private void SelectedFramesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(LoadMdaFrames);
    }

    [Reactive]
    public ObservableCollection<MdaFrameViewModel> Frames { get; set; }
    
    private void UpdateImageColor(PaletteCollorTable? palleteFile)
    {
        if (palleteFile != null && palleteFile.Colors.Count > 0)
        {
            var startPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
            var endPoint = new RelativePoint(1, 0, RelativeUnit.Relative);
            var foregroundBrush = new LinearGradientBrush
            {
                StartPoint = startPoint,
                EndPoint = endPoint,
            };
            var backgroundBrush = new LinearGradientBrush
            {
                StartPoint = startPoint,
                EndPoint = endPoint,
            };
            
            // Calculate offset step if you want even distribution
            double step = 1.0 / (palleteFile.Colors.Count - 1);
    
            for (int i = 0; i < palleteFile.Colors.Count; i++)
            {
                var color = palleteFile.Colors[i];
                foregroundBrush.GradientStops.Add(new GradientStop
                {
                    Color = new Avalonia.Media.Color(255, color.Red, color.Green, color.Blue),
                    Offset = i * step
                });
            }
    
            ForegroundBrush = foregroundBrush;

            LoadMdaFrames();
        }
    }

    public ReadOnlyObservableCollection<PaletteCollorTable> ColorTables { get; }

    [Reactive]
    public ObservableCollection<MdtFrame> SelectedFrames { get; set; } = [];
    
    [Reactive]
    public LinearGradientBrush ForegroundBrush { get; set; } = new();

    [Reactive]
    public LinearGradientBrush BackgroundBrush { get; set; } = new();

    [Reactive] 
    public double RangeStart { get; set; } = 10;

    [Reactive] 
    public double RangeEnd { get; set; } = 90;
    
    [Reactive] 
    public PaletteCollorTable? SelectedColorTable { get; set; }

    [Reactive] 
    public double ColorTableMaxLimit { get; set; }

    [Reactive] 
    public double ColorTableMinLimit { get; set; }

    [Reactive] 
    public double ColorTableMaxValue { get; set; }

    [Reactive] 
    public double ColorTableMinValue { get; set; }
    
    [Reactive]
    public MdtFile Mdt { get; set; }

    public async Task OpenMdtAsync()
    {
        var imageFiles = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите файл микроскопии MDT",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("MDT (NT-MDT)")
                {
                    Patterns = ["*.mdt"]
                }
            ]
        });

        if (imageFiles.Count == 0)
            return;

        var index = 0;
        Mdt = MdtParser.Parse(imageFiles.First().Path.LocalPath);

        // foreach (var frame in Mdt.Frames)
        // {
        //     if (frame is MdaFrame mdaFrame)
        //     {
        //         var proc = new FrameImageProcessor(mdaFrame);
        //         var map = proc.GetHeightMap();
        //         var stream = new FileStream($"MdaFrame_{++index}", FileMode.Create);
        //         await JsonSerializer.SerializeAsync(stream, ConvertToJaggedArray(map));
        //         stream.Close();
        //     }
        // }
        
    }
    
    public static double[,] ConvertFromJaggedArray(double[][] jaggedArray)
    {
        if (jaggedArray == null || jaggedArray.Length == 0)
            return new double[0, 0];

        // Get dimensions
        int rows = jaggedArray.Length;
        int cols = jaggedArray[0].Length;

        // Validate that all rows have same number of columns
        for (int i = 1; i < rows; i++)
        {
            if (jaggedArray[i].Length != cols)
            {
                throw new ArgumentException("Jagged array is not rectangular");
            }
        }

        // Create and populate 2D array
        double[,] array2D = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array2D[i, j] = jaggedArray[i][j];
            }
        }

        return array2D;
    }
    
    double[][] ConvertToJaggedArray(double[,] array)
    {
        double[][] jagged = new double[array.GetLength(0)][];
        for (int i = 0; i < array.GetLength(0); i++)
        {
            jagged[i] = new double[array.GetLength(1)];
            for (int j = 0; j < array.GetLength(1); j++)
            {
                jagged[i][j] = array[i, j];
            }
        }
        return jagged;
    }

    private void LoadMdaFrames()
    {
        if (SelectedFrames.Count > 0 && SelectedColorTable != null)
        {
            var frameVMs = SelectedFrames
                .OfType<MdaFrame>()
                .Select(mda => new MdaFrameViewModel(mda, SelectedColorTable));

            Frames = new ObservableCollection<MdaFrameViewModel>(frameVMs);
        }
    }
}