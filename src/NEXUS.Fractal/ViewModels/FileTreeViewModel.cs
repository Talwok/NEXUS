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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using NEXUS.Fractal.Models;

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

        //Root = new EntityNodeModel();

        SelectFolderCommand = ReactiveCommand.CreateFromTask(SelectFolderAsync, outputScheduler: RxApp.MainThreadScheduler);
        /*ExpandAllCommand = ReactiveCommand.Create(() =>
        {
            UpdateAllNodesByCallback(Root, n => n.IsExpanded = true);
            this.RaisePropertyChanged(nameof(Root));
        });
        CollapseAllCommand = ReactiveCommand.Create(() =>
        {
            UpdateAllNodesByCallback(Root, n => n.IsExpanded = false);
            this.RaisePropertyChanged(nameof(Root));
        });*/

    }

    public ICommand ExpandAllCommand { get; }
    public ICommand CollapseAllCommand { get; }
    public ICommand SelectFolderCommand { get; }

    [Reactive]
    public EntityNodeModel? Root { get; private set; }

    [Reactive]
    public ObservableCollection<EntityNodeModel>? SelectedNodes { get; set; } = [];

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
            await _service.SetWatchedFolder(folder.Path.LocalPath);
        }
    }

    private void OnTreeUpdated(object? sender, TreeUpdatedEventArgs e)
        => Dispatcher.UIThread.InvokeAsync(() => Root = e.NewRoot);

    private void UpdateAllNodesByCallback(EntityNodeModel node, Action<EntityNodeModel> callback)
    {
        if (node != null)
        {
            callback?.Invoke(node);

            foreach (var child in node.Children)
            {
                UpdateAllNodesByCallback(child, callback);
            }
        }
    }
}