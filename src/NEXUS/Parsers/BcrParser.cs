using System.Globalization;
using System.Text;
using NEXUS.Parsers.Bcr;

namespace NEXUS.Parsers;

public class BcrParser
{
    public enum BcrDataType
    {
        Int16,
        Float32
    }

    private static readonly byte[] MagicBcrstm = Encoding.ASCII.GetBytes("fileformat = bcrstm\n");
    private static readonly byte[] MagicBcrf = Encoding.ASCII.GetBytes("fileformat = bcrf\n");
    private static readonly byte[] MagicBcrstmUnicode = Encoding.Unicode.GetBytes("fileformat = bcrstm_unicode\n");
    private static readonly byte[] MagicBcrfUnicode = Encoding.Unicode.GetBytes("fileformat = bcrf_unicode\n");
    private static readonly byte[] MagicBcrfCRLF = Encoding.ASCII.GetBytes("fileformat = bcrf\r\n");

    private const int HeaderSizeBytes = 2048;

    private BcrParser() { }

    public static BcrFile Parse(string path)
    {
        var fileBytes = File.ReadAllBytes(path);

        if (fileBytes.Length < HeaderSizeBytes)
            throw new InvalidDataException("Файл слишком мал для формата BCR.");

        bool isUtf16 = false;
        BcrDataType bcrDataType;
        int headerSize = HeaderSizeBytes;

        if (MatchMagic(fileBytes, MagicBcrstmUnicode) || MatchMagic(fileBytes, MagicBcrfUnicode))
        {
            isUtf16 = true;
            headerSize = FindHeaderSizeUtf16(fileBytes) ?? HeaderSizeBytes * 2;
        }
        else if (MatchMagic(fileBytes, MagicBcrstm) || MatchMagic(fileBytes, MagicBcrf) || MatchMagic(fileBytes, MagicBcrfCRLF))
        {
            isUtf16 = false;
            headerSize = HeaderSizeBytes;
        }
        else
        {
            throw new InvalidDataException("Файл не распознан как BCR/BCRF.");
        }

        string headerText = isUtf16
            ? Encoding.Unicode.GetString(fileBytes, 0, headerSize)
            : Encoding.ASCII.GetString(fileBytes, 0, headerSize);

        var metadata = ParseMetadata(headerText);

        if (!metadata.TryGetValue("fileformat", out string fileFormat))
            throw new InvalidDataException("Не найден обязательный параметр 'fileformat'.");

        bcrDataType = fileFormat switch
        {
            "bcrstm" or "bcrstm_unicode" => BcrDataType.Int16,
            "bcrf" or "bcrf_unicode" => BcrDataType.Float32,
            _ => throw new InvalidDataException($"Неизвестный тип файла: {fileFormat}")
        };

        if (!metadata.TryGetValue("xpixels", out string xPixelsStr) ||
            !metadata.TryGetValue("ypixels", out string yPixelsStr))
            throw new InvalidDataException("Не найдены обязательные параметры xpixels и ypixels.");

        int xPixels = int.Parse(xPixelsStr, CultureInfo.InvariantCulture);
        int yPixels = int.Parse(yPixelsStr, CultureInfo.InvariantCulture);

        int expectedDataSize = xPixels * yPixels * (bcrDataType == BcrDataType.Int16 ? 2 : 4);
        int actualDataSize = fileBytes.Length - headerSize;

        if (expectedDataSize != actualDataSize)
            throw new InvalidDataException($"Размер данных не соответствует ожидаемому: {expectedDataSize} vs {actualDataSize}.");

        var data = new double[yPixels, xPixels];
        var voidMask = new bool[yPixels, xPixels];

        ReadData(fileBytes, headerSize, xPixels, yPixels, bcrDataType, data, voidMask);

        ApplyScaling(metadata, data, bcrDataType);

        return new BcrFile
        {
            XPixels = xPixels,
            YPixels = yPixels,
            Data = data,
            VoidMask = voidMask,
            Metadata = metadata
        };
    }

