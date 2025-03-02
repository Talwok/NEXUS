using Avalonia;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Services;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(string? filePath = null)
    {
        if (Application.Current is App app)
        {
            SimulationService = app.ServiceProvider.GetRequiredService<SimulationService>();
        }
        
        SimulationMenuItem = new ScreenMenuItem()
        {
            Name = "Симуляции",
            Icon = MaterialIconKind.PlayBoxMultipleOutline,
            Screen = new SimulationScreenViewModel()
        };
        StartupMenuItem = new ScreenMenuItem()
        {
            Name = "Запуск",
            Icon = MaterialIconKind.FolderPlayOutline,
            Screen = new StartupScreenViewModel()
        };
        ViewerMenuItem = new ScreenMenuItem()
        {
            Name = "Результаты",
            Icon = MaterialIconKind.ChartBoxOutline,
            Screen = new ViewerScreenViewModel(string.Empty)
        };
        SettingsMenuItem = new ScreenMenuItem()
        {
            Name = "Настройки",
            Icon = MaterialIconKind.SettingsOutline,
            Screen = new SettingsScreenViewModel()
        };
        /*ScreenViewModel = ViewerHelper.IsFileValid(filePath)
            ? new ViewerScreenViewModel(filePath!)
            : new StartupScreenViewModel();*/
    }

    public SimulationService SimulationService { get; set; }

    public ScreenMenuItem SimulationMenuItem { get; }
    public ScreenMenuItem StartupMenuItem { get; }
    public ScreenMenuItem ViewerMenuItem { get; }
    public ScreenMenuItem SettingsMenuItem { get; }
}

public class ScreenMenuItem : ViewModelBase
{
    [Reactive] public string Name { get; set; }

    [Reactive] public MaterialIconKind Icon { get; set; }

    [Reactive] public ScreenViewModelBase Screen { get; set; }
}