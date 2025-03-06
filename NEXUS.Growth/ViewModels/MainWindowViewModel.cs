using System.Collections.Generic;
using System.Linq;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Growth.Models;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class MainWindowViewModel : MainViewModel<MainArguments>
{
    public MainWindowViewModel(IEnumerable<StatefulViewModelBase> viewModels)
    {
        var viewModelBases = viewModels.ToList();
        
        SimulationMenuItem = new ScreenMenuItem()
        {
            Name = "Симуляции",
            Icon = MaterialIconKind.PlayBoxMultipleOutline,
            Screen = viewModelBases.First<SimulationScreenViewModel>()
        };
        StartupMenuItem = new ScreenMenuItem()
        {
            Name = "Запуск",
            Icon = MaterialIconKind.FolderPlayOutline,
            Screen = viewModelBases.First<StartupScreenViewModel>()
        };
        ViewerMenuItem = new ScreenMenuItem()
        {
            Name = "Результаты",
            Icon = MaterialIconKind.ChartBoxOutline,
            Screen = viewModelBases.First<ViewerScreenViewModel>()
        };
        SettingsMenuItem = new ScreenMenuItem()
        {
            Name = "Настройки",
            Icon = MaterialIconKind.SettingsOutline,
            Screen = viewModelBases.First<SettingsScreenViewModel>()
        };
        
        /*ScreenViewModel = ViewerHelper.IsFileValid(filePath)
            ? new ViewerScreenViewModel(filePath!)
            : new StartupScreenViewModel();*/
        //SelectedItem = SettingsMenuItem;
    }

    public ScreenMenuItem SimulationMenuItem { get; }
    public ScreenMenuItem StartupMenuItem { get; }
    public ScreenMenuItem ViewerMenuItem { get; }
    public ScreenMenuItem SettingsMenuItem { get; }
}