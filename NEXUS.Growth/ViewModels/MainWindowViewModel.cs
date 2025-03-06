using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(string? filePath = null)
    {
        SimulationMenuItem = new ScreenMenuItem()
        {
            Name = "Симуляции",
            Icon = MaterialIconKind.PlayBoxMultipleOutline,
            Screen = App.ServiceProvider.GetServices<StatefulViewModelBase>().First<SimulationScreenViewModel>()
        };
        StartupMenuItem = new ScreenMenuItem()
        {
            Name = "Запуск",
            Icon = MaterialIconKind.FolderPlayOutline,
            Screen = App.ServiceProvider.GetServices<StatefulViewModelBase>().First<StartupScreenViewModel>()
        };
        ViewerMenuItem = new ScreenMenuItem()
        {
            Name = "Результаты",
            Icon = MaterialIconKind.ChartBoxOutline,
            Screen = App.ServiceProvider.GetServices<StatefulViewModelBase>().First<ViewerScreenViewModel>()
        };
        SettingsMenuItem = new ScreenMenuItem()
        {
            Name = "Настройки",
            Icon = MaterialIconKind.SettingsOutline,
            Screen = App.ServiceProvider.GetServices<StatefulViewModelBase>().First<SettingsScreenViewModel>()
        };
        /*ScreenViewModel = ViewerHelper.IsFileValid(filePath)
            ? new ViewerScreenViewModel(filePath!)
            : new StartupScreenViewModel();*/
    }

    public ScreenMenuItem SimulationMenuItem { get; }
    public ScreenMenuItem StartupMenuItem { get; }
    public ScreenMenuItem ViewerMenuItem { get; }
    public ScreenMenuItem SettingsMenuItem { get; }
}

public class ScreenMenuItem : ViewModelBase
{
    [Reactive] public string Name { get; set; }

    [Reactive] public MaterialIconKind Icon { get; set; }

    [Reactive] public ViewModelBase Screen { get; set; }
}