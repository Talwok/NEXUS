using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Disposables.Fluent;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Microsoft.Win32;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads.Project;
using NEXUS.Fractal.Core.ViewModels.Project;
using NEXUS.Fractal.Project;
using NEXUS.Fractal.Project.Data;
using NEXUS.Fractal.Project.Entity;
using NEXUS.Helpers;
using NEXUS.Parsers;
using NEXUS.Parsers.Bcr.Helpers;
using NEXUS.Parsers.Mdt.Helpers;
using NEXUS.Parsers.Mdt.Models.Frames.MDA;
using NEXUS.Parsers.Mdt.Models.Frames.Scanned;
using NEXUS.Parsers.Mdt.Models.Frames.Spectroscopy;
using NEXUS.Parsers.Ovito.Helpers;
using Prism.Events;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Fractal.Core.Services.Project;

public partial class ProjectService : ObservableBaseObject
{
    private const string PngExtension = "png";
    private const string JpgExtension = "jpg";
    private const string JpegExtension = "jpeg";
    private const string BmpExtension = "bmp";

    private const string MdtExtension = "mdt";
    private const string BcrExtension = "bcr";
    private const string XyzExtension = "xyz";

    private const string ChartExtension = "nxc";

    private readonly Dictionary<string, ProjectEntityType> _entityTypes = new()
    {
        { PngExtension, ProjectEntityType.Heightmap },
        { JpgExtension, ProjectEntityType.Heightmap },
        { JpegExtension, ProjectEntityType.Heightmap },
        { BmpExtension, ProjectEntityType.Heightmap },

        { MdtExtension, ProjectEntityType.Heightmap },
        { BcrExtension, ProjectEntityType.Heightmap },
        { XyzExtension, ProjectEntityType.Heightmap },

        { ChartExtension, ProjectEntityType.Chart }
    };

    private const string ProjectFileExtension = ".nxf";

    private const string ProjectFileFilter = "NEXUS Fractal Project (*.nxf)|*.nxf";

    private const string ProjectEntityFilter = "All files (.png, .bmp, .jpg, .jpeg, .mdt, .bcr, .xyz)|*.png;*.bmp;*.jpg;*.jpeg;*.mdt;*.bcr;*.xyz|" +
                                               "Images (.png, .bmp, .jpg, .jpeg)|*.png;*.bmp;*.jpg;*.jpeg|" +
                                               "MDT (.mdt)|*.mdt|" +
                                               "BCR (.bcr)|*.bcr|" +
                                               "XYZ (.xyz)|*.xyz";

    private string? _projectPath;
    private ProjectInteractor? _projectInteractor;

    private readonly SourceCache<ProjectEntity, Guid> _entitiesSource;

    private readonly PubSubEvent<OnProjectOpenedEventPayload> _projectOpenedEvent;
    private readonly PubSubEvent<OnProjectChangedEventPayload> _projectChangedEvent;

    [ObservableProperty] private ObservableCollection<ProjectEntityViewModel> _selectedEntities;

    public ProjectService(IEventAggregator eventAggregator)
    {
        SelectedEntities = [];

        eventAggregator.GetEvent<PubSubEvent<CreateProjectEventPayload>>()
            .Subscribe(CreateProject)
            .DisposeWith(Disposable);

        eventAggregator.GetEvent<PubSubEvent<OpenProjectEventPayload>>()
            .Subscribe(OpenProject)
            .DisposeWith(Disposable);

        eventAggregator.GetEvent<PubSubEvent<SaveProjectEventPayload>>()
            .Subscribe(SaveProject)
            .DisposeWith(Disposable);

        eventAggregator.GetEvent<PubSubEvent<ImportProjectEntityEventPayload>>()
            .Subscribe(ImportEntity)
            .DisposeWith(Disposable);

        eventAggregator.GetEvent<PubSubEvent<RemoveProjectEntityEventPayload>>()
            .Subscribe(RemoveEntity)
            .DisposeWith(Disposable);

        eventAggregator.GetEvent<PubSubEvent<SelectProjectEntityEventPayload>>()
            .Subscribe(OnSelectProjectEntity)
            .DisposeWith(Disposable);

        _projectOpenedEvent = eventAggregator.GetEvent<PubSubEvent<OnProjectOpenedEventPayload>>();
        _projectChangedEvent = eventAggregator.GetEvent<PubSubEvent<OnProjectChangedEventPayload>>();

        _entitiesSource = new SourceCache<ProjectEntity, Guid>(ent => ent.Id)
            .DisposeWith(Disposable);

        _entitiesSource.Connect()
            .Transform(TransformProjectEntityToProjectEntityViewModel)
            .Bind(out var entities)
            .Subscribe()
            .DisposeWith(Disposable);

        Entities = entities;

        Disposable.Add(() =>
        {
            _projectInteractor?.Dispose();
            _projectInteractor = null;
            _entitiesSource.Clear();
        });
    }

