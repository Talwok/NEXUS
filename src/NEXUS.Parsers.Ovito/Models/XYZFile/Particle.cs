using System.Globalization;

namespace NEXUS.Parsers.Ovito.Models.XYZFile;

public class Particle(string element, double x, double y, double z)
{
    public string Element { get; set; } = element;
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double Z { get; set; } = z;

    public override string ToString()
    {
        return $"{Element} {X.ToString("F6", CultureInfo.InvariantCulture)} " +
               $"{Y.ToString("F6", CultureInfo.InvariantCulture)} " +
               $"{Z.ToString("F6", CultureInfo.InvariantCulture)}";
    }
}