using System.Collections.Generic;
using Material.Icons;
using NEXUS.Extensions;
using NEXUS.Fractal.Models;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public class MainWindowViewModel : MainViewModel<MainArguments>
{
    public MainWindowViewModel(IEnumerable<StatefulViewModelBase> viewModels)
    {
        SettingsMenuItem = new ScreenMenuItem
        {
            Name = "Настройки",
            Icon = MaterialIconKind.SettingsOutline,
            Screen = viewModels.First<SettingsScreenViewModel>()
        };
    }
    
    public ScreenMenuItem SettingsMenuItem { get; }
}