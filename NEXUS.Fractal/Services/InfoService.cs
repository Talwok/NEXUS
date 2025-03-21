using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using DynamicData;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Services;

public class InfoService : ServiceBase
{
    private SourceCache<InfoMessageViewModel, Guid> _sourceCache;
    
    public InfoService()
    {
        _sourceCache = new SourceCache<InfoMessageViewModel, Guid>(_ => Guid.NewGuid());
        
        _sourceCache.Connect()
            .ObserveOn(AvaloniaScheduler.Instance)
            .AutoRefresh()
            .ExpireAfter(_ => TimeSpan.FromSeconds(5))
            .Bind(out var messages)
            .DisposeMany()
            .Subscribe();
        
        Messages = messages;
    }

    public ReadOnlyObservableCollection<InfoMessageViewModel> Messages { get; }

    public void AppendMessage(InfoMessageViewModel message) 
        => _sourceCache.AddOrUpdate(message);
}