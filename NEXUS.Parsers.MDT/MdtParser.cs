using System.Text;
using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models;
using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames;
using NEXUS.Parsers.MDT.Models.Frames.Curves;
using NEXUS.Parsers.MDT.Models.Frames.CurvesNew;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Models.Frames.Scanned;
using NEXUS.Parsers.MDT.Models.Frames.Spectroscopy;

namespace NEXUS.Parsers.MDT;

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
        var spectroscopyFrame = new SpectroscopyFrame(frame);
        return spectroscopyFrame;
    }

    private static MdtFrame ParseScannedFrame(MdtFrame frame)
    {
        var scannedFrame = new ScannedFrame(frame);
        return scannedFrame;
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