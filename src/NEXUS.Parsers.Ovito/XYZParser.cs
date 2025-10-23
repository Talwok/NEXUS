using System.Globalization;
using NEXUS.Parsers.Ovito.Models.XYZFile;

namespace NEXUS.Parsers.Ovito;

public static class XYZParser
{
    public static XyzFile Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
            throw new FormatException("Invalid XYZ file format: too few lines");

        var xyzFile = new XyzFile();

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

        return RemoveFloatingAtoms(xyzFile);
    }

    public static XyzFile RemoveFloatingAtoms(this XyzFile xyzFile, double connectionThreshold = 2.0, double surfaceZThreshold = 10.0)
    {
        if (xyzFile?.Particles == null || xyzFile.Particles.Count == 0)
            return xyzFile;

        var particles = xyzFile.Particles;
        var connectedAtoms = new HashSet<int>();
        var surfaceAtoms = new List<int>();

        // Находим атомы поверхности (нижние по Z координате)
        for (int i = 0; i < particles.Count; i++)
        {
            if (particles[i].Z <= surfaceZThreshold)
            {
                surfaceAtoms.Add(i);
                connectedAtoms.Add(i);
            }
        }

        // Рекурсивно находим все связанные атомы от поверхности
        var atomsToCheck = new Queue<int>(surfaceAtoms);

        while (atomsToCheck.Count > 0)
        {
            int currentIndex = atomsToCheck.Dequeue();
            var currentParticle = particles[currentIndex];

            // Проверяем всех соседей
            for (int i = 0; i < particles.Count; i++)
            {
                if (connectedAtoms.Contains(i))
                    continue;

                var neighborParticle = particles[i];
                double distance = CalculateDistance(currentParticle, neighborParticle);

                // Если атом находится в пределах порогового расстояния, считаем его связанным
                if (distance <= connectionThreshold)
                {
                    connectedAtoms.Add(i);
                    atomsToCheck.Enqueue(i);
                }
            }
        }

        // Создаем новый файл только с связанными атомами
        var filteredFile = new XyzFile
        {
            Comment = $"{xyzFile.Comment} (floating atoms removed)",
            AtomCount = connectedAtoms.Count
        };

        foreach (int index in connectedAtoms.OrderBy(i => i))
        {
            filteredFile.Particles.Add(particles[index]);
        }

        return filteredFile;
    }

    private static double CalculateDistance(Particle a, Particle b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        double dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static void Save(this XyzFile xyzFile, string filePath)
    {
        Save(filePath, xyzFile);
    }

    public static void Save(string filePath, XyzFile xyzFile)
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