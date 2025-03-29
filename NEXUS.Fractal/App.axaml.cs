using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.ViewModels;
using NEXUS.Fractal.Views;
using NEXUS.ViewModels;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

namespace NEXUS.Fractal;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<Application>(this);

        serviceCollection.AddCommon();
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        // Или для файловой версии:
        var fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        serviceCollection.AddSingleton<ImageService>(); 
        serviceCollection.AddSingleton<ChartService>(); 
        serviceCollection.AddSingleton<InfoService>();
        serviceCollection.AddSingleton<CalculationService>();
        
        serviceCollection.AddSingleton<ImagesScreenViewModel>();
        serviceCollection.AddSingleton<ChartsScreenViewModel>();
        serviceCollection.AddSingleton<SettingsScreenViewModel>();
        
        serviceCollection.AddSingleton<MainWindowViewModel>();

        var mainWindow = new MainWindow();
        
        serviceCollection.AddSingleton(mainWindow.StorageProvider);
        serviceCollection.AddSingleton(mainWindow);
        
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
            
            mainWindow.DataContext = dataContext;
            
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    public static IServiceProvider ServiceProvider { get; private set; } 
}