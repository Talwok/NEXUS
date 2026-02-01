using System.Reactive.Disposables;

namespace NEXUS.Helpers;

public static class CompositeDisposableHelper
{
    public static void Add(this CompositeDisposable disposable, Action disposeAction) => 
        disposable.Add(Disposable.Create(disposeAction));
}