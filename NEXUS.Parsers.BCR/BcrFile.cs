namespace NEXUS.Parsers.BCR;

public class BcrFile
{
    public int XPixels { get; set; }
    public int YPixels { get; set; }
    public double[,] Data { get; set; }
    public bool[,] VoidMask { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}