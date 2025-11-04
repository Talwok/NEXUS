using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
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
using NEXUS.Parsers.Ovito;
using NEXUS.Parsers.Ovito.Helpers;
using NEXUS.Parsers.Ovito.Models.XYZFile;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp.PixelFormats;
using Path = System.IO.Path;

namespace NEXUS.Fractal.Services;

public class ProjectService : StatefulServiceBase
{
    public static readonly int FramesTab = 0;
    public static readonly int ResearchesTab = 1;

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
            new FilePickerFileType("Все типы") {Patterns = ["*.mdt", "*.bcr", "*.jpeg", "*.jpg", "*.png", "*.bmp", "*.xyz"]},
            new FilePickerFileType("NT-MDT") { Patterns = ["*.mdt"] },
            new FilePickerFileType("DigitalSurf") { Patterns = ["*.bcr"] },
            new FilePickerFileType("XYZ Ovito") { Patterns = ["*.xyz"]},
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
            if (!string.Equals(newTable.Title, "Unnamed Palette", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(newTable.Title, "My Palette", StringComparison.OrdinalIgnoreCase))
                ColorTables.Add(newTable);
        }

        if (ColorTables.FirstOrDefault() is { } table)
            SelectedColorTable = table;

        this.WhenAnyValue(svc => svc.Project)
            .Select(prj => prj != null)
            .Subscribe(hasProj => HasProject = hasProj);
    }

    [Reactive] public ObservableCollection<RecentProjectModel> RecentProjects { get; set; } = [];
    [Reactive, JsonIgnore] public ProjectViewModel? Project { get; private set; }
    [Reactive, JsonIgnore] public bool HasProject { get; private set; }
    [Reactive, JsonIgnore] public int SelectedTab { get; set; }
    [Reactive, JsonIgnore] public FrameViewModel SelectedFrame { get; set; }
    [Reactive, JsonIgnore] public ObservableCollection<FrameViewModel> SelectedFrames { get; set; } = [];
    [Reactive, JsonIgnore] public ResearchViewModel SelectedResearch { get; set; }
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

        fileStream.Close();

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

        fileStream.Close();

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

            fileStream.Close();

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

            fileStream.Close();

            UpdateRecentProjects(projectPath);
        }
    }

    public async Task ImportToProject()
    {
        var files = await _storageProvider.OpenFilePickerAsync(ImportFilePickerOptions);

        if (files.Count == 0)
            return;

        /*_infoService.AppendMessage(new InfoMessageViewModel
        {
            Severity = InfoBarSeverity.Informational,
            Title = "Импорт файлов",
            Message = $"Идёт импортирование файлов, всего: {files.Count}"
        });*/

        foreach (var storageFile in files)
        {
            var filePath = storageFile.Path.LocalPath;

            var extension = Path.GetExtension(filePath);

            switch (extension)
            {
                case ".jpg" or ".jpeg" or ".png" or ".bmp":
                    ProcessImageFile(filePath);
                    break;
                case ".mdt":
                    ProcessMdtFile(filePath);
                    break;
                case ".bcr":
                    ProcessBcrFile(filePath);
                    break;
                case ".xyz":
                    await ProcessXyzFile(filePath);
                    break;
            }
        }
    }

    private void ProcessImageFile(string filePath)
    {
        if (Project == null)
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
            HeightMap = heightMap.Normalize(),
            HeightSpacing = 10,
            HeightScaling = 1,
            MetaData = null // TODO: добавить заполнение необходимых для экспорта метаданных
        };

        Project.Frames.Add(new FrameViewModel(frameModel));
    }

    private void ProcessMdtFile(string filePath)
    {
        if (Project == null)
            return;

        var mdt = MdtParser.Parse(filePath);

        /*_infoService.AppendMessage(new InfoMessageViewModel
        {
            Severity = InfoBarSeverity.Informational,
            Message = $"Идёт импортирование MDT файла, всего фреймов: {mdt.Frames.Count}"
        });*/

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

            if (frame is MdaFrame mdaFrame && mdaFrame.CreateFromMdaFrame() is { } mdaProcessor)
            {
                frameModel = new FrameModel
                {
                    Id = Guid.NewGuid(),
                    SourceType = sourceType,
                    Name = mdaFrame.Title,
                    HeightMap = mdaProcessor.GetHeightMap().Normalize(),
                    MetaData = null // TODO: добавить заполнение необходимых для экспорта метаданных
                };
                Project.Frames.Add(new FrameViewModel(frameModel));
            }
            else if (frame is ScannedFrame scannedFrame && scannedFrame.CreateFromScannedFrame() is { } scannedProcessor)
            {
                frameModel = new FrameModel
                {
                    Id = Guid.NewGuid(),
                    SourceType = sourceType,
                    Name = scannedFrame.Title,
                    HeightMap = scannedProcessor.GetHeightMap().Normalize(),
                    MetaData = null // TODO: добавить заполнение необходимых для экспорта метаданных
                };
                Project.Frames.Add(new FrameViewModel(frameModel));
            }
            else if (frame is SpectroscopyFrame spectroscopyFrame) { }
        }
    }

    private void ProcessBcrFile(string filePath)
    {
        if (Project == null)
            return;

        var bcr = BcrParser.Parse(filePath);
        var processor = bcr.CreateFromBcrFrame();
        FrameModel frameModel = new FrameModel
        {
            Id = Guid.NewGuid(),
            SourceType = FrameSourceType.DigitalSurf,
            Name = Path.GetFileNameWithoutExtension(filePath),
            HeightMap = processor.GetHeightMap().Normalize(),
            HeightSpacing = 10,
            HeightScaling = 1,
            MetaData = bcr.Metadata
        };
        Project.Frames.Add(new FrameViewModel(frameModel));
    }

    private async Task ProcessXyzFile(string filePath)
    {
        if (Project == null)
            return;

        var frames = (await XYZParser.Parse(filePath));

        /*_infoService.AppendMessage(new InfoMessageViewModel
        {
            Severity = InfoBarSeverity.Informational,
            Title = "Импорт файлов",
            Message = $"Импортирование XYZ файла, всего фреймов: {frames.Count}"
        });*/

        foreach (var frame in frames)
        {
            var processor = frame.CreateFromXyzFrame();

            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata[nameof(XYZFrame.FrameNumber)] = frame.FrameNumber.ToString();
            metadata[nameof(XYZFrame.PropertyNames)] = string.Join(", ", frame.PropertyNames);
            metadata[nameof(XYZFrame.Comment)] = frame.Comment;

            var frameModel = new FrameModel
            {
                Id = Guid.NewGuid(),
                SourceType = FrameSourceType.OvitoXyz,
                Name = Path.GetFileNameWithoutExtension(filePath),
                HeightMap = processor.GetHeightMap().Normalize(),
                HeightSpacing = 10,
                HeightScaling = 1,
                MetaData = metadata
            };
            Project.Frames.Add(new FrameViewModel(frameModel));
        }
    }

    public async Task ExportFromProject()
    {
        var file = await _storageProvider.SaveFilePickerAsync(ExportFilePickerOptions);

        if (file == null)
            return;

        var projectPath = file.Path.LocalPath;
    }

    public void RemoveCurrentFrame()
    {
        if (SelectedFrame is { } frame)
        {
            if (frame.ParentId is { } parentId && GetFrame(Project?.Frames, parentId) is { } parentFrame)
            {
                parentFrame.Children.Remove(frame);
            }
            else
            {
                Project?.Frames.Remove(frame);
            }
        }
    }

    public void CloneCurrentFrame()
    {
        void AppendFrame(FrameViewModel? frame, FrameViewModel? parentFrame = null)
        {
            if (frame == null)
                return;

            var frameModel = frame.GetModel();
            frameModel.Id = Guid.NewGuid();

            var frameViewModel = new FrameViewModel(frameModel);

            if (parentFrame != null)
            {
                parentFrame.Children.Add(frameViewModel);
            }
            else if (frameModel.ParentId is { } parentId)
            {
                parentFrame = GetFrame(Project?.Frames, parentId);
                parentFrame?.Children.Add(frameViewModel);
            }
            else
            {
                Project?.Frames.Add(frameViewModel);
            }

            var children = frame.Children.ToList();

            foreach (var child in children)
            {
                AppendFrame(child, frameViewModel);
            }
        }

        AppendFrame(SelectedFrame);
    }

    private FrameViewModel? GetFrame(IEnumerable<FrameViewModel>? frames, Guid id)
    {
        if (frames == null)
            return null;

        foreach (var frame in frames)
        {
            if (frame.Id == id)
                return frame;

            if (GetFrame(frame.Children, id) is { } childFrame)
                return childFrame;
        }

        return null;
    }
}