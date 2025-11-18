using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Threading;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public class FileWatcherService : ServiceBase
{
    private readonly Dictionary<string, EntityNodeModel> _pathToNode =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly InfoService _infoService;
    private readonly ProcessService _processService;

    private FileSystemWatcher? _watcher;

    public event EventHandler<TreeUpdatedEventArgs> TreeUpdated;

    public FileWatcherService(InfoService infoService, ProcessService processService)
    {
        _infoService = infoService;
        _processService = processService;
    }

    [Reactive] public string? WatchedPath { get; private set; }

    [Reactive] public string? WatchedFolder { get; private set; }

    public async Task SetWatchedFolder(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        WatchedPath = path;
        WatchedFolder = new DirectoryInfo(path).Name;

        Logger.Info("Watching folder: " + path);

        // Stop previous watcher if any
        DisposeWatcher();

        // Setup new watcher
        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite |
                           NotifyFilters.Size
        };
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Changed += OnChanged;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnWatcherError;
        _watcher.EnableRaisingEvents = true;

        // Build initial tree
        await Dispatcher.UIThread.InvokeAsync(UpdateTree);
    }

    private void OnCreated(object sender, FileSystemEventArgs e) =>
        Dispatcher.UIThread.InvokeAsync(() => HandleCreated(e));

    private void OnDeleted(object sender, FileSystemEventArgs e) =>
        Dispatcher.UIThread.InvokeAsync(() => HandleDeleted(e));

    private void OnChanged(object sender, FileSystemEventArgs e) =>
        Dispatcher.UIThread.InvokeAsync(() => HandleChanged(e));

    private void OnRenamed(object sender, RenamedEventArgs e) =>
        Dispatcher.UIThread.InvokeAsync(() => HandleRenamed(e));

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        // Handle error, e.g., log it
        Console.WriteLine($"Watcher error: {e.GetException().Message}");
        // Optionally rebuild tree
        Dispatcher.UIThread.InvokeAsync(UpdateTree);
    }

    private async Task UpdateTree()
    {
        _pathToNode.Clear();

        var root = new EntityNodeModel
        {
            FullPath = WatchedPath
        };

        _pathToNode[WatchedPath] = root;

        var process = new RunningProcessViewModel("Построение дерева файловой системы");

        _processService.AddProcess(process);
        try
        {
            await BuildTreeRecursive(root, WatchedPath, process);
            TreeUpdated.Invoke(this, new TreeUpdatedEventArgs(root));
            _processService.RemoveProcess(process.Id);
        }
        catch (OperationCanceledException)
        {
            // Skip if cancelled
            DisposeWatcher();

            _pathToNode.Clear();
            WatchedPath = null;
            WatchedFolder = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch (UnauthorizedAccessException)
        {
            // Skip inaccessible folders
        }
    }

    private async Task BuildTreeRecursive(EntityNodeModel parent, string path, RunningProcessViewModel process)
    {
        foreach (var dir in Directory.GetDirectories(path))
        {
            process.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            await Task.Run(async () =>
            {
                var dirNode = new EntityNodeModel
                {
                    FullPath = dir
                };
                parent.Children.Add(dirNode);
                _pathToNode[dir] = dirNode;
                await process.IncrementProgressAsync();
                await BuildTreeRecursive(dirNode, dir, process);
            });

        }

        foreach (var file in Directory.GetFiles(path))
        {
            process.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            await Task.Run(async () =>
            {
                var fileNode = new EntityNodeModel
                {
                    FullPath = file
                };
                parent.Children.Add(fileNode);
                _pathToNode[file] = fileNode;
                await process.IncrementProgressAsync();
            });
        }
    }

    private void HandleCreated(FileSystemEventArgs e)
    {
        string parentPath = Path.GetDirectoryName(e.FullPath);
        if (_pathToNode.TryGetValue(parentPath, out EntityNodeModel parent))
        {
            var newNode = new EntityNodeModel
            {
                FullPath = e.FullPath
            };
            parent.Children.Add(newNode);
            _pathToNode[e.FullPath] = newNode;
        }
    }

    private void HandleDeleted(FileSystemEventArgs e)
    {
        if (_pathToNode.TryGetValue(e.FullPath, out EntityNodeModel EntityNodeViewModel))
        {
            string parentPath = Path.GetDirectoryName(e.FullPath);
            if (_pathToNode.TryGetValue(parentPath, out EntityNodeModel parent))
            {
                parent.Children.Remove(EntityNodeViewModel);
            }

            RemoveFromDict(EntityNodeViewModel);
        }
    }

    private void HandleChanged(FileSystemEventArgs e)
    {
        // For now, do nothing as we don't track file contents or additional properties
        // If needed, could update a LastModified property or similar
    }

    private void HandleRenamed(RenamedEventArgs e)
    {
        if (_pathToNode.TryGetValue(e.OldFullPath, out var entityNodeViewModel))
        {
            UpdatePathsRecursive(entityNodeViewModel, e.FullPath);
        }
    }

    private void RemoveFromDict(EntityNodeModel entityNodeViewModel)
    {
        _pathToNode.Remove(entityNodeViewModel.FullPath, out _);
        if (Directory.Exists(entityNodeViewModel.FullPath))
        {
            foreach (var child in entityNodeViewModel.Children.ToArray())
            {
                RemoveFromDict(child);
            }
        }
    }

    private void UpdatePathsRecursive(EntityNodeModel entityNodeViewModel, string newPath)
    {
        string oldPath = entityNodeViewModel.FullPath;
        entityNodeViewModel.FullPath = newPath;
        _pathToNode.Remove(oldPath, out _);
        _pathToNode[newPath] = entityNodeViewModel;

        if (Directory.Exists(entityNodeViewModel.FullPath))
        {
            foreach (var child in entityNodeViewModel.Children)
            {
                string childName = Path.GetFileName(child.FullPath);
                string childNewPath = Path.Combine(newPath, childName);
                UpdatePathsRecursive(child, childNewPath);
            }
        }
    }

    private void DisposeWatcher()
    {
        if (_watcher == null)
            return;

        _watcher.Created -= OnCreated;
        _watcher.Deleted -= OnDeleted;
        _watcher.Changed -= OnChanged;
        _watcher.Renamed -= OnRenamed;
        _watcher.Error -= OnWatcherError;

        _watcher.Dispose();
    }
}

public class TreeUpdatedEventArgs(EntityNodeModel newRoot) : EventArgs
{
    public EntityNodeModel NewRoot { get; } = newRoot;
}