using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.ViewModels;
using NEXUS.Helpers;
using NEXUS.ViewModels;
using ReactiveUI;

namespace NEXUS.Fractal.Views;

public partial class MainWindow : Window
{
    private GitHubUpdater? _updater;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        
        if (version != null)
        {
            _updater = new GitHubUpdater(ApplicationType.Fractal, version);
        }
        
        _ = CheckForUpdates();
    }

    private async Task CheckForUpdates()
    {
        var infoService = App.ServiceProvider.GetService<InfoService>();

        try
        {
            infoService?.AppendMessage(new InfoMessageViewModel
            {
                Title = "Обновление",
                Message = "Проверка наличия обновлений",
                Icon = MaterialIconKind.Download,
                Severity = InfoBarSeverity.Informational
            });
            
            if (_updater != null && await _updater.CheckForUpdates())
            {
                var message = new InfoMessageViewModel
                {
                    Title = "Обновление",
                    Message = "Рекомендуется провести обновление",
                    Icon = MaterialIconKind.Download,
                    Severity = InfoBarSeverity.Informational,
                };
                message.Command = ReactiveCommand.CreateFromTask(
                    async () =>
                    {
                        var startMessage = new InfoMessageViewModel
                        {
                            Title = "Обновление",
                            Message = "Приложение перезапустится по завершению обновления",
                            Icon = MaterialIconKind.Download,
                            Severity = InfoBarSeverity.Informational,
                        };
                        infoService?.AppendMessage(startMessage, false);
                        var success = await _updater.UpdateApplication();
                        infoService?.RemoveMessage(startMessage);
                        infoService?.AppendMessage(new InfoMessageViewModel
                        {
                            Title = "Обновление",
                            Message = success ? "Обновление прошло успешно" : "Не удалось установить обновление",
                            Icon = MaterialIconKind.Download,
                            Severity = success ? InfoBarSeverity.Informational : InfoBarSeverity.Error
                        });
                        infoService?.RemoveMessage(startMessage);

                        if (success)
                            Close();
                    }, outputScheduler: AvaloniaScheduler.Instance);
                infoService?.AppendMessage(message, false);
            }
        }
        catch (Exception ex)
        {
            infoService?.AppendMessage(new InfoMessageViewModel
            {
                Title = "Обновление",
                Message = $"Ошибка проверкки уведомлений: {ex.Source}",
                Icon = MaterialIconKind.Download,
                Severity = InfoBarSeverity.Error
            });
        }
    }
    
    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (sender is NavigationView view && e.SelectedItem is NavigationViewItem { DataContext: ScreenMenuItem vm })
        {
            view.Content = vm.Screen;
        }
    }
}