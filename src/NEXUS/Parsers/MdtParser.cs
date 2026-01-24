using System.Text;
using NEXUS.Parsers.Mdt.Helpers;
using NEXUS.Parsers.Mdt.Models;
using NEXUS.Parsers.Mdt.Models.Enums;
using NEXUS.Parsers.Mdt.Models.Frames;
using NEXUS.Parsers.Mdt.Models.Frames.Curves;
using NEXUS.Parsers.Mdt.Models.Frames.CurvesNew;
using NEXUS.Parsers.Mdt.Models.Frames.MDA;
using NEXUS.Parsers.Mdt.Models.Frames.Scanned;
using NEXUS.Parsers.Mdt.Models.Frames.Spectroscopy;

namespace NEXUS.Parsers;

public static class MdtParser
{
    public static MdtFile Parse(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);
        var file = new MdtFile();

        // Read magic header
        var magic = reader.ReadInt32Le();
        if (magic != Magic.FileMagicHeader)
        {
            throw new InvalidDataException("Invalid Mdt file format.");
        }

        // Read file size (without header)
        file.Size = reader.ReadUInt32();

        // Skip reserved bytes
        reader.ReadBytes(4);

        // Read last frame index
        file.LastFrame = reader.ReadUInt16();

        // Skip remaining reserved bytes
        reader.ReadBytes(18);
        reader.ReadByte();

        // Read frames
        for (var i = 0; i <= file.LastFrame; i++)
        {
            var frame = ReadFrame(reader);
            file.Frames.Add(frame);
        }

