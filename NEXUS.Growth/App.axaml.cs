using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
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

        serviceCollection.AddSingleton<Application>(this);

        serviceCollection.AddCommon();
        
        serviceCollection.AddSingleton<StartupViewModel>();
        serviceCollection.AddSingleton<SettingsService>();
        serviceCollection.AddSingleton<StartupService>();
        serviceCollection.AddSingleton<SimulationService>();
        serviceCollection.AddSingleton<ViewerService>();
        
        serviceCollection.AddSingleton<SettingsScreenViewModel>();
        serviceCollection.AddSingleton<StatefulViewModelBase, SimulationScreenViewModel>();
        serviceCollection.AddSingleton<StatefulViewModelBase, StartupScreenViewModel>();
        serviceCollection.AddSingleton<StatefulViewModelBase, ViewerScreenViewModel>();
        
        serviceCollection.AddSingleton<MainWindowViewModel>();
        
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        foreach (var statefulViewModelBase in ServiceProvider.GetServices<StatefulViewModelBase>())
        {
            _ = statefulViewModelBase.Load();
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            
            dataContext.TrySetArgs(
                desktop.Args == null 
                || desktop.Args.Length == 0 
                    ? string.Empty : desktop.Args?[0]);
            
            _mainWindow = new MainWindow
            {
                DataContext = dataContext 
            };

            desktop.MainWindow = _mainWindow;
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    public static IServiceProvider ServiceProvider { get; private set; } 
    public static IStorageProvider StorageProvider => _mainWindow.StorageProvider;
}