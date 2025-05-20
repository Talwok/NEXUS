using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Extensions;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.Views;
using NEXUS.Helpers;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class MainWindowViewModel : MainViewModel<MainArguments>
{
    private readonly GitHubUpdater? _updater;

    public MainWindowViewModel(
        SettingsScreenViewModel settings,
        IEnumerable<StatefulServiceBase> statefulServices,
        InfoService infoService)
    {
        SettingsMenuItemScreen = settings;
        ProjectService = statefulServices.FirstOrDefault<ProjectService>();
        InfoService = infoService;
        
#if !DEBUG
        if (Version != null)
        {
            _updater = new GitHubUpdater(ApplicationType.Fractal, Version);
        }
        _ = CheckForUpdates();
#endif
        
        UpdateCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if(_updater == null)
                    return;
                
                var mainWindow = App.ServiceProvider.GetService<MainWindow>();

                var startMessage = new InfoMessageViewModel
                {
                    Title = "Обновление",
                    Message = "Приложение перезапустится по завершению обновления",
                    Icon = MaterialIconKind.Download,
                    Severity = InfoBarSeverity.Informational,
                };
                InfoService.AppendMessage(startMessage, false);
                var success = await _updater.UpdateApplication();
                InfoService.RemoveMessage(startMessage);
                InfoService.AppendMessage(new InfoMessageViewModel
                {
                    Title = "Обновление",
                    Message = success ? "Обновление прошло успешно" : "Не удалось установить обновление",
                    Icon = MaterialIconKind.Download,
                    Severity = success ? InfoBarSeverity.Informational : InfoBarSeverity.Error
                });
                InfoService.RemoveMessage(startMessage);

                if (success)
                    mainWindow?.Close();
            }, outputScheduler: AvaloniaScheduler.Instance);

        OpenRecentProjectCommand = ReactiveCommand.CreateFromTask<string>(ProjectService.OpenRecentProject, outputScheduler: RxApp.MainThreadScheduler);
        CreateProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.CreateProject, outputScheduler: RxApp.MainThreadScheduler);
        OpenProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.OpenProject, outputScheduler: RxApp.MainThreadScheduler);
        SaveProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.SaveProject, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
        SaveAsProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.SaveProjectAs, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
        ExportFromProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.ExportFromProject, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
        ImportToProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.ImportToProject, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
    }

    public ProjectService? ProjectService { get; set; }
    
    public SettingsScreenViewModel SettingsMenuItemScreen { get; set; }
    
    public ICommand OpenRecentProjectCommand { get; set; }
    public ICommand CreateProjectCommand { get; set; }
    public ICommand OpenProjectCommand { get; set; }
    public ICommand SaveProjectCommand { get; set; }
    public ICommand SaveAsProjectCommand { get; set; }
    public ICommand ExportFromProjectCommand { get; set; } 
    public ICommand ImportToProjectCommand { get; set; }
    public ICommand UpdateCommand { get; set; }

    [Reactive]
    public Version? UpdateVersion { get; set; }
    [Reactive]
    public bool IsUpdateFound { get; set; }
    public InfoService InfoService { get; }
    public Version? Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

    private async Task CheckForUpdates()
    {
        try
        {
            InfoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Обновление",
                Message = "Проверка наличия обновлений",
                Icon = MaterialIconKind.Download,
                Severity = InfoBarSeverity.Informational
            });
            if (_updater != null && await _updater.CheckForUpdates())
            {
                IsUpdateFound = true;
                UpdateVersion = _updater.LatestVersion;
                return;
            }
        }
        catch (Exception ex)
        {
            InfoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Обновление",
                Message = $"Ошибка проверкки уведомлений: {ex.Source}",
                Icon = MaterialIconKind.Download,
                Severity = InfoBarSeverity.Error
            });
        }
        IsUpdateFound = false;
    }
}