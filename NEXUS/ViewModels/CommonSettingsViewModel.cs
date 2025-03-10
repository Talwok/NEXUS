using System.ComponentModel;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Helpers;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.ViewModels;

public class CommonSettingsViewModel : StatefulViewModelBase
{
    public static string FileName = "CommonSettings.json";

    [JsonConstructor]
    public CommonSettingsViewModel() : base(Path.Combine(Paths.ProgramData, Paths.AppName, FileName))
    {
    }

    [ActivatorUtilitiesConstructor]
    public CommonSettingsViewModel(Application application) : base(Path.Combine(Paths.ProgramData, Paths.AppName, FileName))
    {
        IsDarkThemeToggled = application.ActualThemeVariant == ThemeVariant.Dark;
        PropertyChanged += OnPropertyChanged;
    }

    [Reactive]
    public bool IsDarkThemeToggled { get; set; }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(Application.Current is not {} app)
            return;
        
        if (e.PropertyName == nameof(IsDarkThemeToggled))
        {
            app.RequestedThemeVariant = IsDarkThemeToggled ? ThemeVariant.Dark : ThemeVariant.Light;
        }

        if(!IsDeserializing)
            _ = Save(this);
    }
}