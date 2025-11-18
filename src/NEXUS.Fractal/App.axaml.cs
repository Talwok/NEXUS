using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.ViewModels;
using NEXUS.Fractal.Views;
using NEXUS.Helpers;
using NEXUS.ViewModels;
using NLog;
using NLog.Config;
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
        var config = LogManager.Configuration ?? new LoggingConfiguration();
        config.AddRuleForAllLevels(new LogCallbackTarget());
        LogManager.Configuration = config;

        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<Application>(this);

        serviceCollection.AddCommon();

        serviceCollection.AddSingleton<InfoService>();
        serviceCollection.AddSingleton<CalculationService>();
        serviceCollection.AddSingleton<GeometryService>();
        serviceCollection.AddSingleton<FilterService>();
        serviceCollection.AddSingleton<FileWatcherService>();
        serviceCollection.AddSingleton<ColorTableService>();
        serviceCollection.AddSingleton<ProcessService>();

        serviceCollection.AddSingleton<FileTabsViewModel>();
        serviceCollection.AddSingleton<FileTreeViewModel>();

        serviceCollection.AddSingleton<StatefulServiceBase, ColorTableService>();

        serviceCollection.AddSingleton<StatefulServiceBase, ProjectService>();

        serviceCollection.AddSingleton<SettingsScreenViewModel>();

        serviceCollection.AddSingleton<StatefulViewModelBase, SettingsViewModel>();

        serviceCollection.AddSingleton<MainWindowViewModel>();

        var mainWindow = new MainWindow();
        mainWindow.Closing += (sender, args) =>
        {
            var statefulVms = ServiceProvider.GetServices<StatefulViewModelBase>();
            var statefulSvcs = ServiceProvider.GetServices<StatefulServiceBase>();

            foreach (var statefulVm in statefulVms)
            {
                _ = statefulVm.Save();
            }
            foreach (var statefulSvc in statefulSvcs)
            {
                _ = statefulSvc.Save();
            }
        };
        serviceCollection.AddSingleton(mainWindow.StorageProvider);
        serviceCollection.AddSingleton(mainWindow);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        foreach (var statefulVm in ServiceProvider.GetServices<StatefulViewModelBase>())
        {
            _ = statefulVm.Load();
        }

        foreach (var statefulSvc in ServiceProvider.GetServices<StatefulServiceBase>())
        {
            _ = statefulSvc.Load();
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