using System;
using System.Reflection;
using Material.Icons;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public class MainWindowViewModel(
    ImagesScreenViewModel images,
    ChartsScreenViewModel charts,
    SettingsScreenViewModel settings,
    InfoService infoService)
    : MainViewModel<MainArguments>
{
    public ScreenMenuItem ChartsMenuItem { get; } = new()
    {
        Name = "Графики",
        Icon = MaterialIconKind.ChartBoxOutline,
        Screen = charts
    };

    public ScreenMenuItem ImagesMenuItem { get; } = new()
    {
        Name = "Изображения",
        Icon = MaterialIconKind.Images,
        Screen = images
    };

    public ScreenMenuItem SettingsMenuItem { get; } = new()
    {
        Name = "Настройки",
        Icon = MaterialIconKind.SettingsOutline,
        Screen = settings
    };

    public InfoService InfoService { get; } = infoService;
    public Version? Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
}