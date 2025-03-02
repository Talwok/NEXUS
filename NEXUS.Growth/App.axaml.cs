using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Services;
using NEXUS.Growth.ViewModels;
using NEXUS.Growth.Views;

namespace NEXUS.Growth;

public partial class App : Application
{
    private MainWindow _mainWindow;

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

        ServiceProvider = serviceCollection.BuildServiceProvider();
        
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

    public IServiceProvider ServiceProvider { get; private set; } 
    public IStorageProvider StorageProvider => _mainWindow.StorageProvider;
}