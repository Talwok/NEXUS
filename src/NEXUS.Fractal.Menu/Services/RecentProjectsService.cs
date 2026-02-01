using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Text.Json;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads.Project;
using NEXUS.Fractal.Menu.Models;
using Prism.Events;

namespace NEXUS.Fractal.Menu.Services;

public partial class RecentProjectsService : ObservableBaseObject
{
    private const string AppFolderName = "NEXUS";
    private const string RecentsFileName = "recent-folders.json";
    private const int MaxItems = 10;
    
    private readonly ObservableCollection<RecentFolder> _items;
    
    private readonly string _recentsFilePath;
    
    public RecentProjectsService(IEventAggregator eventAggregator)
    {
        _recentsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            AppFolderName,
            RecentsFileName);
        
        _items = [];
        
        Items = new ReadOnlyObservableCollection<RecentFolder>(_items);

        Load();
        
        eventAggregator
            .GetEvent<PubSubEvent<OnProjectOpenedEventPayload>>()
            .Subscribe(AddOrUpdate)
            .DisposeWith(Disposable);

        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateRecentsExistence())
            .DisposeWith(Disposable);
    }

    public ReadOnlyObservableCollection<RecentFolder> Items { get; }

    private void Load()
    {
        if (!File.Exists(_recentsFilePath))
            return;
        
        using var stream = File.OpenRead(_recentsFilePath);
        var recents = JsonSerializer.Deserialize<IEnumerable<RecentFolder>>(stream);
        
        _items.AddRange(recents);
        
        UpdateRecentsExistence();
        
        OnPropertyChanged(nameof(Items));
    }

    private void Save()
    {
        using var stream = File.Create(_recentsFilePath);
        JsonSerializer.Serialize(stream, _items);
    }
    
    private void AddOrUpdate(OnProjectOpenedEventPayload payload)
    {
        var folderPath = payload.ProjectPath;
        var openedAt = payload.OpenedAt;
        
        if (string.IsNullOrWhiteSpace(folderPath))
            return;

        var existing = _items.FirstOrDefault(x =>
            string.Equals(x.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
            _items.Remove(existing);

        _items.Insert(0, new RecentFolder
        {
            FolderPath = folderPath,
            LastOpened = openedAt
        });

        while (_items.Count > MaxItems)
            _items.RemoveAt(_items.Count - 1);
        
        Save();
        
        OnPropertyChanged(nameof(Items));
    }
    
    private void UpdateRecentsExistence()
    {
        foreach (var item in _items)
        {
            item.IsExisting = File.Exists(item.FolderPath);
        }
    }
}
