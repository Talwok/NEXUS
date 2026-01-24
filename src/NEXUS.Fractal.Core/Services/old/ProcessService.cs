/*using System.Collections.ObjectModel;
using DynamicData;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Threading;

namespace NEXUS.Fractal.Services;

public class ProcessService : ServiceBase, IDisposable
{
    private readonly SourceCache<RunningProcessViewModel, Guid> _processesSourceCache;

    public ProcessService()
    {
        _processesSourceCache = new SourceCache<RunningProcessViewModel, Guid>(p => p.Id);
        _processesSourceCache.Connect()
            .Bind(out var processes)
            .Subscribe();

        RunningProcesses = processes;
    }

    [Reactive]
    public ReadOnlyObservableCollection<RunningProcessViewModel> RunningProcesses { get; private set; }

    [Reactive]
    public RunningProcessViewModel? LastProcess { get; private set; }

    public RunningProcessViewModel AddProcess(RunningProcessViewModel process, Action? onCanceled = null)
    {
        _processesSourceCache.AddOrUpdate(process);

        process.CancellationTokenSource.Token.Register(() =>
        {
            if (LastProcess?.Id == process.Id)
                LastProcess = null;

            _processesSourceCache.Remove(process);
            onCanceled?.Invoke();
        });

        LastProcess = process;

        return process;
    }

    public RunningProcessViewModel? RemoveProcess(Guid id)
    {
        var item = _processesSourceCache.Lookup(id);
        if (item.HasValue)
        {
            var process = item.Value;
            process.CancellationTokenSource.Cancel();
            return process;
        }
        return null;
    }

    public void Dispose()
    {
        Disposable.Dispose();

        // Отменяем все оставшиеся процессы
        foreach (var process in RunningProcesses.ToList())
        {
            process.CancellationTokenSource.Cancel();
        }

        _processesSourceCache.Clear();
        _processesSourceCache.Dispose();
    }
}*/