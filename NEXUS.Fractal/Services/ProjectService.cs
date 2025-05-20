using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using NEXUS.Parsers.BCR;
using NEXUS.Parsers.BCR.Helpers;
using NEXUS.Parsers.MDT;
using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Frames.Scanned;
using NEXUS.Parsers.MDT.Models.Frames.Spectroscopy;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp.PixelFormats;
using Path = System.IO.Path;

namespace NEXUS.Fractal.Services;

public class ProjectService : StatefulServiceBase
{
    private static readonly string ProjectExtension = ".nfproj";
    private static readonly string FileName = "ProjectsConfig.json";

    private string? _projectInitialPath;
    private readonly IStorageProvider _storageProvider;
    private readonly InfoService _infoService;

    private static readonly FilePickerOpenOptions ImportFilePickerOptions = new()
    {
        Title = "Импорт",
        AllowMultiple = true,
        FileTypeFilter =
        [
            new FilePickerFileType("NT-MDT") { Patterns = ["*.mdt"] },
            new FilePickerFileType("DigitalSurf") { Patterns = ["*.bcr"] },
            new FilePickerFileType("Изображения") { Patterns = ["*.jpeg", "*.jpg", "*.png", "*.bmp"] }
        ]
    };

    private static readonly FilePickerSaveOptions ExportFilePickerOptions = new()
    {
        Title = "Экспорт",
        FileTypeChoices =
        [
            new FilePickerFileType("NT-MDT") { Patterns = ["*.mdt"] },
            new FilePickerFileType("DigitalSurf") { Patterns = ["*.bcr"] },
            new FilePickerFileType("Изображения") { Patterns = ["*.jpeg", "*.jpg", "*.png", "*.bmp"] }
        ]
    };

    private static readonly FilePickerOpenOptions ProjectFilePickerOpenOptions = new()
    {
        Title = "Проект",
        AllowMultiple = false,
        FileTypeFilter = [new FilePickerFileType("Проект Fractal") { Patterns = ["*.nfproj"] }]
    };

    private static readonly FilePickerSaveOptions ProjectFilePickerSaveOptions = new()
    {
        Title = "Проект",
        FileTypeChoices = [new FilePickerFileType("Проект Fractal") { Patterns = ["*.nfproj"] }]
    };

    [JsonConstructor]
    public ProjectService() : base(FileName) { }
    
    [ActivatorUtilitiesConstructor]
    public ProjectService(IStorageProvider storageProvider, InfoService infoService) : base(FileName)
    {
        _storageProvider = storageProvider;
        _infoService = infoService;

        foreach (var newTable in PaletteParser.GetStandardPalleteFiles().SelectMany(file => file.Tables))
        {
            if(!string.Equals(newTable.Title, "Unnamed Palette", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(newTable.Title, "My Palette", StringComparison.OrdinalIgnoreCase))
                ColorTables.Add(newTable);
        }
        
        if(ColorTables.FirstOrDefault() is { } table)
            SelectedColorTable = table;

        this.WhenAnyValue(svc => svc.Project)
            .Select(prj => prj != null)
            .Subscribe(hasProj => HasProject = hasProj);
    }

    [Reactive] public ObservableCollection<RecentProjectModel> RecentProjects { get; set; } = [];

    [Reactive, JsonIgnore] public ProjectViewModel? Project { get; private set; }

    [Reactive, JsonIgnore] public bool HasProject { get; private set; }
    [Reactive, JsonIgnore] public object SelectedItem { get; set; }
    [Reactive, JsonIgnore] public PaletteColorTable? SelectedColorTable { get; set; }
    [JsonIgnore] public ObservableCollection<PaletteColorTable> ColorTables { get; } = [];

    private void UpdateRecentProjects(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
        {
            return;
        }

        if (RecentProjects.FirstOrDefault(proj => string.Equals(
                proj.FullPath, projectPath,
                StringComparison.OrdinalIgnoreCase)) is { } recentProject)
        {
            RecentProjects.Remove(recentProject);
        }

        recentProject = new RecentProjectModel
        {
            Name = Path.GetFileNameWithoutExtension(projectPath),
            Directory = Path.GetDirectoryName(projectPath),
            FullPath = projectPath
        };

        RecentProjects.Insert(0, recentProject);

        for (var i = 10; i < RecentProjects.Count; i++)
            RecentProjects.RemoveAt(i);
    }

    public async Task OpenRecentProject(string recentPath)
    {
        _projectInitialPath = recentPath;

        var fileStream = File.Open(_projectInitialPath, FileMode.OpenOrCreate);

        var model = await MessagePackSerializer.DeserializeAsync<ProjectModel>(fileStream);

        Project = new ProjectViewModel(_projectInitialPath, model);

        UpdateRecentProjects(_projectInitialPath);
    }

    public async Task CreateProject()
    {
        var file = await _storageProvider.SaveFilePickerAsync(ProjectFilePickerSaveOptions);

        if (file == null)
            return;

        _projectInitialPath = file.Path.LocalPath;

        Project = new ProjectViewModel(_projectInitialPath, new ProjectModel());

        await SaveProject();

        UpdateRecentProjects(_projectInitialPath);
    }

    public async Task OpenProject()
    {
        var files = await _storageProvider.OpenFilePickerAsync(ProjectFilePickerOpenOptions);

        if (files.Count == 0)
            return;

        var projectPath = files.First().Path.LocalPath;

        _projectInitialPath = projectPath;

        var fileStream = File.Open(_projectInitialPath, FileMode.OpenOrCreate);

        var model = await MessagePackSerializer.DeserializeAsync<ProjectModel>(fileStream);

        Project = new ProjectViewModel(_projectInitialPath, model);

        UpdateRecentProjects(_projectInitialPath);
    }

    public async Task SaveProject()
    {
        if (_projectInitialPath != null && Project != null)
        {
            var fileStream = File.Open(_projectInitialPath, FileMode.OpenOrCreate);

            var model = Project.GetModel();

            await MessagePackSerializer.SerializeAsync(fileStream, model);

            UpdateRecentProjects(_projectInitialPath);
        }
    }

    public async Task SaveProjectAs()
    {
        if (Project != null)
        {
            var file = await _storageProvider.SaveFilePickerAsync(ProjectFilePickerSaveOptions);

            if (file == null)
                return;

            var projectPath = file.Path.LocalPath;

            var fileStream = File.Open(projectPath, FileMode.OpenOrCreate);

            var model = Project.GetModel();

            await MessagePackSerializer.SerializeAsync(fileStream, model);

            UpdateRecentProjects(projectPath);
        }
    }

    public async Task ImportToProject()
    {
        var files = await _storageProvider.OpenFilePickerAsync(ImportFilePickerOptions);

        if (files.Count == 0)
            return;

        foreach (var storageFile in files)
        {
            var filePath = storageFile.Path.LocalPath;

            var extension = Path.GetExtension(filePath);

            switch (extension)
            {
                case ".jpg" or ".jpeg" or "png" or "bmp":
                    ProcessImageFile(filePath);
                    break;
                case ".mdt":
                    ProcessMdtFile(filePath);
                    break;
                case ".bcr":
                    ProcessBcrFile(filePath);
                    break;
            }
        }
    }

    private void ProcessImageFile(string filePath)
    {
        if(Project == null)
            return;
        
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath);
        
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

        var frameModel = new FrameModel
        {
            Id = Guid.NewGuid(),
            SourceType = FrameSourceType.Image,
            Name = Path.GetFileNameWithoutExtension(filePath),
            HeightMap = Normalize(heightMap),
            HeightSpacing = 10,
            HeightScaling = 1,
            MetaData = null // TODO: добавить заполнение необходимых для экспорта метаданных
        };
        
        Project.Frames.Add(new FrameViewModel(frameModel));
    }