        return file;
    }

    private static MdtFrame ReadFrame(BinaryReader reader)
    {
        var frame = new MdtFrame();
        // Read frame size
        frame.Size = reader.ReadUInt32();
        var position = reader.BaseStream.Position;
        // Read frame type
        frame.Type = (FrameType)reader.ReadUInt16();

        // Read frame version
        frame.Version = reader.ReadUInt16();

        // Read frame date and time
        frame.Year = reader.ReadUInt16();
        frame.Month = reader.ReadUInt16();
        frame.Day = reader.ReadUInt16();
        frame.Hour = reader.ReadUInt16();
        frame.Minute = reader.ReadUInt16();
        frame.Second = reader.ReadUInt16();

        // Read variable size
        frame.VarSize = reader.ReadUInt16();

        // Read frame data
        frame.Buffer = reader.ReadBytes((int)frame.Size - Magic.FrameHeaderSize);
        var modifiedFrame = frame.Type switch
        {
            FrameType.Mda => ParseMdaFrame(frame),
            FrameType.Scanned => ParseScannedFrame(frame),
            FrameType.Spectroscopy => ParseSpectroscopyFrame(frame),
            FrameType.Text => ParseTextFrame(frame),
            FrameType.OldMda => ParseOldMdaFrame(frame),
            FrameType.Palette => ParsePaletteFrame(frame),
            FrameType.CurvesNew => ParseCurvesNewFrame(frame),
            FrameType.Curves => ParseCurvesFrame(frame),
            _ => frame
        };
        reader.BaseStream.Seek(position + frame.Size - 4, SeekOrigin.Begin);
        // Parse specific frame types
        return modifiedFrame;
    }

    private static MdtFrame ParseCurvesFrame(MdtFrame frame)
    {
        var curvesFrame = new CurvesFrame(frame);
        return curvesFrame;
    }

    private static MdtFrame ParseCurvesNewFrame(MdtFrame frame)
    {
        var curvesNewFrame = new NewCurvesFrame(frame);
        return curvesNewFrame;
    }

    private static MdtFrame ParsePaletteFrame(MdtFrame frame)
    {
        return frame;
    }

    private static MdtFrame ParseOldMdaFrame(MdtFrame frame)
    {
        return frame;
    }

    private static MdtFrame ParseTextFrame(MdtFrame frame)
    {
        return frame;
    }

    private static MdtFrame ParseSpectroscopyFrame(MdtFrame frame)
    {
        var specFrame = new SpectroscopyFrame(frame);
        using var ms = new MemoryStream(frame.Buffer);
        using var reader = new BinaryReader(ms);

        // Чтение осей X, Y, Z
        specFrame.XScale = ReadAxisScale(reader);
        specFrame.YScale = ReadAxisScale(reader);
        specFrame.ZScale = ReadAxisScale(reader);

        // Чтение параметров спектроскопии
        specFrame.SpMode = reader.ReadUInt16();
        specFrame.SpFilter = reader.ReadUInt16();
        specFrame.UBegin = reader.ReadSingle();
        specFrame.UEnd = reader.ReadSingle();
        specFrame.ZUp = reader.ReadInt16();
        specFrame.ZDown = reader.ReadInt16();
        specFrame.SpAveraging = reader.ReadUInt16();
        specFrame.SpRepeat = reader.ReadByte() != 0;
        specFrame.SpBack = reader.ReadByte() != 0;
        specFrame.Sp4Nx = reader.ReadInt16();
        specFrame.SpOsc = reader.ReadByte() != 0;
        specFrame.SpN4 = reader.ReadByte();
        specFrame.Sp4X0 = reader.ReadSingle();
        specFrame.Sp4Xr = reader.ReadSingle();
        specFrame.Sp4U = reader.ReadInt16();
        specFrame.Sp4I = reader.ReadInt16();
        specFrame.SpNx = reader.ReadInt16();
        specFrame.SpReserved = reader.ReadBytes(95);
        specFrame.SpVer = reader.ReadByte();
        specFrame.ScnGuid = reader.ReadUInt32();

        // Пропуск оставшихся байтов переменной части
        int bytesRead = 30 + 142; // Оси (30) + остальные поля (142)
        if (frame.VarSize > bytesRead)
        {
            reader.ReadBytes((int)(frame.VarSize - bytesRead));
        }

        // Чтение Frame Mode
        specFrame.FrameMode = reader.ReadUInt16();
        specFrame.FrameXRes = reader.ReadUInt16();
        specFrame.FrameYRes = reader.ReadUInt16();
        specFrame.FrameNDots = reader.ReadUInt16();

        // Чтение точек (Dots)
        if (specFrame.FrameNDots > 0)
        {
            if (frame.Type == FrameType.Spectroscopy)
            {
                specFrame.Dots = reader.ReadBytes(specFrame.FrameNDots * 4);
            }
            else if (frame.Type == FrameType.Curves)
            {
                reader.ReadBytes(14); // Пропуск заголовка точек
                specFrame.Dots = reader.ReadBytes(specFrame.FrameNDots * 16);
            }
        }

        // Чтение данных спектроскопии
        if (specFrame.FrameXRes > 0 && specFrame.FrameYRes > 0)
        {
            int dataSize = specFrame.FrameXRes * specFrame.FrameYRes;
            specFrame.Data = new short[dataSize];
            for (int i = 0; i < dataSize; i++)
            {
                specFrame.Data[i] = reader.ReadInt16();
            }
        }

        // Чтение заголовка
        if (ms.Position < ms.Length - 4)
        {
            specFrame.TitleLength = reader.ReadUInt32();
            if (specFrame.TitleLength > 0 && ms.Position + specFrame.TitleLength <= ms.Length)
            {
                specFrame.Title = Encoding.GetEncoding(1251).GetString(reader.ReadBytes((int)specFrame.TitleLength));
            }
        }

        // Чтение XML-данных
        if (ms.Position < ms.Length - 4)
        {
            uint xmlLength = reader.ReadUInt32();
            if (xmlLength > 0 && ms.Position + xmlLength <= ms.Length)
            {
                specFrame.XmlStuff = Encoding.Unicode.GetString(reader.ReadBytes((int)xmlLength));
            }
        }

        return specFrame;
    }

    private static MdtFrame ParseScannedFrame(MdtFrame frame)
    {
        var scannedFrame = new ScannedFrame(frame);
        using var ms = new MemoryStream(frame.Buffer);
        using var reader = new BinaryReader(ms);

        // Чтение осей X, Y, Z
        scannedFrame.XScale = ReadAxisScale(reader);
        scannedFrame.YScale = ReadAxisScale(reader);
        scannedFrame.ZScale = ReadAxisScale(reader);

        // Чтение остальных полей переменной части
        scannedFrame.ChannelIndex = reader.ReadByte();
        scannedFrame.Mode = reader.ReadByte();
        scannedFrame.XResolution = reader.ReadUInt16();
        scannedFrame.YResolution = reader.ReadUInt16();
        scannedFrame.Ndacq = reader.ReadUInt16();
        scannedFrame.StepLength = reader.ReadSingle();
        scannedFrame.Adt = reader.ReadUInt16();
        scannedFrame.AdcGainAmpLog10 = reader.ReadByte();
        scannedFrame.AdcIndex = reader.ReadByte();
        scannedFrame.S16Version = reader.ReadByte();
        scannedFrame.S17PassNum = reader.ReadByte();
        scannedFrame.ScanDir = reader.ReadByte();
        scannedFrame.PowerOf2 = reader.ReadByte() != 0;
        scannedFrame.Velocity = reader.ReadSingle();
        scannedFrame.Setpoint = reader.ReadSingle();
        scannedFrame.BiasVoltage = reader.ReadSingle();
        scannedFrame.Draw = reader.ReadByte() != 0;
        reader.ReadByte(); // Пропуск резервного байта
        scannedFrame.XOffset = reader.ReadInt32();
        scannedFrame.YOffset = reader.ReadInt32();
        scannedFrame.NlCorr = reader.ReadByte() != 0;

        // Пропуск оставшихся байтов переменной части
        int bytesRead = 30 + 41; // Оси (30) + остальные поля (41)
        if (frame.VarSize > bytesRead)
        {
            reader.ReadBytes(frame.VarSize - bytesRead);
        }

        // Чтение Frame Mode
        //scannedFrame.FrameMode = reader.ReadUInt16();
        scannedFrame.FrameXRes = reader.ReadUInt16();
        scannedFrame.FrameYRes = reader.ReadUInt16();
        scannedFrame.FrameNDots = reader.ReadUInt16();

        // Чтение точек (Dots)
        if (scannedFrame.FrameNDots > 0)
        {
            reader.ReadBytes(14); // Пропуск заголовка точек
            scannedFrame.Dots = reader.ReadBytes(scannedFrame.FrameNDots * 16);
        }

        // Чтение изображения
        if (scannedFrame.FrameXRes > 0 && scannedFrame.FrameYRes > 0)
        {
            int imageSize = scannedFrame.FrameXRes * scannedFrame.FrameYRes;
            scannedFrame.ImageBuffer = new short[imageSize];
            for (int i = 0; i < imageSize; i++)
            {
                scannedFrame.ImageBuffer[i] = reader.ReadInt16();
            }
        }

        // Чтение заголовка
        if (ms.Position < ms.Length - 4)
        {
            scannedFrame.TitleLength = reader.ReadUInt32();
            if (scannedFrame.TitleLength > 0 && ms.Position + scannedFrame.TitleLength <= ms.Length)
            {
                scannedFrame.Title = Encoding.UTF8.GetString(reader.ReadBytes((int)scannedFrame.TitleLength));
            }
        }

        // Чтение XML-данных
        if (ms.Position < ms.Length - 4)
        {
            uint xmlLength = reader.ReadUInt32();
            if (xmlLength > 0 && ms.Position + xmlLength <= ms.Length)
            {
                scannedFrame.XmlStuff = Encoding.Unicode.GetString(reader.ReadBytes((int)xmlLength));
            }
        }

        return scannedFrame;
    }

    // Вспомогательный метод для чтения структуры MDTAxisScale
    private static MdtAxisScale ReadAxisScale(BinaryReader reader)
    {
        return new MdtAxisScale
        {
            Offset = reader.ReadSingle(),
            Step = reader.ReadSingle(),
            Unit = reader.ReadInt16()
        };
    }

    private static MdaFrame ParseMdaFrame(MdtFrame frame)
    {
        var mdaFrame = new MdaFrame(frame);

        using var ms = new MemoryStream(frame.Buffer);
        using var reader = new BinaryReader(ms);
        // Read Mda header
        uint headSize = reader.ReadUInt32();
        uint totLen = reader.ReadUInt32();
        reader.ReadBytes(16 * 2 + 4); // Skip guids and frame status

        uint nameSize = reader.ReadUInt32();
        uint commSize = reader.ReadUInt32();
        uint viewInfoSize = reader.ReadUInt32();
        uint specSize = reader.ReadUInt32();
        uint sourceInfoSize = reader.ReadUInt32();
        uint varSize = reader.ReadUInt32();
        reader.ReadBytes(4); // Skip data offset
        uint dataSize = reader.ReadUInt32();

        // Read title
        if (nameSize > 0)
        {
            mdaFrame.Title = Encoding.UTF8.GetString(reader.ReadBytes((int)nameSize));
        }

        // Read XML stuff
        if (commSize > 0)
        {
            mdaFrame.XmlStuff = Encoding.UTF8.GetString(reader.ReadBytes((int)commSize));
        }

        // Skip FrameSpec, ViewInfo, SourceInfo and vars
        reader.ReadBytes((int)(specSize + viewInfoSize + sourceInfoSize));

        reader.ReadUInt32();
        // Read array structure
        uint structLen = reader.ReadUInt32();
        var position = reader.BaseStream.Position;
        mdaFrame.ArraySize = reader.ReadUInt64();
        mdaFrame.CellSize = reader.ReadUInt32();
        mdaFrame.DimensionsCount = reader.ReadInt32();
        mdaFrame.MesurandsCount = reader.ReadInt32();
        reader.BaseStream.Seek(structLen + position, SeekOrigin.Begin);
        // Read dimensions
        for (int i = 0; i < mdaFrame.DimensionsCount; i++)
        {
            var calibration = ReadMdaCalibration(reader);
            mdaFrame.Dimensions.Add(calibration);
        }

        // Read mesurands
        for (int i = 0; i < mdaFrame.MesurandsCount; i++)
        {
            var calibration = ReadMdaCalibration(reader);
            mdaFrame.Mesurands.Add(calibration);
        }

        // Read image data
        mdaFrame.ImageBuffer = ReadImageBuffer(reader, mdaFrame);

        return mdaFrame;
    }

    private static MdaCalibration ReadMdaCalibration(BinaryReader reader)
    {
        var calibration = new MdaCalibration();

        calibration.TotLen = reader.ReadUInt32();
        calibration.StructLen = reader.ReadUInt32();

        var structPosition = reader.BaseStream.Position + calibration.StructLen;

        calibration.NameLen = reader.ReadUInt32();
        calibration.CommentLen = reader.ReadUInt32();
        calibration.UnitLen = reader.ReadUInt32();

        calibration.SiUnit = reader.ReadUInt64();
        calibration.Accuracy = reader.ReadDouble();
        reader.ReadBytes(8);
        calibration.Scale = reader.ReadDouble();
        calibration.Bias = reader.ReadDouble();
        calibration.MinIndex = reader.ReadUInt64();
        calibration.MaxIndex = reader.ReadUInt64();
        calibration.DataType = (MdaDataType)reader.ReadInt32();
        calibration.AuthorLen = reader.ReadUInt32();

        reader.BaseStream.Seek(structPosition, SeekOrigin.Begin);

        if (calibration.NameLen > 0)
        {
            calibration.Name = Encoding.UTF8.GetString(reader.ReadBytes((int)calibration.NameLen));
        }

        if (calibration.CommentLen > 0)
        {
            calibration.Comment = Encoding.UTF8.GetString(reader.ReadBytes((int)calibration.CommentLen));
        }

        if (calibration.UnitLen > 0)
        {
            calibration.Unit = Encoding.UTF8.GetString(reader.ReadBytes((int)calibration.UnitLen));
        }

        if (calibration.AuthorLen > 0)
        {
            calibration.Author = Encoding.UTF8.GetString(reader.ReadBytes((int)calibration.AuthorLen));
        }

        return calibration;
    }

    private static byte[] ReadImageBuffer(BinaryReader reader, MdaFrame mdaFrame)
    {
        // Calculate the total size of the image data
        ulong totalSize = mdaFrame.ArraySize * mdaFrame.CellSize;

        // Read the image data
        return reader.ReadBytes((int)totalSize);
    }
}