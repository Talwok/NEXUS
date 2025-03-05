using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Services;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class SettingsScreenViewModel : StatefulViewModelBase
{
    public SettingsScreenViewModel() : base("SettingsState.json")
    {
        PropertyChanged += OnPropertyChanged;
    }
    
    [Reactive]
    public bool IsDarkThemeToggled { get; set; }
    
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