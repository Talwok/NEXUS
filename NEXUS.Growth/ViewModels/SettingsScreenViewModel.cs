using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Services;

namespace NEXUS.Growth.ViewModels;

public class SettingsScreenViewModel : ScreenViewModelBase
{
    public SettingsScreenViewModel()
    {
        if (Application.Current is App app)
        {
            SettingsService = app.ServiceProvider.GetService<SettingsService>();
        }

    }

    public SettingsService? SettingsService { get; }
    
}