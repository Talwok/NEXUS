using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Services;

namespace NEXUS.Growth.ViewModels;

public class SimulationScreenViewModel : ScreenViewModelBase
{
    public SimulationScreenViewModel()
    {
        if (Application.Current is App app)
        {
            SimulationService = app.ServiceProvider.GetRequiredService<SimulationService>();
        }
    }

    public SimulationService SimulationService { get; set; }
}