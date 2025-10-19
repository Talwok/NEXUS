using System.Reactive.Disposables;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.Services;

public class StatefulServiceBase : StatefulViewModelBase
{
    public StatefulServiceBase(string fileName) : base(fileName)
    {

    }

    public CompositeDisposable Disposable { get; } = new();

}