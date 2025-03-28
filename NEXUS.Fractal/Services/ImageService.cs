using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Fractal.Services;

public class ImageService : ServiceBase
{
    private static readonly Dictionary<MatrixType, Func<Image<Rgba32>, Image<Rgba32>>> FilterFunctions =
        new([
            new(MatrixType.Gaussian3x3_085, Filter.Gaussian3x3Filter_085),
            new(MatrixType.Gaussian3x3_0391, Filter.Gaussian3x3Filter_0391),
            new(MatrixType.Gaussian5x5_0625, Filter.Gaussian5x5Filter_0625),
            new(MatrixType.Gaussian5x5_10, Filter.Gaussian5x5Filter_10),
            new(MatrixType.Uniform3x3, Filter.Uniform3x3Filter),
            new(MatrixType.Uniform5x5, Filter.Uniform5x5Filter),
            new(MatrixType.Sharpen3x3, Filter.Sharpen3x3Filter),
            new(MatrixType.Laplacian3x3, Filter.Laplacian3x3Filter),
            new(MatrixType.Laplacian5x5, Filter.Laplacian5x5Filter),
            new(MatrixType.HighPass5x5Type1, Filter.Highpass5x5Filter1),
            new(MatrixType.HighPass5x5Type2, Filter.Highpass5x5Filter2),
            new(MatrixType.Sobel3x3, Filter.Sobel3x3Filter),
            new(MatrixType.Sobel3x3Horizontal, Filter.Sobel3x3HorizontalFilter),
            new(MatrixType.Sobel3x3Vertical, Filter.Sobel3x3VerticalFilter),
            new(MatrixType.Prewitt3x3, Filter.Prewitt3x3Filter),
            new(MatrixType.Prewitt3x3Horizontal, Filter.Prewitt3x3HorizontalFilter),
            new(MatrixType.Prewitt3x3Vertical, Filter.Prewitt3x3VerticalFilter),
            new(MatrixType.Kirsch3x3, Filter.Kirsch3x3Filter),
            new(MatrixType.Kirsch3x3Horizontal, Filter.Kirsch3x3HorizontalFilter),
            new(MatrixType.Kirsch3x3Vertical, Filter.Kirsch3x3VerticalFilter)
        ]);

    private static readonly string[] SearchPatterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp"];

    private static readonly string NameGroup = "name";
    private static readonly string GuidGroup = "guid";
    private static readonly string ExtensionGroup = "extension";

    private static readonly string RegexPattern =
        $@"^\.\$(?<{NameGroup}>.+)_(?<{GuidGroup}>[0-9a-fA-F\-]+)\.(?<{ExtensionGroup}>png|jpg|jpeg|bmp)$";

    private readonly SourceCache<ImageFileViewModel, string> _sourceCache;

    private readonly IStorageProvider _storageProvider;
    private readonly InfoService _infoService;

    private FileSystemWatcher? _watcher;

    public ImageService(IStorageProvider storageProvider, InfoService infoService)
    {
        _infoService = infoService;
        _storageProvider = storageProvider;

        _sourceCache = new SourceCache<ImageFileViewModel, string>(x => x.Path);
        _sourceCache.Connect()
            .ObserveOn(AvaloniaScheduler.Instance)
            .Bind(out var treeImages)
            .Subscribe();

        TreeImages = treeImages;
    }

    public ReadOnlyObservableCollection<ImageFileViewModel> TreeImages { get; }

    [Reactive] public string? Folder { get; private set; }

    [Reactive] public ObservableCollection<ImageFileViewModel> SelectedImages { get; set; } = [];

    public async Task InitFolder()
    {
        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Выберите папку для изображений",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;

        Folder = folders[0].Path.LocalPath;
        _watcher?.Dispose();
        _watcher = CreateWatcher(Folder);
    }

    private FileSystemWatcher CreateWatcher(string folderPath)
    {
        var watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        watcher.Created += (s, e) => ProcessFiles();
        watcher.Changed += (s, e) => ProcessFiles();
        watcher.Renamed += (s, e) => ProcessFiles();
        watcher.Deleted += (s, e) => ProcessFiles();

        ProcessFiles();

        return watcher;
    }

