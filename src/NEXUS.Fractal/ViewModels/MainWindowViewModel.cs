using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Dialogs;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.Views;
using NEXUS.Helpers;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;

namespace NEXUS.Fractal.ViewModels;

public class MainWindowViewModel : MainViewModel<MainArgumentsModel>
{
    private readonly GitHubUpdater? _updater;
    private readonly IEnumerable<StatefulServiceBase> _statefulServices;

    public MainWindowViewModel(
        IStorageProvider storageProvider,
        GeometryService geometryService,
        CalculationService calculationService,
        FilterService filterService,
        SettingsScreenViewModel settings,
        IEnumerable<StatefulServiceBase> statefulServices,
        FileWatcherService fileWatcherService,
        ColorTableService colorTableService/*,
        InfoService infoService*/)
    {
        SettingsMenuItemScreen = settings;
        ProjectService = statefulServices.FirstOrDefault<ProjectService>();
        //InfoService = infoService;
        GeometryService = geometryService;
        CalculationService = calculationService;
        FileWatcherService = fileWatcherService;
        FileTree = new FileTreeViewModel(storageProvider, fileWatcherService);
        ColorTableService = colorTableService;
        _statefulServices = statefulServices;

        if (Version != null)
        {
            _updater = new GitHubUpdater(ApplicationType.Fractal, Version);
        }
        _ = CheckForUpdates();

        UpdateCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if (_updater == null)
                    return;

                var mainWindow = App.ServiceProvider.GetService<MainWindow>();

                /*var startMessage = new InfoMessageViewModel
                {
                    Title = "Обновление",
                    Message = "Приложение перезапустится по завершению обновления",
                    Icon = MaterialIconKind.Download,
                    Severity = InfoBarSeverity.Informational,
                };*/
                // InfoService.AppendMessage(startMessage, false);
                var success = await _updater.UpdateApplication();
                /*
                InfoService.RemoveMessage(startMessage);
                InfoService.AppendMessage(new InfoMessageViewModel
                {
                    Title = "Обновление",
                    Message = success ? "Обновление прошло успешно" : "Не удалось установить обновление",
                    Icon = MaterialIconKind.Download,
                    Severity = success ? InfoBarSeverity.Informational : InfoBarSeverity.Error
                });
                InfoService.RemoveMessage(startMessage);
                */

                if (success)
                    mainWindow?.Close();
            }, outputScheduler: AvaloniaScheduler.Instance);

        OpenRecentProjectCommand = ReactiveCommand.CreateFromTask<string>(ProjectService.OpenRecentProject, outputScheduler: RxApp.MainThreadScheduler);
        CreateProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.CreateProject, outputScheduler: RxApp.MainThreadScheduler);
        OpenProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.OpenProject, outputScheduler: RxApp.MainThreadScheduler);
        SaveProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.SaveProject, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
        SaveAsProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.SaveProjectAs, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
        ExportFromProjectCommand = ReactiveCommand.CreateFromTask(ProjectService.ExportFromProject, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
        ImportToProjectCommand = ReactiveCommand.CreateRunInBackground(ProjectService.ImportToProject, ProjectService.WhenAnyValue(svc => svc.HasProject), outputScheduler: RxApp.MainThreadScheduler);
        GeometryUpdateCommand = ReactiveCommand.CreateRunInBackground<GeometryUpdateType>(geometryService.UpdateGeometry, outputScheduler: RxApp.MainThreadScheduler);
        ApplyFilterCommand = ReactiveCommand.CreateRunInBackground<FilterType>(filterService.ApplyFilter, outputScheduler: RxApp.MainThreadScheduler);
        RemoveFrameCommand = ReactiveCommand.CreateRunInBackground(ProjectService.RemoveCurrentFrame, outputScheduler: RxApp.MainThreadScheduler);
        CloneFrameCommand = ReactiveCommand.CreateRunInBackground(ProjectService.CloneCurrentFrame, outputScheduler: RxApp.MainThreadScheduler);
        CalculateFractalDimensionCommand = ReactiveCommand.CreateRunInBackground<FractalDimensionType>(CalculationService.CalculateDimension, outputScheduler: RxApp.MainThreadScheduler);
        OpenSettingsDrawerCommand = ReactiveCommand.CreateFromTask(OpenSettingsDrawer);
        OpenLogsJournalDrawerCommand = ReactiveCommand.CreateFromTask(OpenLogsJournalDrawer);
    }

    public ICommand OpenLogsJournalDrawerCommand { get; set; }

    public ColorTableService ColorTableService { get; set; }

    public FileWatcherService FileWatcherService { get; set; }

    public FileTreeViewModel FileTree { get; set; }

    private async Task OpenLogsJournalDrawer(CancellationToken arg)
    {
        var options = new DrawerOptions()
        {
            Position = Position.Top,
            Buttons = DialogButton.OKCancel,
            CanLightDismiss = true,
            IsCloseButtonVisible = true,
            Title = "Лог",
            CanResize = true
        };
        var vm = new LogJournalDialogViewModel();
        await Drawer.ShowCustomModal<LogJournalDialog, LogJournalDialogViewModel, object?>(vm, options: options);

    }
    private async Task OpenSettingsDrawer(CancellationToken arg)
    {
        var options = new DrawerOptions()
        {
            Position = Position.Right,
            Buttons = DialogButton.OKCancel,
            CanLightDismiss = true,
            IsCloseButtonVisible = true,
            Title = "Настройки",
            CanResize = true
        };
        var vm = new SettingsDialogViewModel(_statefulServices);
        await Drawer.ShowCustomModal<SettingsDialog, SettingsDialogViewModel, object?>(vm, options: options);
    }

    public CalculationService CalculationService { get; set; }

    public GeometryService GeometryService { get; set; }

    public ProjectService? ProjectService { get; set; }

    public SettingsScreenViewModel SettingsMenuItemScreen { get; set; }

    public ICommand OpenRecentProjectCommand { get; set; }
    public ICommand CreateProjectCommand { get; set; }
    public ICommand OpenProjectCommand { get; set; }
    public ICommand SaveProjectCommand { get; set; }
    public ICommand SaveAsProjectCommand { get; set; }
    public ICommand ExportFromProjectCommand { get; set; }
    public ICommand ImportToProjectCommand { get; set; }
    public ICommand GeometryUpdateCommand { get; }
    public ICommand ApplyFilterCommand { get; }
    public ICommand CloneFrameCommand { get; }
    public ICommand RemoveFrameCommand { get; set; }

    public ICommand CalculateFractalDimensionCommand { get; }
    public ICommand UpdateCommand { get; set; }

    [Reactive]
    public Version? UpdateVersion { get; set; }
    [Reactive]
    public bool IsUpdateFound { get; set; }
    //public InfoService InfoService { get; }
    public Version? Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
    public ICommand OpenSettingsDrawerCommand { get; set; }

    private async Task CheckForUpdates()
    {
        try
        {
            /*InfoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Обновление",
                Message = "Проверка наличия обновлений",
                Icon = MaterialIconKind.Download,
                Severity = InfoBarSeverity.Informational
            });*/
            if (_updater != null && await _updater.CheckForUpdates())
            {
                IsUpdateFound = true;
                UpdateVersion = _updater.LatestVersion;
                return;
            }
        }
        catch (Exception ex)
        {
            /*InfoService.AppendMessage(new InfoMessageViewModel
            {
                Title = "Обновление",
                Message = $"Ошибка проверкки уведомлений: {ex.Source}",
                Icon = MaterialIconKind.Download,
                Severity = InfoBarSeverity.Error
            });*/
        }
        IsUpdateFound = false;
    }
}