    public ReadOnlyObservableCollection<ProjectEntityViewModel> Entities { get; }

    public ProjectHeightmapEntityData? GetHeightMapEntityData(Guid id) =>
        _projectInteractor?.GetEntityData<ProjectHeightmapEntityData>(id);

    private void CreateProject(CreateProjectEventPayload payload)
    {
        if (payload.FilePath is not null)
        {
            if (_projectInteractor is not null)
            {
                _projectInteractor.Dispose();
                _projectInteractor = null;
                _entitiesSource.Clear();
            }

            _projectInteractor = ProjectInteractor.OpenOrCreate(payload.FilePath);
            _projectOpenedEvent.Publish(new OnProjectOpenedEventPayload(payload.FilePath, DateTime.Now));
            _projectPath = payload.FilePath;
            return;
        }

        var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = ProjectFileExtension,
            Filter = ProjectFileFilter
        };

        if (dialog.ShowDialog() is true)
        {
            if (_projectInteractor is not null)
            {
                _projectInteractor.Dispose();
                _projectInteractor = null;
                _entitiesSource.Clear();
            }

            _projectInteractor = ProjectInteractor.OpenOrCreate(dialog.FileName);
            _projectOpenedEvent.Publish(new OnProjectOpenedEventPayload(dialog.FileName, DateTime.Now));
            _projectPath = dialog.FileName;
        }
    }

    private void SaveProject(SaveProjectEventPayload payload)
    {
        if (_projectPath is null)
            return;

        if (_projectInteractor is not null)
        {
            _projectInteractor.Dispose();
            _projectInteractor = null;
            _entitiesSource.Clear();
        }

        _projectInteractor = ProjectInteractor.OpenOrCreate(_projectPath);
        _entitiesSource.AddOrUpdate(_projectInteractor.GetEntities());
    }

    private void OpenProject(OpenProjectEventPayload payload)
    {
        if (payload.FilePath is not null)
        {
            if (_projectInteractor is not null)
            {
                _projectInteractor.Dispose();
                _projectInteractor = null;
                _entitiesSource.Clear();
            }

            _projectInteractor = ProjectInteractor.OpenOrCreate(payload.FilePath);
            _projectOpenedEvent.Publish(new OnProjectOpenedEventPayload(payload.FilePath, DateTime.Now));
            _projectPath = payload.FilePath;
            _entitiesSource.AddOrUpdate(_projectInteractor.GetEntities());
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = ProjectFileFilter
        };

        if (dialog.ShowDialog() is true)
        {
            if (_projectInteractor is not null)
            {
                _projectInteractor.Dispose();
                _projectInteractor = null;
                _entitiesSource.Clear();
            }

            _projectInteractor = ProjectInteractor.OpenOrCreate(dialog.FileName);
            _projectOpenedEvent.Publish(new OnProjectOpenedEventPayload(dialog.FileName, DateTime.Now));
            _projectPath = dialog.FileName;
            _entitiesSource.AddOrUpdate(_projectInteractor.GetEntities());
        }
    }

    private void RemoveEntity(RemoveProjectEntityEventPayload payload)
    {
        if (_projectInteractor is null)
            return;

        if (_projectInteractor.GetEntities() is { } entities)
        {
            var projectEntities = entities.Where(ent =>  !payload.Ids.Contains(ent.Id));
            _projectInteractor.SetEntities(projectEntities.ToList());
            foreach (var id in payload.Ids)
            {
                _projectInteractor.RemoveEntityData(id);
                _entitiesSource.Remove(id);
            }
        }
    }

    private void ImportEntity(ImportProjectEntityEventPayload payload)
    {
        if (_projectInteractor is null)
            return;

        var dialog = new OpenFileDialog
        {
            Filter = ProjectEntityFilter,
            Multiselect = true
        };

        if (dialog.ShowDialog() is true)
        {
            var filePaths = dialog.FileNames;

            if (_projectInteractor.GetEntities() is { } entities)
            {
                foreach (var filePath in filePaths)
                {
                    var extension = Path.GetExtension(filePath).Replace(".", "");

                    if (extension is PngExtension or JpgExtension or JpegExtension or BmpExtension)
                    {
                        var entity = CreateProjectEntity(Path.GetFileNameWithoutExtension(filePath));
                        entities.Add(entity);
                        _projectInteractor.SetEntities(entities);
                        _projectInteractor.SetEntityData(entity.Id,
                            ProjectHeightmapEntityData.FromHeightMap(ProcessImageFile(filePath)));
                    }
                    else if (extension is BcrExtension)
                    {
                        var entity = CreateProjectEntity(Path.GetFileNameWithoutExtension(filePath));
                        entities.Add(entity);
                        _projectInteractor.SetEntities(entities);
                        _projectInteractor.SetEntityData(entity.Id,
                            ProjectHeightmapEntityData.FromHeightMap(ProcessBcrFile(filePath)));
                    }
                    else if (extension is MdtExtension)
                    {
                        var mdt = MdtParser.Parse(filePath);

                        foreach (var frame in mdt.Frames)
                        {
                            if (frame is MdaFrame mdaFrame && mdaFrame.CreateFromMdaFrame() is { } mdaProcessor)
                            {
                                var entity = CreateProjectEntity(mdaFrame.Title);
                                entities.Add(entity);
                                _projectInteractor.SetEntities(entities);
                                _projectInteractor.SetEntityData(entity.Id,
                                    ProjectHeightmapEntityData.FromHeightMap(mdaProcessor.GetHeightMap()));
                            }
                            else if (frame is ScannedFrame scannedFrame && scannedFrame.CreateFromScannedFrame() is
                                         { } scannedProcessor)
                            {
                                var entity = CreateProjectEntity(scannedFrame.Title);
                                entities.Add(entity);
                                _projectInteractor.SetEntities(entities);
                                _projectInteractor.SetEntityData(entity.Id,
                                    ProjectHeightmapEntityData.FromHeightMap(scannedProcessor.GetHeightMap()));
                            }
                            else if (frame is SpectroscopyFrame spectroscopyFrame)
                            {
                            }
                        }
                    }
                    else if (extension is XyzExtension)
                    {
                        var frames = XyzParser.Parse(filePath);
                        foreach (var frame in frames)
                        {
                            var processor = frame.CreateFromXyzFrame();
                            var entity = CreateProjectEntity($"XYZ Frame {frame.FrameNumber}");
                            entities.Add(entity);
                            _projectInteractor.SetEntities(entities);
                            _projectInteractor.SetEntityData(entity.Id,
                                ProjectHeightmapEntityData.FromHeightMap(processor.GetHeightMap()));
                        }
                    }
                }
            }
        }
    }

    private ProjectEntityViewModel TransformProjectEntityToProjectEntityViewModel(ProjectEntity arg) => new(arg);

    private float[,] ProcessImageFile(string filePath)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath);

        var width = image.Width;
        var height = image.Height;

        var heightMap = new float[width, height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = image[x, y];
                var grayValue = (pixel.R * 0.3f + pixel.G * 0.59f + pixel.B * 0.11f) / 255f;
                heightMap[x, y] = grayValue;
            }
        }

        return heightMap;
    }

    private float[,] ProcessBcrFile(string filePath)
    {
        var bcr = BcrParser.Parse(filePath);
        var processor = bcr.CreateFromBcrFrame();
        return processor.GetHeightMap();
    }

    private ProjectEntity CreateProjectEntity(string entityName)
    {
        var entity = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Type = ProjectEntityType.Heightmap,
            Name = entityName,
            LastModified = DateTime.Now
        };
        _entitiesSource.AddOrUpdate(entity);
        return entity;
    }

    private void OnSelectProjectEntity(SelectProjectEntityEventPayload payload)
    {
        
    }
}