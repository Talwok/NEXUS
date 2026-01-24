using System.Reactive.Disposables;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;

namespace NEXUS.BaseClasses;

public abstract class ObservableBaseObject : ObservableObject
{
    protected Logger Logger { get; } = LogManager.GetCurrentClassLogger();

    protected CompositeDisposable Disposable { get; } = new();
}