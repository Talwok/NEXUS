using System.Reactive.Disposables;
using ReactiveUI;

namespace NEXUS.Fractal.Services;

public abstract class ServiceBase : ReactiveObject
{
    public CompositeDisposable Disposable { get; } = new();
}