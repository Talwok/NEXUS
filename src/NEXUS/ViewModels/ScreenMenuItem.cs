using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.ViewModels;

public class ScreenMenuItem : ViewModelBase
{
    [Reactive] 
    public string Name { get; set; }

    [Reactive] 
    public MaterialIconKind Icon { get; set; }

    [Reactive] 
    public ViewModelBase Screen { get; set; }
}