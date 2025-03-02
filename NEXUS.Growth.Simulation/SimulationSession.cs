namespace NEXUS.Growth.Simulation;

public class SimulationSession
{
    public SimulationSession(SimulationSessionOptions options)
    {
        Options = options;
    }

    public SimulationSessionOptions Options { get; }

    public async Task StartSimulation()
    {
        
    }
}