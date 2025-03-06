using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.ViewModels;
using NEXUS.Fractal.Views;
using NEXUS.ViewModels;

namespace NEXUS.Fractal;

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

        serviceCollection.AddSingleton<StatefulViewModelBase, SettingsScreenViewModel>();
        
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