    private static bool MatchMagic(byte[] buffer, byte[] magic)
    {
        if (buffer.Length < magic.Length) return false;
        for (int i = 0; i < magic.Length; i++)
            if (buffer[i] != magic[i]) return false;
        return true;
    }

    private static int? FindHeaderSizeUtf16(byte[] buffer)
    {
        var marker = Encoding.Unicode.GetBytes("headersize");
        for (int i = 0; i < buffer.Length - marker.Length; i += 2)
        {
            bool match = true;
            for (int j = 0; j < marker.Length; j++)
            {
                if (buffer[i + j] != marker[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                int pos = i + marker.Length;
                while (pos + 1 < buffer.Length && (BitConverter.ToUInt16(buffer, pos) == ' ' || BitConverter.ToUInt16(buffer, pos) == '=')) pos += 2;
                int size = 0;
                while (pos + 1 < buffer.Length)
                {
                    ushort c = BitConverter.ToUInt16(buffer, pos);
                    if (c < '0' || c > '9') break;
                    size = size * 10 + (c - '0');
                    pos += 2;
                }
                return size * 2;
            }
        }
        return null;
    }

    private static Dictionary<string, string> ParseMetadata(string header)
    {
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var reader = new StringReader(header);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("%"))
                continue;
            int eq = line.IndexOf('=');
            if (eq > 0)
            {
                string key = line.Substring(0, eq).Trim();
                string value = line.Substring(eq + 1).Trim();
                meta[key.ToLowerInvariant()] = value;
            }
        }
        return meta;
    }

    private static void ReadData(byte[] fileBytes, int offset, int xPixels, int yPixels, BcrDataType type, double[,] data, bool[,] voidMask)
    {
        int stride = (type == BcrDataType.Int16) ? 2 : 4;
        for (int y = 0; y < yPixels; y++)
        {
            for (int x = 0; x < xPixels; x++)
            {
                int idx = offset + (y * xPixels + x) * stride;
                if (type == BcrDataType.Int16)
                {
                    short val = BitConverter.ToInt16(fileBytes, idx);
                    if (val == 32767)
                    {
                        voidMask[y, x] = true;
                        data[y, x] = 0;
                    }
                    else
                    {
                        data[y, x] = val;
                        voidMask[y, x] = false;
                    }
                }
                else if (type == BcrDataType.Float32)
                {
                    float val = BitConverter.ToSingle(fileBytes, idx);
                    if (val > 1.7e38f)
                    {
                        voidMask[y, x] = true;
                        data[y, x] = 0;
                    }
                    else
                    {
                        data[y, x] = val;
                        voidMask[y, x] = false;
                    }
                }
            }
        }
    }

    private static void ApplyScaling(Dictionary<string, string> metadata, double[,] data, BcrDataType type)
    {
        double scale = 1.0;

        if (metadata.TryGetValue("zunit", out var zUnit))
        {
            scale = ParseUnitToScale(zUnit);
        }

        if (type == BcrDataType.Int16 && metadata.TryGetValue("bit2nm", out var bit2nmStr))
        {
            if (double.TryParse(bit2nmStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double bit2nm))
            {
                scale *= bit2nm;
            }
        }

        int yLen = data.GetLength(0);
        int xLen = data.GetLength(1);
        for (int y = 0; y < yLen; y++)
        {
            for (int x = 0; x < xLen; x++)
            {
                data[y, x] *= scale;
            }
        }

        if (metadata.TryGetValue("zmin", out var zminStr) &&
            double.TryParse(zminStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double zmin))
        {
            double minVal = data.Cast<double>().Where(d => !double.IsNaN(d)).Min();
            double offset = (scale * zmin) - minVal;
            for (int y = 0; y < yLen; y++)
            {
                for (int x = 0; x < xLen; x++)
                {
                    data[y, x] += offset;
                }
            }
        }
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