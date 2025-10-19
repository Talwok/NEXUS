using System.ComponentModel;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Services;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class SimulationScreenViewModel : StatefulViewModelBase
{
    [JsonConstructor]
    public SimulationScreenViewModel() : base("SimulationsState.json")
    {
        PropertyChanged += OnPropertyChanged;
        
        if (Design.IsDesignMode)
        {
            SimulationService = new SimulationService();
        }
    }

    [ActivatorUtilitiesConstructor]
    public SimulationScreenViewModel(SimulationService simulationSvc) : this()
    {
        SimulationService = simulationSvc;
    }
    
    [Reactive]
    public SimulationService SimulationService { get; set; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(!IsDeserializing)
            _ = Save(this);
    }
}