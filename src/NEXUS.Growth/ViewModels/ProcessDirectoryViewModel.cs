using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class ProcessDirectoryViewModel : ViewModelBase
{
    [Reactive]
    public string? Process { get; set; }
    
    [Reactive]
    public string? ElementName { get; set; }
    
    [Reactive]
    public string? SubstrateElementName { get; set; }
    
    [Reactive]
    public double AtomCount { get; set; }

    [Reactive]
    public string Date { get; set; }

    [Reactive]
    public string Folder { get; set; }

}