using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
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
        ImagesScreenViewModel images,
        ChartsScreenViewModel charts,
        SettingsScreenViewModel settings,
        InfoService infoService)
    {
        ChartsMenuItem.Screen = charts;
        ImagesMenuItem.Screen = images;
        SettingsMenuItem.Screen = settings;
        InfoService = infoService;
        
        if (Version != null)
        {
            _updater = new GitHubUpdater(ApplicationType.Fractal, Version);
        }
        
        _ = CheckForUpdates();
        
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
    }

    public ICommand UpdateCommand { get; set; }

    public ScreenMenuItem ChartsMenuItem { get; } = new()
    {
        Name = "Графики",
        Icon = MaterialIconKind.ChartBoxOutline,
    };

    public ScreenMenuItem ImagesMenuItem { get; } = new()
    {
        Name = "Изображения",
        Icon = MaterialIconKind.Images,
    };

    public ScreenMenuItem SettingsMenuItem { get; } = new()
    {
        Name = "Настройки",
        Icon = MaterialIconKind.SettingsOutline,
    };

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
                var name = _updater.Asset?.Name;
                IsUpdateFound = true;
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