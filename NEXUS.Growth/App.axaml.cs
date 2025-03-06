using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Services;
using NEXUS.Growth.ViewModels;
using NEXUS.Growth.Views;
using NEXUS.ViewModels;

namespace NEXUS.Growth;

public partial class App : Application
{
    private static MainWindow _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<StartupViewModel>();
        serviceCollection.AddSingleton<SettingsService>();
        serviceCollection.AddSingleton<StartupService>();
        serviceCollection.AddSingleton<SimulationService>();
        serviceCollection.AddSingleton<ViewerService>();
        
        serviceCollection.AddSingleton<StatefulViewModelBase, SettingsScreenViewModel>();
        serviceCollection.AddSingleton<StatefulViewModelBase, SimulationScreenViewModel>();
        serviceCollection.AddSingleton<StatefulViewModelBase, StartupScreenViewModel>();
        serviceCollection.AddSingleton<StatefulViewModelBase, ViewerScreenViewModel>();
        
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        foreach (var statefulViewModelBase in ServiceProvider.GetServices<StatefulViewModelBase>())
        {
            _ = statefulViewModelBase.Load();
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(
                    desktop.Args == null || desktop.Args.Length == 0 
                        ? string.Empty 
                        : desktop.Args?[0])
            };

            desktop.MainWindow = _mainWindow;
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    public static IServiceProvider ServiceProvider { get; private set; } 
    public static IStorageProvider StorageProvider => _mainWindow.StorageProvider;
}