    private void ProcessFiles()
    {
        if (string.IsNullOrEmpty(Folder))
            return;

        var fileGroups = SearchPatterns
            .SelectMany(pattern => Directory.GetFiles(Folder, pattern))
            .Distinct()
            .Select(filePath =>
            {
                var fileName = Path.GetFileName(filePath);
                var match = Regex.Match(fileName, RegexPattern);
                if (match.Success)
                {
                    if (Guid.TryParse(match.Groups[GuidGroup].Value, out var guid))
                    {
                        return new ImageFileViewModel
                        {
                            Path = filePath,
                            Name = match.Groups[NameGroup].Value,
                            Extension = match.Groups[ExtensionGroup].Value,
                            Id = guid
                        };
                    }
                }

                return new ImageFileViewModel
                {
                    Path = filePath,
                    Name = Path.GetFileNameWithoutExtension(fileName),
                    Extension = Path.GetExtension(fileName)
                };
            })
            .GroupBy(file => file.Name);

        foreach (var root in TreeImages.ToList())
        {
            foreach (var rootChild in root.Children.ToList())
            {
                if (!File.Exists(rootChild.Path))
                {
                    root.Children.Remove(rootChild);
                }
            }
            
            if (!File.Exists(root.Path))
            {
                _sourceCache.Remove(root.Path);
            }
        }

        foreach (var fileGroup in fileGroups)
        {
            var root = fileGroup.FirstOrDefault(item => item.Id == null);
            if (root != null)
            {
                var cachedRoot = _sourceCache.Lookup(root.Path);
                if (cachedRoot.HasValue) 
                    root = cachedRoot.Value;
                
                var children = fileGroup
                    .Where(item => item.Id != null && !root.Children.Select(child => child.Path).Contains(item.Path));
                
                root.Children.AddRange(children);
                
                _sourceCache.AddOrUpdate(root);
            }
        }
    }

    public async Task AddImages()
    {
        if (string.IsNullOrEmpty(Folder))
            return;

        try
        {
            var imageFiles = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите изображения",
                AllowMultiple = true,
                FileTypeFilter =
                [
                    new FilePickerFileType("Изображение")
                    {
                        Patterns = SearchPatterns
                    }
                ]
            });

            if (imageFiles.Count == 0)
                return;

            foreach (var storageFile in imageFiles)
            {
                var filePath = storageFile.Path.LocalPath;
                File.Copy(filePath, Path.Combine(Folder, Path.GetFileName(filePath)));
            }

            _infoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Изображения",
                Message = $"Добавлено {imageFiles.Count} изображений",
                Icon = MaterialIconKind.Images,
                Severity = InfoBarSeverity.Informational
            });
        }
        catch (Exception ex)
        {
            _infoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Изображения",
                Message = $"Ошибка добавления изображений",
                Icon = MaterialIconKind.Images,
                Severity = InfoBarSeverity.Error
            });
            Console.WriteLine($"ImageService AddImages: {ex.Message}");
        }
    }

    public void RemoveImages(IEnumerable<ImageFileViewModel> images)
    {
        if (string.IsNullOrEmpty(Folder))
            return;

        try
        {
            var count = 0;
            foreach (var image in images)
            {
                foreach (var child in image.Children)
                {
                    File.Delete(child.Path);
                    count++;
                }

                File.Delete(image.Path);
                count++;
            }

            _infoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Изображения",
                Message = $"Удалено {count} изображений",
                Icon = MaterialIconKind.Images,
                Severity = InfoBarSeverity.Informational
            });
        }
        catch (Exception ex)
        {
            _infoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Изображения",
                Message = $"Ошибка при удалении изображений",
                Icon = MaterialIconKind.Images,
                Severity = InfoBarSeverity.Error
            });
            Console.WriteLine($"ImageService RemoveImages: {ex.Message}");
        }
    }

    public async Task ApplyFilter(MatrixType matrixType)
    {
        if (Folder == null)
            return;

        if (FilterFunctions.TryGetValue(matrixType, out var filter))
        {
            foreach (var imageVm in SelectedImages)
            {
                var image = await Image.LoadAsync<Rgba32>(imageVm.Path);
                var filteredImage = filter(image);

                // Формируем новое имя файла по шаблону .$Name_Guid.Extension
                var newFileName = $".${imageVm.Name}_{Guid.NewGuid()}.{imageVm.Extension?.Replace(".","")}";

                await filteredImage.SaveAsync(Path.Combine(Folder, newFileName));
            }
        }
    }
}