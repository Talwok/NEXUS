using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.Services;

public class SimulationService : ServiceBase
{
    [Reactive]
    public int SimulationsCount { get; set; }
}