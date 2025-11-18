using System.Reactive.Disposables;
using NLog;
using ReactiveUI;

namespace NEXUS.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public Logger Logger { get; } = LogManager.GetCurrentClassLogger();

    public CompositeDisposable Disposable { get; } = new();
}