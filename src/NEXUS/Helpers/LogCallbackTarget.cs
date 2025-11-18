using NLog;
using NLog.Targets;

namespace NEXUS.Helpers;

[Target(nameof(LogCallbackTarget))]
public class LogCallbackTarget : Target
{
    public Action<LogEventInfo> OnLogEvent;

    protected override void Write(LogEventInfo eventInfo)
    {
        OnLogEvent?.Invoke(eventInfo);
    }
}