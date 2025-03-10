using System.Text.Json.Serialization;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class ChartsScreenViewModel : StatefulViewModelBase
{
    public ChartsScreenViewModel() : base("ChartsState.json")
    {
        
    }
    
    
    [Reactive, JsonIgnore]
    public int TotalCharts { get; set; }

    [Reactive]
    public bool IsPaneOpened { get; set; }

    [Reactive] 
    public double RangeFromLimit { get; set; } = 2;

    [Reactive] 
    public double RangeToLimit { get; set; } = 3;

    [Reactive] 
    public double RangeFromValue { get; set; } = 2;

    [Reactive] 
    public double RangeToValue { get; set; } = 3;
}