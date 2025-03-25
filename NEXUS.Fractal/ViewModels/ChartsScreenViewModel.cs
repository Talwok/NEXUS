using System.Text.Json.Serialization;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class ChartsScreenViewModel : ViewModelBase
{
    public ChartsScreenViewModel(ChartService chartService)
    {
        ChartService = chartService;
    }

    public ChartService ChartService { get; }

    [Reactive]
    public bool IsPaneOpened { get; set; }
}