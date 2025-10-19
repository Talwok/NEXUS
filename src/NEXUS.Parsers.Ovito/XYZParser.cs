using System.Globalization;
using NEXUS.Parsers.Ovito.Models.CoordinateFile;

namespace NEXUS.Parsers.Ovito;

public class XYZParser
{
    public XYZFile Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
            throw new FormatException("Invalid XYZ file format: too few lines");

        var xyzFile = new XYZFile();

        // Parse atom count (first line)
        if (!int.TryParse(lines[0].Trim(), out int atomCount))
            throw new FormatException("Invalid atom count format");

        xyzFile.AtomCount = atomCount;

        // Parse comment (second line)
        xyzFile.Comment = lines[1].Trim();

        // Parse atoms (remaining lines)
        for (int i = 2; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 4)
                throw new FormatException($"Invalid atom data at line {i + 1}");

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
                throw new FormatException($"Invalid X coordinate at line {i + 1}");

            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                throw new FormatException($"Invalid Y coordinate at line {i + 1}");

            if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double z))
                throw new FormatException($"Invalid Z coordinate at line {i + 1}");

            var particle = new Particle(parts[0], x, y, z);
            xyzFile.Particles.Add(particle);
        }

        // Validate atom count
        if (xyzFile.Particles.Count != xyzFile.AtomCount)
            throw new FormatException($"Atom count mismatch: expected {xyzFile.AtomCount}, found {xyzFile.Particles.Count}");

        return xyzFile;
    }

    public void Save(string filePath, XYZFile xyzFile)
    {
        if (xyzFile == null)
            throw new ArgumentNullException(nameof(xyzFile));

        // Update atom count to match actual atoms
        xyzFile.AtomCount = xyzFile.Particles.Count;

        var lines = new List<string>
        {
            xyzFile.AtomCount.ToString(),
            xyzFile.Comment
        };

        lines.AddRange(xyzFile.Particles.Select(atom => atom.ToString()));

        File.WriteAllLines(filePath, lines);
    }
}