using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.ViewModels;
using NEXUS.Helpers;
using NEXUS.ViewModels;

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
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
                Icon = MaterialIconKind.Internet,
                Severity = InfoBarSeverity.Informational
            });
            
            if (_updater != null && await _updater.CheckForUpdates())
            {
                infoService?.AppendMessage(new InfoMessageViewModel
                {
                    Title = "Обновление",
                    Message = "Найдено новое обновление, проводится установка",
                    Icon = MaterialIconKind.Internet,
                    Severity = InfoBarSeverity.Informational
                });

                var success = await _updater.UpdateApplication();
                infoService?.AppendMessage(new InfoMessageViewModel
                {
                    Title = "Обновление",
                    Message = success ? "Обновление прошло успешно" : "Не удалось установить обновление",
                    Icon = MaterialIconKind.Internet,
                    Severity = success ? InfoBarSeverity.Informational : InfoBarSeverity.Error
                });
                if (success) Close();
            }
        }
        catch (Exception ex)
        {
            infoService?.AppendMessage(new InfoMessageViewModel
            {
                Title = "Обновление",
                Message = $"Ошибка проверкки уведомлений: {ex.Source}",
                Icon = MaterialIconKind.Internet,
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