using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using DynamicData;

namespace NEXUS.Fractal.ViewModels;

public class FileTreeViewModel : ViewModelBase
{
    private readonly FileWatcherService _service;
    private readonly IStorageProvider _storageProvider;

    public FileTreeViewModel(IStorageProvider storageProvider, FileWatcherService service)
    {
        _storageProvider = storageProvider;
        _service = service;
        _service.TreeUpdated += OnTreeUpdated;

        SelectFolderCommand = new AsyncRelayCommand(SelectFolderAsync);
        Root = _service.RootNode;

        this.WhenAnyValue(vm => vm.SelectedTab)
            .Subscribe(OnTabChanged);
    }

    private void OnTabChanged(EntityNodeViewModel? tab)
    {
        var pathLasting = tab?.FullPath.Split(_service.WatchedFolder + "\\").LastOrDefault();
        if (pathLasting != null)
        {
            Breadcrumbs.Clear();
            Breadcrumbs.AddRange(pathLasting.Split('\\'));
        }
    }

    public AsyncRelayCommand SelectFolderCommand { get; set; }

    private async Task SelectFolderAsync()
    {

        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder to Watch",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var folder = folders[0];
            SelectedPath = folder.Path.LocalPath;
            _service.SetWatchedFolder(SelectedPath);
        }
    }

    [Reactive]
    public string SelectedPath { get; set; }

    private void OnTreeUpdated(object? sender, TreeUpdatedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => Root = e.NewRoot);
    }

    [Reactive]
    public EntityNodeViewModel Root { get; set; }

    [Reactive]
    public ObservableCollection<EntityNodeViewModel> SelectedNodes { get; set; } = [];

    [Reactive]
    public EntityNodeViewModel SelectedTab { get; set; }

    [Reactive]
    public ObservableCollection<string> Breadcrumbs { get; set; } = [];
}