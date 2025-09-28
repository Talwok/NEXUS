using NEXUS.Parsers.CSEG.Models.SimulationSystemSnapshot;

namespace NEXUS.Parsers.CSEG;

public class SimulationSystemSnapshotParser
{
    public static SimulationSystemSnapshot Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"SSS file not found: {filePath}");

        var sssFile = new SimulationSystemSnapshot();
        var lines = File.ReadAllLines(filePath);
        sssFile.RawLines = lines.ToList();

        if (lines.Length == 0)
            return sssFile;

        try
        {
            int currentLine = 0;

            // Парсинг заголовка (первые 3 числа)
            sssFile.Header = ParseHeader(lines, ref currentLine);

            // Парсинг координатных записей
            sssFile.Coordinates = ParseCoordinateRecords(lines, ref currentLine, sssFile.Header.Count);

            // Парсинг футера (оставшиеся строки)
            sssFile.Footer = ParseFooter(lines, ref currentLine);

            return sssFile;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Error parsing SSS file", ex);
        }
    }
    
    public static void Save(string filePath, SimulationSystemSnapshot sssFile)
    {
        using var writer = new StreamWriter(filePath);

        // Запись заголовка
        writer.WriteLine($"{sssFile.Header.Reserved1}");
        writer.WriteLine($"{sssFile.Header.Count} {sssFile.Header.Reserved2}");

        // Запись координат
        foreach (var coord in sssFile.Coordinates)
        {
            writer.WriteLine(FormatCoordinateRecord(coord));
        }

        writer.WriteLine("0");
        // Запись футера
        writer.WriteLine($"{sssFile.Footer.Type} {sssFile.Footer.Dimensions[0]:F6} {sssFile.Footer.Dimensions[1]:F6} {sssFile.Footer.Dimensions[2]:F6}");
        writer.WriteLine(sssFile.Footer.SpecialValue);
        writer.WriteLine($"{sssFile.Footer.Origin[0]:F6} {sssFile.Footer.Origin[1]:F6} {sssFile.Footer.Origin[2]:F6}");

        // Дополнительные строки
        foreach (var line in sssFile.Footer.AdditionalLines)
        {
            writer.WriteLine(line);
        }
    }

    private static SssHeader ParseHeader(string[] lines, ref int currentLine)
    {
        var headerLine = lines[currentLine++].Trim();
        var parts = headerLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            throw new FormatException("Invalid header format");

        return new SssHeader
        {
            Reserved1 = int.Parse(parts[0]),
            Count = int.Parse(parts[1]),
            Reserved2 = int.Parse(parts[2])
        };
    }

    private static List<SssCoordinate> ParseCoordinateRecords(string[] lines, ref int currentLine, int count)
    {
        var coordinates = new List<SssCoordinate>();

        for (int i = 0; i < count && currentLine < lines.Length; i++)
        {
            var line = lines[currentLine++].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var record = ParseCoordinateRecord(line);
            coordinates.Add(record);
        }

        return coordinates;
    }

    private static SssCoordinate ParseCoordinateRecord(string line)
    {
        var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4)
            throw new FormatException($"Invalid coordinate record: {line}");

        var record = new SssCoordinate
        {
            Type = int.Parse(parts[0]),
            X = double.Parse(parts[1]),
            Y = double.Parse(parts[2]),
            Z = double.Parse(parts[3])
        };

        // Обрабатываем дополнительные данные, если они есть
        if (parts.Length > 4)
        {
            record.AdditionalData = new double[parts.Length - 4];
            for (int i = 4; i < parts.Length; i++)
            {
                record.AdditionalData[i - 4] = double.Parse(parts[i]);
            }
        }

        return record;
    }

    private static SssFooter ParseFooter(string[] lines, ref int currentLine)
    {
        var footer = new SssFooter();

        if (currentLine >= lines.Length)
            return footer;

        // Первая строка футера (3 числа)
        var firstLine = lines[currentLine++].Trim();
        var firstParts = firstLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if (firstParts.Length >= 1) footer.Type = int.Parse(firstParts[0]);
        if (firstParts.Length >= 4)
        {
            footer.Dimensions[0] = double.Parse(firstParts[1]);
            footer.Dimensions[1] = double.Parse(firstParts[2]);
            footer.Dimensions[2] = double.Parse(firstParts[3]);
        }

        // Вторая строка футера (специальное значение)
        if (currentLine < lines.Length)
        {
            var secondLine = lines[currentLine++].Trim();
            if (int.TryParse(secondLine, out int specialValue))
            {
                footer.SpecialValue = specialValue;
            }
        }

        // Третья строка футера (координаты origin)
        if (currentLine < lines.Length)
        {
            var thirdLine = lines[currentLine++].Trim();
            var thirdParts = thirdLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            if (thirdParts.Length >= 3)
            {
                footer.Origin[0] = double.Parse(thirdParts[0]);
                footer.Origin[1] = double.Parse(thirdParts[1]);
                footer.Origin[2] = double.Parse(thirdParts[2]);
            }
        }

        // Остальные строки футера
        while (currentLine < lines.Length)
        {
            footer.AdditionalLines.Add(lines[currentLine++]);
        }

        return footer;
    }
    
    private static string FormatCoordinateRecord(SssCoordinate coord)
    {
        var baseRecord = $"{coord.Type} {coord.X:F6} {coord.Y:F6} {coord.Z:F6}";

        if (coord.AdditionalData.Length > 0)
        {
            var additional = string.Join(" ", coord.AdditionalData.Select(d => d.ToString("F6")));
            return $"{baseRecord} {additional}";
        }

        return baseRecord;
    }
}