    private void ProcessMdtFile(string filePath)
    {
        if(Project == null)
            return;
        
        var mdt = MdtParser.Parse(filePath);
        
        foreach (var frame in mdt.Frames)
        {
            FrameSourceType sourceType = frame.Type switch
            {
                FrameType.Mda => FrameSourceType.NtMdtMda,
                FrameType.Scanned => FrameSourceType.NtMdtScanned,
                FrameType.Spectroscopy => FrameSourceType.NtMdtSpectroscopy,
                _ => throw new IndexOutOfRangeException()
            };

            FrameModel frameModel;
            
            if (frame is MdaFrame mdaFrame)
            {
                var processor = mdaFrame.CreateFromMdaFrame();
                frameModel = new FrameModel
                {
                    Id = Guid.NewGuid(),
                    SourceType = sourceType,
                    Name = mdaFrame.Title,
                    HeightMap = Normalize(processor.GetHeightMap()),
                    HeightSpacing = 10,
                    HeightScaling = 1,
                    MetaData = null // TODO: добавить заполнение необходимых для экспорта метаданных
                };    
                Project.Frames.Add(new FrameViewModel(frameModel));
            }
            else if (frame is ScannedFrame scannedFrame) { }
            else if (frame is SpectroscopyFrame spectroscopyFrame) { }
        }
    }

    private void ProcessBcrFile(string filePath)
    {
        if(Project == null)
            return;
        
        var bcr = BcrParser.Parse(filePath);
        var processor = bcr.CreateFromBcrFrame();
        FrameModel frameModel = new FrameModel
        {
            Id = Guid.NewGuid(),
            SourceType = FrameSourceType.DigitalSurf,
            Name = Path.GetFileNameWithoutExtension(filePath),
            HeightMap = Normalize(processor.GetHeightMap()),
            HeightSpacing = 10,
            HeightScaling = 1,
            MetaData = bcr.Metadata
        };
        Project.Frames.Add(new FrameViewModel(frameModel));
    }

    public static float[,] Normalize(float[,] data)
    {
        if (data == null || data.Length == 0)
            return data;

        int rows = data.GetLength(0);
        int cols = data.GetLength(1);
    
        // Find min and max values in the array
        float min = data[0, 0];
        float max = data[0, 0];
    
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (data[i, j] < min) min = data[i, j];
                if (data[i, j] > max) max = data[i, j];
            }
        }
    
        // Handle case where all values are the same (avoid division by zero)
        if (min == max)
        {
            // You can choose to return all zeros, all 0.5, or the original array
            // Here we return all 0.5 since it's in the middle of [0,1]
            float[,] result = new float[rows, cols];
            for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = 0.5f;
            return result;
        }
    
        // Normalize the data
        float[,] normalized = new float[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                normalized[i, j] = (data[i, j] - min) / (max - min);
            }
        }
    
        return normalized;
    }
    
    public async Task ExportFromProject()
    {
        var file = await _storageProvider.SaveFilePickerAsync(ExportFilePickerOptions);

        if (file == null)
            return;

        var projectPath = file.Path.LocalPath;
    }
}