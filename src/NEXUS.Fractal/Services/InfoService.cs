using System;
using System.Collections.ObjectModel;
using DynamicData;
using NEXUS.Helpers;
using NLog;

namespace NEXUS.Fractal.Services;

public class InfoService : ServiceBase, IDisposable
{
    private readonly LogCallbackTarget? _logTarget;
    private readonly SourceCache<LogEventInfo, Guid> _messagesSourceCache;

    public InfoService()
    {
        if (LogManager.Configuration?.FindTargetByName<LogCallbackTarget>(nameof(LogCallbackTarget)) is { } target)
        {
            _logTarget = target;
            _logTarget.OnLogEvent += OnLogEvent;
        }

        _messagesSourceCache = new SourceCache<LogEventInfo, Guid>(_ => Guid.NewGuid());
        _messagesSourceCache.Connect()
            .Bind(out var messages)
            .Subscribe();

        LogMessages = messages;
    }

    public ReadOnlyObservableCollection<LogEventInfo> LogMessages { get; set; }

    private void OnLogEvent(LogEventInfo eventInfo)
        => _messagesSourceCache.AddOrUpdate(eventInfo);

    public void Dispose()
    {
        Disposable.Dispose();
        if (_logTarget?.OnLogEvent != null)
        {
            _logTarget.OnLogEvent -= OnLogEvent;
            _logTarget.Dispose();
        }
    }
}