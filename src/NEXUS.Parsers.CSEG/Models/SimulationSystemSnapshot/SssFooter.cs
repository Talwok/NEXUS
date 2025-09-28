namespace NEXUS.Parsers.CSEG.Models.SimulationSystemSnapshot;

public class SssFooter
{
    public int Type { get; set; }
    public double[] Dimensions { get; set; } = new double[3];
    public int SpecialValue { get; set; }
    public double[] Origin { get; set; } = new double[3];
    public List<string> AdditionalLines { get; set; } = [];
}
