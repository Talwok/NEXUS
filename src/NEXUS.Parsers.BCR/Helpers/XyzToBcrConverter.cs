using System.Globalization;
using System.Text;
using NEXUS.Parsers.Ovito.Models.XYZFile;

namespace NEXUS.Parsers.BCR.Helpers;

public static class XyzToBcrConverter
{
    public static BcrFile ConvertToBcr(this XYZFrame xyzFrame, double pixelSize = 0.5, double atomRadius = 1.34,
        double baseZ = -10.0, string unit = "nm", BcrParser.BcrDataType dataType = BcrParser.BcrDataType.Float32)
    {
        if (xyzFrame == null || xyzFrame.Particles.Count == 0)
            throw new ArgumentException("XYZ файл пустой или невалидный.");

        // Определяем bounding box
        double minX = xyzFrame.Particles.Min(p => p.X);
        double maxX = xyzFrame.Particles.Max(p => p.X);
        double minY = xyzFrame.Particles.Min(p => p.Y);
        double maxY = xyzFrame.Particles.Max(p => p.Y);

        int xPixels = (int)Math.Ceiling((maxX - minX) / pixelSize) + 1;
        int yPixels = (int)Math.Ceiling((maxY - minY) / pixelSize) + 1;

        double[,] data = new double[yPixels, xPixels];
        bool[,] voidMask = new bool[yPixels, xPixels];

        // ИЗМЕНЕНИЕ: Подложка всегда имеет высоту baseZ, voidMask = false везде!
        for (int j = 0; j < yPixels; j++)
        {
            for (int i = 0; i < xPixels; i++)
            {
                data[j, i] = baseZ; // Подложка по умолчанию
                voidMask[j, i] = false; // ВАЖНО: НЕ void!
            }
        }

        // Для каждого пикселя вычисляем высоту от атомов (перекрывает подложку)
        Parallel.For(0, yPixels, j => // Параллелизация для скорости
        {
            double gridY = minY + j * pixelSize;
            for (int i = 0; i < xPixels; i++)
            {
                double gridX = minX + i * pixelSize;
                double maxHeight = baseZ;

                foreach (var atom in xyzFrame.Particles)
                {
                    double distXY = Math.Sqrt(Math.Pow(gridX - atom.X, 2) + Math.Pow(gridY - atom.Y, 2));
                    if (distXY <= atomRadius) // <= вместо < для полного покрытия
                    {
                        double heightContribution =
                            atom.Z + Math.Sqrt(Math.Pow(atomRadius, 2) - Math.Pow(distXY, 2));
                        if (heightContribution > maxHeight)
                        {
                            maxHeight = heightContribution;
                        }
                    }
                }

                data[j, i] = maxHeight; // Атомы перекрывают подложку
            }
        });

        // Нормализация (сдвигаем min Z к 0)
        double globalMinZ = data.Cast<double>().Min();
        for (int j = 0; j < yPixels; j++)
        {
            for (int i = 0; i < xPixels; i++)
            {
                data[j, i] -= globalMinZ; // Подложка теперь на Z=0
            }
        }

        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "fileformat", dataType == BcrParser.BcrDataType.Int16 ? "bcrstm" : "bcrf" },
            { "xpixels", xPixels.ToString() },
            { "ypixels", yPixels.ToString() },
            { "zunit", unit },
            { "xlength", (maxX - minX + pixelSize).ToString("F3", CultureInfo.InvariantCulture) },
            { "ylength", (maxY - minY + pixelSize).ToString("F3", CultureInfo.InvariantCulture) },
            { "zmin", "0" }
        };

        if (dataType == BcrParser.BcrDataType.Int16)
        {
            metadata["bit2nm"] = "1.0";
        }

        // Добавляем комментарий о подложке
        metadata["comment"] = $"Generated from XYZ with substrate at Z={baseZ} and atom radius={atomRadius}";

        return new BcrFile
        {
            XPixels = xPixels,
            YPixels = yPixels,
            Data = data,
            VoidMask = voidMask, // Все false - вся поверхность валидна!
            Metadata = metadata
        };
    }

    /// <summary>
    /// Сохраняет BcrFile в файл (реализация Save для BCR).
    /// </summary>
    /// <param name="bcrFile">Объект BcrFile.</param>
    /// <param name="filePath">Путь к файлу.</param>
    public static void SaveBcrFile(BcrFile bcrFile, string filePath)
    {
        if (bcrFile == null)
            throw new ArgumentNullException(nameof(bcrFile));

        bool isInt16 = bcrFile.Metadata["fileformat"].Contains("stm");
        double scale = ParseUnitToScale(bcrFile.Metadata.GetValueOrDefault("zunit", "nm"));
        double bit2nm = 1.0;
        if (isInt16 && bcrFile.Metadata.TryGetValue("bit2nm", out var bitStr))
            double.TryParse(bitStr, out bit2nm);

        // Подготавливаем заголовок
        var header = new StringBuilder();
        foreach (var kvp in bcrFile.Metadata)
        {
            header.AppendLine($"{kvp.Key} = {kvp.Value}");
        }

        // Добавляем zmin если нужно
        double minZ = bcrFile.Data.Cast<double>().Min();
        header.AppendLine($"zmin = {minZ.ToString(CultureInfo.InvariantCulture)}");

        byte[] headerBytes = Encoding.ASCII.GetBytes(header.ToString());
        Array.Resize(ref headerBytes, 2048); // Фиксированный размер 2048 байт, заполняем нулями если меньше

        // Подготавливаем данные
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        for (int y = 0; y < bcrFile.YPixels; y++)
        {
            for (int x = 0; x < bcrFile.XPixels; x++)
            {
                double val = bcrFile.Data[y, x] / scale;
                if (bcrFile.VoidMask[y, x])
                {
                    if (isInt16) bw.Write((short)32767);
                    else bw.Write(1.7e38f);
                }
                else
                {
                    if (isInt16)
                    {
                        short scaledVal = (short)(val / bit2nm);
                        bw.Write(scaledVal);
                    }
                    else
                    {
                        bw.Write((float)val);
                    }
                }
            }
        }

        // Сохраняем файл
        using var fs = new FileStream(filePath, FileMode.Create);
        fs.Write(headerBytes, 0, headerBytes.Length);
        fs.Write(ms.ToArray(), 0, (int)ms.Length);
    }

    private static double ParseUnitToScale(string unit)
    {
        unit = unit.Trim().ToLowerInvariant();
        return unit switch
        {
            "nm" => 1.0,
            "um" => 1e3,
            "mm" => 1e6,
            "m" => 1e9,
            _ => 1.0
        };
    }
}