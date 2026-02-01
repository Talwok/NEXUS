using System.Globalization;
using NEXUS.Parsers.Ovito.Models.XYZFile;

namespace NEXUS.Parsers;

public static class XyzParser
{
    public static List<XyzFrame> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XYZ file not found: {filePath}");

        using var stream = File.Open(filePath, FileMode.Open);
        using var reader = new StreamReader(stream);
        return ParseStream(reader);
    }

    private static List<XyzFrame> ParseStream(TextReader reader)
    {
        List<XyzFrame> frames = [];
        int frameNumber = 0;
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // First line: number of particles
            if (!int.TryParse(line.Trim(), out int numParticles))
                throw new FormatException($"Invalid number of particles: {line}");

            // Second line: comment line (may contain property names)
            string commentLine = reader.ReadLine();
            if (commentLine == null)
                throw new FormatException("Unexpected end of file after particle count");

            var frame = new XyzFrame
            {
                FrameNumber = frameNumber++,
                NumberOfParticles = numParticles,
                Comment = commentLine.Trim()
            };

            // Parse property names from comment line (OVITO extended XYZ format)
            ParsePropertyNames(frame, commentLine);

            // Parse particles
            for (int i = 0; i < numParticles; i++)
            {
                line = reader.ReadLine();
                if (line == null)
                    throw new FormatException($"Unexpected end of file at particle {i + 1}");

                var particle = ParseParticleLine(line, frame.PropertyNames, i);
                frame.Particles.Add(particle);
            }

            frames.Add(RemoveFloatingAtoms(frame));
        }

        return frames;
    }


    public static XyzFrame RemoveFloatingAtoms(this XyzFrame inputFrame, double connectionThreshold = 2.0, double surfaceZThreshold = 10.0)
    {
        if (inputFrame?.Particles == null || inputFrame.Particles.Count == 0)
            return inputFrame;

        var particles = inputFrame.Particles;
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
        var filteredFile = new XyzFrame()
        {
            Comment = $"{inputFrame.Comment} (floating atoms removed)",
            NumberOfParticles = connectedAtoms.Count,
            FrameNumber = inputFrame.FrameNumber
        };

        foreach (var index in connectedAtoms.OrderBy(i => i))
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


    private static void ParsePropertyNames(XyzFrame frame, string commentLine)
    {
        // Look for property names in comment line
        // OVITO format: "Properties=species:S:1:pos:R:3:mass:R:1:..."
        if (commentLine.Contains("Properties="))
        {
            var propertiesSection = commentLine.Split(new[] { "Properties=" }, StringSplitOptions.None)[1];
            // Take only the part before any other keywords
            propertiesSection = propertiesSection.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries)[0];

            var propertyDefinitions = propertiesSection.Split(':');

            for (int i = 0; i < propertyDefinitions.Length; i += 3)
            {
                if (i + 2 < propertyDefinitions.Length)
                {
                    string name = propertyDefinitions[i];
                    string type = propertyDefinitions[i + 1];
                    int count = int.Parse(propertyDefinitions[i + 2]);

                    // Add property name for each component
                    if (count == 1)
                    {
                        frame.PropertyNames.Add(name);
                    }
                    else
                    {
                        for (int j = 0; j < count; j++)
                        {
                            frame.PropertyNames.Add($"{name}_{j + 1}");
                        }
                    }
                }
            }
        }
        else
        {
            // Default properties for basic XYZ format
            frame.PropertyNames.AddRange(new[] { "species", "x", "y", "z" });
        }
    }

    private static Particle ParseParticleLine(string line, List<string> propertyNames, int particleId)
    {
        var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length < 4)
            throw new FormatException($"Invalid particle data at line: {line}");

        var particle = new Particle { Id = particleId };

        // Parse basic XYZ format: species x y z [additional properties...]
        particle.Type = tokens[0];
        particle.X = ParseFloat(tokens[1]);
        particle.Y = ParseFloat(tokens[2]);
        particle.Z = ParseFloat(tokens[3]);

        // Parse additional properties
        for (int i = 4; i < tokens.Length && i - 4 < propertyNames.Count; i++)
        {
            string propertyName = propertyNames[i - 4];

            // Skip position properties as they're already handled
            if (propertyName == "x" || propertyName == "y" || propertyName == "z" || propertyName == "species")
                continue;

            particle.Properties[propertyName] = ParseValue(tokens[i]);
        }

        return particle;
    }

    private static float ParseFloat(string value)
    {
        return float.Parse(value, CultureInfo.InvariantCulture);
    }

    private static object ParseValue(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleResult))
            return doubleResult;

        if (int.TryParse(value, out int intResult))
            return intResult;

        return value; // Return as string if not numeric
    }
}