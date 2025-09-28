namespace NEXUS.Parsers.CSEG.Models.SimulationSystemSnapshot;

public class SssCoordinate
{
    public int Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double[] AdditionalData { get; set; } = [];
}
