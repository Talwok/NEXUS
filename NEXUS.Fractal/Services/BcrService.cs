using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using DynamicData;
using NEXUS.Parsers.MDT;
using NEXUS.Parsers.MDT.Models.Pallete;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using NEXUS.Fractal.ViewModels;
using NEXUS.Parsers.BCR;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public class BcrService : ServiceBase
{
    private static readonly string[] BcrSearchPatterns = ["*.bcr"];
    private static readonly FilePickerFileType BcrFileType = new("Файлы BCR") { Patterns = BcrSearchPatterns };
    
    private readonly IStorageProvider _storageProvider;
    private readonly InfoService _infoService;
    private readonly SourceCache<PaletteColorTable, string> _palleteSource;

    public BcrService(IStorageProvider storageProvider, InfoService infoService)
    {
        _storageProvider = storageProvider;
        _infoService = infoService;
        
        _palleteSource = new SourceCache<PaletteColorTable, string>(pal => $"{pal.Parent.Path} {pal.Title}");
        
        foreach (var filePath in PaletteParser.GetStandardPalleteFiles()) 
            _palleteSource.AddOrUpdate(filePath.Tables);

        _palleteSource
            .Connect()
            .Bind(out var colorTables)
            .Subscribe();

        ColorTables = colorTables;
        
        SelectedColorTable = ColorTables.FirstOrDefault();
    }

    [Reactive]
    public PaletteColorTable? SelectedColorTable { get; set; }

    public ReadOnlyObservableCollection<PaletteColorTable> ColorTables { get; }
    
    
    [Reactive]
    public BcrFile Bcr { get; set; }

    [Reactive]
    public BcrFrameViewModel? Frame { get; set; }
    
    [Reactive] 
    public double ColorTableMaxLimit { get; set; }

    [Reactive] 
    public double ColorTableMinLimit { get; set; }

    [Reactive] 
    public double ColorTableMaxValue { get; set; }

    [Reactive] 
    public double ColorTableMinValue { get; set; }
    
    [Reactive] 
    public double RangeStart { get; set; } = 10;

    [Reactive] 
    public double RangeEnd { get; set; } = 90;

    public async Task OpenBcrAsync()
    {
        var imageFiles = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите файл микроскопии MDT",
            AllowMultiple = false,
            FileTypeFilter = [BcrFileType]
        });

        if (imageFiles.Count == 0)
            return;

        var index = 0;
        Bcr = BcrParser.Parse(imageFiles.First().Path.LocalPath);
        Frame = new BcrFrameViewModel(Bcr, SelectedColorTable);
    }
}