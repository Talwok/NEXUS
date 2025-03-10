using System.Collections.Generic;
using Material.Icons;
using NEXUS.Extensions;
using NEXUS.Fractal.Models;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public class MainWindowViewModel : MainViewModel<MainArguments>
{
    public MainWindowViewModel(IEnumerable<StatefulViewModelBase> viewModels, SettingsScreenViewModel settings)
    {
        ImagesMenuItem = new ScreenMenuItem
        {
            Name = "Изображения",
            Icon = MaterialIconKind.Images,
            Screen = viewModels.First<ImagesScreenViewModel>()
        };
        ChartsMenuItem = new ScreenMenuItem
        {
            Name = "Графики",
            Icon = MaterialIconKind.ChartBoxOutline,
            Screen = viewModels.First<ChartsScreenViewModel>()
        };
        SettingsMenuItem = new ScreenMenuItem
        {
            Name = "Настройки",
            Icon = MaterialIconKind.SettingsOutline,
            Screen = settings
        };
    }

    public ScreenMenuItem ChartsMenuItem { get; }

    public ScreenMenuItem ImagesMenuItem { get; }

    public ScreenMenuItem SettingsMenuItem { get; }
}