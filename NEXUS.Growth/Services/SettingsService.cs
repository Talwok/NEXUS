using System.ComponentModel;
using Avalonia;
using Avalonia.Styling;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.Services;

public class SettingsService : ServiceBase
{

    public SettingsService()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsDarkThemeToggled))
        {
            Application.Current.RequestedThemeVariant = IsDarkThemeToggled ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }

    [Reactive]
    public bool IsDarkThemeToggled { get; set; }
    
}