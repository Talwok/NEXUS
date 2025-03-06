using System.ComponentModel;
using Avalonia;
using Avalonia.Styling;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class SettingsScreenViewModel : StatefulViewModelBase
{
    public SettingsScreenViewModel() : base("SettingsState.json")
    {
        PropertyChanged += OnPropertyChanged;
    }
    
    [Reactive]
    public bool IsDarkThemeToggled { get; set; } = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsDarkThemeToggled))
        {
            Application.Current.RequestedThemeVariant = IsDarkThemeToggled ? ThemeVariant.Dark : ThemeVariant.Light;
        }

        if(!IsDeserializing)
            _ = Save(this);
    }
}