using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using DynamicData;
using NEXUS.Fractal.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public partial class FileWatcherService : ReactiveObject
{
    private FileSystemWatcher _watcher;
    private string _watchedPath;
    [Reactive]
    public string WatchedPath { get; set; }
    [Reactive]
    public string? WatchedFolder { get; set; }
    [Reactive]
    public EntityNodeViewModel RootNode { get; set; }

    private readonly Dictionary<string, EntityNodeViewModel> _pathToNode = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<TreeUpdatedEventArgs> TreeUpdated;

    public FileWatcherService()
    {
        RootNode = new EntityNodeViewModel { Name = "No folder selected" };
    }

    public void SetWatchedFolder(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        WatchedPath = path;
        WatchedFolder = new DirectoryInfo(path).Name;

        // Stop previous watcher if any
        _watcher?.Dispose();

        // Setup new watcher
        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Changed += OnChanged;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnWatcherError;
        _watcher.EnableRaisingEvents = true;

        // Build initial tree
        UpdateTree();
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => HandleCreated(e));
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => HandleDeleted(e));
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => HandleChanged(e));
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => HandleRenamed(e));
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        // Handle error, e.g., log it
        Console.WriteLine($"Watcher error: {e.GetException().Message}");
        // Optionally rebuild tree
        Dispatcher.UIThread.InvokeAsync(UpdateTree);
    }

    private void UpdateTree()
    {
        _pathToNode.Clear();

        var root = new EntityNodeViewModel
        {
            Name = Path.GetFileName(WatchedPath) ?? WatchedPath,
            IsDirectory = true,
            FullPath = WatchedPath
        };

        _pathToNode[WatchedPath] = root;

        BuildTreeRecursive(root, WatchedPath);

        RootNode = root;
        TreeUpdated?.Invoke(this, new TreeUpdatedEventArgs(root));
    }

    private void BuildTreeRecursive(EntityNodeViewModel parent, string path)
    {
        try
        {
            // Add directories
            foreach (var dir in Directory.GetDirectories(path))
            {
                var dirNode = new EntityNodeViewModel
                {
                    Name = Path.GetFileName(dir),
                    IsDirectory = true,
                    FullPath = dir
                };
                parent.Children.Add(dirNode);
                _pathToNode[dir] = dirNode;
                BuildTreeRecursive(dirNode, dir);
            }

            // Add files
            foreach (var file in Directory.GetFiles(path))
            {
                var fileNode = new EntityNodeViewModel
                {
                    Name = Path.GetFileName(file),
                    IsDirectory = false,
                    FullPath = file,
                    Extension = Path.GetExtension(file)
                };
                parent.Children.Add(fileNode);
                _pathToNode[file] = fileNode;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip inaccessible folders
        }
    }

    private void HandleCreated(FileSystemEventArgs e)
    {
        string parentPath = Path.GetDirectoryName(e.FullPath);
        if (_pathToNode.TryGetValue(parentPath, out EntityNodeViewModel parent))
        {
            bool isDir = Directory.Exists(e.FullPath);
            EntityNodeViewModel newNode = new EntityNodeViewModel
            {
                Name = Path.GetFileName(e.FullPath),
                IsDirectory = isDir,
                FullPath = e.FullPath,
                Extension = Path.GetExtension(e.FullPath)
            };
            parent.Children.Add(newNode);
            _pathToNode[e.FullPath] = newNode;

            // If it's a directory, no need to build recursive here; sub-events will handle additions
        }
    }

    private void HandleDeleted(FileSystemEventArgs e)
    {
        if (_pathToNode.TryGetValue(e.FullPath, out EntityNodeViewModel EntityNodeViewModel))
        {
            string parentPath = Path.GetDirectoryName(e.FullPath);
            if (_pathToNode.TryGetValue(parentPath, out EntityNodeViewModel parent))
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
        if (_pathToNode.TryGetValue(e.OldFullPath, out EntityNodeViewModel EntityNodeViewModel))
        {
            EntityNodeViewModel.Name = Path.GetFileName(e.FullPath);
            UpdatePathsRecursive(EntityNodeViewModel, e.FullPath);
        }
    }

    private void RemoveFromDict(EntityNodeViewModel EntityNodeViewModel)
    {
        _pathToNode.Remove(EntityNodeViewModel.FullPath);
        if (EntityNodeViewModel.IsDirectory)
        {
            foreach (var child in EntityNodeViewModel.Children.ToArray())
            {
                RemoveFromDict(child);
            }
        }
    }

    private void UpdatePathsRecursive(EntityNodeViewModel entityNodeViewModel, string newPath)
    {
        string oldPath = entityNodeViewModel.FullPath;
        entityNodeViewModel.FullPath = newPath;
        entityNodeViewModel.Extension = Path.GetExtension(newPath);
        _pathToNode.Remove(oldPath);
        _pathToNode[newPath] = entityNodeViewModel;

        if (entityNodeViewModel.IsDirectory)
        {
            foreach (var child in entityNodeViewModel.Children)
            {
                string childNewPath = Path.Combine(newPath, child.Name);
                UpdatePathsRecursive(child, childNewPath);
            }
        }
    }
}

public class TreeUpdatedEventArgs : EventArgs
{
    public EntityNodeViewModel NewRoot { get; }

    public TreeUpdatedEventArgs(EntityNodeViewModel newRoot)
    {
        NewRoot = newRoot;
    }
}