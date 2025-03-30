using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using DynamicData;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public class InfoService : ServiceBase
{
    private readonly SourceCache<InfoMessageViewModel, Guid> _sourceCache;
    
    public InfoService()
    {
        _sourceCache = new SourceCache<InfoMessageViewModel, Guid>(message => message.Id);
        
        _sourceCache.Connect()
            .ObserveOn(AvaloniaScheduler.Instance)
            .Bind(out var messages)
            .DisposeMany()
            .Subscribe();
        
        Messages = messages;
    }

    public ReadOnlyObservableCollection<InfoMessageViewModel> Messages { get; }

    [Reactive]
    public InfoMessageViewModel? LastMessage { get; private set; }
    
    /// <summary>
    /// Appending message with specified timeout
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="timeout">Timeout</param>
    private void AppendMessage(InfoMessageViewModel message, TimeSpan timeout)
    {
        _sourceCache.AddOrUpdate(message);
        
        if (timeout > TimeSpan.Zero) 
            Observable.Timer(timeout).Subscribe(_ => RemoveMessage(message));
        
        LastMessage = message;
    }
    
    /// <summary>
    /// Message with existance timeout
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="withTimeout">Must have timeout</param>
    public void AppendMessage(InfoMessageViewModel message, bool withTimeout = true) 
        => AppendMessage(message, withTimeout ? TimeSpan.FromSeconds(5) : TimeSpan.Zero);

    /// <summary>
    /// Remove message by id
    /// </summary>
    /// <param name="guid">Message id</param>
    public void RemoveMessage(Guid guid)
    {
        if (_sourceCache.Lookup(guid) is { HasValue: true, Value: { } message }) 
            RemoveMessage(message);
    }

    /// <summary>
    /// Remove message
    /// </summary>
    /// <param name="message">Message</param>
    public void RemoveMessage(InfoMessageViewModel message)
    {
        _sourceCache.Remove(message);   
        
        if (LastMessage?.Id == message.Id) 
            LastMessage = null;
    }
}