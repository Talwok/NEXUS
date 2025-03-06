using System.ComponentModel;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class SimulationScreenViewModel : StatefulViewModelBase
{
    public SimulationScreenViewModel() : base("SimulationsState.json")
    {
        PropertyChanged += OnPropertyChanged;
    }
        
    [Reactive]
    public int SimulationsCount { get; set; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(!IsDeserializing)
            _ = Save(this);
    }
}