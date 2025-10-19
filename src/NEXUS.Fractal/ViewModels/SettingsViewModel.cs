using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Helpers;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class SettingsViewModel : StatefulViewModelBase
{
    public static string FileName = "Settings.json";

    [JsonConstructor]
    public SettingsViewModel() : base(Path.Combine(Paths.LocalAppData, Paths.AppName, FileName))
    {
    }

    [ActivatorUtilitiesConstructor]
    public SettingsViewModel(Application app) : base(Path.Combine(Paths.LocalAppData, Paths.AppName, FileName))
    {
        PropertyChanged += OnPropertyChanged;
    }

    [Reactive, JsonIgnore] public ObservableCollection<double> ColumnWidths { get; set; } = [300, 8, 0, 8, 300];

    [Reactive, JsonIgnore] public bool LeftPaneOpened { get; set; } = true;

    [Reactive, JsonIgnore] public bool RightPaneOpened { get; set; } = true;

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!IsDeserializing)
            _ = Save(this);
    }
}