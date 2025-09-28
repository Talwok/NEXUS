namespace NEXUS.Parsers.CSEG.Models.SimulationSystemSnapshot;

public class SimulationSystemSnapshot
{
    public SssHeader Header { get; set; } = new();
    public List<SssCoordinate> Coordinates { get; set; } = [];
    public SssFooter Footer { get; set; } = new();
    public List<string> RawLines { get; set; } = [];
}