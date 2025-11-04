using System.Globalization;

namespace NEXUS.Parsers.Ovito.Models.XYZFile;

public class Particle
{
    public int Id { get; set; }
    public string Type { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();

    public override string ToString()
    {
        return $"Particle {Id}: {Type} at ({X:F6}, {Y:F6}, {Z:F6})";
    }
}