using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Text.Json;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads;
using NEXUS.Fractal.Menu.Models;
using Prism.Events;

namespace NEXUS.Fractal.Menu.Services;

public partial class RecentFilesService : ObservableBaseObject
{
    private const string RecentsFileName = "recent-files.json";
    private const int MaxItems = 10;
    private readonly List<RecentFile> _items;
    
    private readonly string _recentsFilePath;
    
    public RecentFilesService(IEventAggregator eventAggregator)
    {
        _recentsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), RecentsFileName);
        
        _items = [];
        
        Items = new ReadOnlyObservableCollection<RecentFile>(new ObservableCollection<RecentFile>(_items.OrderByDescending(item => item.LastOpened)));
        
        eventAggregator
            .GetEvent<PubSubEvent<FileOpenedEventPayload>>()
            .Subscribe(Add)
            .DisposeWith(Disposable);

        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateRecentsExistence())
            .DisposeWith(Disposable);
    }

    public ReadOnlyObservableCollection<RecentFile> Items { get; }
    
    public IEnumerable<RecentFile> Load()
    {
        if (!File.Exists(_recentsFilePath))
            yield break;
        
        using var stream = File.OpenRead(_recentsFilePath);
        var recents = JsonSerializer.Deserialize<IEnumerable<RecentFile>>(stream);
        
        _items.AddRange(recents);
        
        UpdateRecentsExistence();
    }
    
    public void Save()
    {
        using var stream = File.OpenWrite(_recentsFilePath);
        JsonSerializer.Serialize(stream, _items);
    }
    
    private void Add(FileOpenedEventPayload payload)
    {
        var filePath = payload.FilePath;
        
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        var existing = _items.FirstOrDefault(x =>
            string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
            _items.Remove(existing);

        _items.Insert(0, new RecentFile
        {
            FilePath = filePath,
            LastOpened = DateTime.Now
        });

        while (_items.Count > MaxItems)
            _items.RemoveAt(_items.Count - 1);
        
        Save();
    }
    
    private void UpdateRecentsExistence()
    {
        foreach (var item in _items)
        {
            item.IsExisting = File.Exists(item.FilePath);
        }
    }
}
