using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads;
using Prism.Events;
using Prism.Services.Dialogs;

namespace NEXUS.Fractal.Explorer.Services;

public class ExplorerService : ObservableBaseObject
{
    private readonly IDialogService _dialogService;
    private readonly IEventAggregator _eventAggregator;

    private readonly PubSubEvent<MarqueeActionStartEventPayload> _marqueeActionStart;
    private readonly PubSubEvent<MarqueeActionEndEventPayload> _marqueeActionEnd;
    
    private const int EnourmousEntriesCount = 1000;

    private FileSystemWatcher? _watcher;
    private SourceList<string> _fileSource;
    
    

    public ExplorerService(IDialogService dialogService, IEventAggregator eventAggregator)
    {
        _dialogService = dialogService;
        _eventAggregator = eventAggregator;
        _marqueeActionStart = _eventAggregator.GetEvent<PubSubEvent<MarqueeActionStartEventPayload>>();
        _marqueeActionEnd = _eventAggregator.GetEvent<PubSubEvent<MarqueeActionEndEventPayload>>();

    }

    public async Task InitializeByFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        var id = Guid.NewGuid();
        
        _marqueeActionStart.Publish(new MarqueeActionStartEventPayload(
            id, 
            "Loading folder",
            "Wait until loading folder is finished"));
        InitializeWatcherByFolder(folderPath);
        await InitializeFileSourceCache(folderPath);
        _marqueeActionEnd.Publish(new MarqueeActionEndEventPayload(id));
    }

    private async Task InitializeFileSourceCache(string folderPath)
    {
        if (_fileSource is not null)
        {
            _fileSource.Clear();
            _fileSource.Dispose();
        }

        _fileSource = new SourceList<string>();

        try
        {
            var entries = await Task.Run(() => Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories ));

            var entryCount = await Task.Run(() => entries.Count());
            
            if (entryCount >= EnourmousEntriesCount)
            {
                var parameters = new DialogParameters { { "entryCount", entryCount } };
                _dialogService.ShowDialog("EnormousEntriesDialogView", parameters, result => {});
                Logger.Warn($"Enormous count of entries {entryCount} by path: {folderPath} ");
            }

            await Task.Run(() => _fileSource.AddRange(entries));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private void InitializeWatcherByFolder(string folderPath)
    {
        if (_watcher is not null)
        {
            UnsubscribeWatcherEvents(_watcher);
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

        _watcher = new FileSystemWatcher
        {
            IncludeSubdirectories = true,
            Path = folderPath,
            NotifyFilter = NotifyFilters.FileName
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.Attributes
                           | NotifyFilters.Size
                           | NotifyFilters.LastWrite
                           | NotifyFilters.LastAccess
                           | NotifyFilters.CreationTime
                           | NotifyFilters.Security
        };

        _watcher.Path = folderPath;

        SubscribeWatcherEvents(_watcher);
    }

    private void SubscribeWatcherEvents(FileSystemWatcher watcher)
    {
        watcher.Changed += OnWatcherChanged;
        watcher.Created += OnWatcherCreated;
        watcher.Deleted += OnWatcherDeleted;
        watcher.Error += OnWatcherError;
        watcher.Renamed += OnWatcherRenamed;
    }

    private void UnsubscribeWatcherEvents(FileSystemWatcher watcher)
    {
        watcher.Changed -= OnWatcherChanged;
        watcher.Created -= OnWatcherCreated;
        watcher.Deleted -= OnWatcherDeleted;
        watcher.Error -= OnWatcherError;
        watcher.Renamed -= OnWatcherRenamed;
    }

    private void OnWatcherRenamed(object sender, RenamedEventArgs e)
    {
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
    }

    private void OnWatcherDeleted(object sender, FileSystemEventArgs e)
    {
    }

    private void OnWatcherCreated(object sender, FileSystemEventArgs e)
    {
    }

    private void OnWatcherChanged(object sender, FileSystemEventArgs e)
    {
    }
}