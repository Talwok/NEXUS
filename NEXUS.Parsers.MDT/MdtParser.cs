using System.Text;

namespace NEXUS.Parsers.MDT;

public class MDTFileParser
{
    private const int MagicHeader = 0x01b093ff;
    private const int HeaderSize = 32;

    public enum FrameType : ushort
    {
        Scanned = 0,
        Spectroscopy = 1,
        Text = 3,
        OldMDA = 105,
        MDA = 106,
        Palette = 107,
        CurvesNew = 190,
        Curves = 201
    }

    public enum MDADataType : int
    {
        Int8 = -1,
        UInt8 = 1,
        Int16 = -2,
        UInt16 = 2,
        Int32 = -4,
        UInt32 = 4,
        Int64 = -8,
        UInt64 = 8,
        Float32 = -(4 + 23 * 256),
        Float64 = -(8 + 52 * 256)
    }

    public class MDTFrame
    {
        public uint Size { get; set; }
        public FrameType Type { get; set; }
        public ushort Version { get; set; }
        public ushort Year { get; set; }
        public ushort Month { get; set; }
        public ushort Day { get; set; }
        public ushort Hour { get; set; }
        public ushort Minute { get; set; }
        public ushort Second { get; set; }
        public ushort VarSize { get; set; }
        public byte[] Data { get; set; }
    }

    public class MDACalibration
    {
        public uint TotLen { get; set; }
        public uint NameLen { get; set; }
        public string Name { get; set; }
        public uint CommentLen { get; set; }
        public string Comment { get; set; }
        public uint UnitLen { get; set; }
        public string Unit { get; set; }
        public uint AuthorLen { get; set; }
        public string Author { get; set; }
        public double Accuracy { get; set; }
        public double Scale { get; set; }
        public double Bias { get; set; }
        public ulong MinIndex { get; set; }
        public ulong MaxIndex { get; set; }
        public MDADataType DataType { get; set; }
        public ulong SiUnit { get; set; }
    }

    public class MDAFrame : MDTFrame
    {
        public int NDimensions { get; set; }
        public int NMesurands { get; set; }
        public uint CellSize { get; set; }
        public ulong ArraySize { get; set; }
        public List<MDACalibration> Dimensions { get; set; } = new List<MDACalibration>();
        public List<MDACalibration> Mesurands { get; set; } = new List<MDACalibration>();
        public byte[] ImageData { get; set; }
        public string Title { get; set; }
        public string XMLStuff { get; set; }
    }

    public class MDTFile
    {
        public uint Size { get; set; }
        public ushort LastFrame { get; set; }
        public List<MDTFrame> Frames { get; set; } = new List<MDTFrame>();
    }

    public static MDTFile Parse(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(stream))
        {
            var file = new MDTFile();

            // Read magic header
            var magic = reader.ReadUInt32LE();
            if (magic != MagicHeader)
            {
                throw new InvalidDataException("Invalid MDT file format.");
            }

            // Read file size (without header)
            file.Size = reader.ReadUInt32LE();

            // Skip reserved bytes
            reader.ReadBytes(4);

            // Read last frame index
            file.LastFrame = reader.ReadUInt16LE();

            // Skip remaining reserved bytes
            reader.ReadBytes(18);

            // Read frames
            for (int i = 0; i <= file.LastFrame; i++)
            {
                var frame = ReadFrame(reader);
                file.Frames.Add(frame);
            }

            return file;
        }
    }

    private static MDTFrame ReadFrame(BinaryReader reader)
    {
        var frame = new MDTFrame();

        // Read frame size
        frame.Size = reader.ReadUInt32LE();

        // Read frame type
        frame.Type = (FrameType)reader.ReadUInt16LE();

        // Read frame version
        frame.Version = reader.ReadUInt16LE();

        // Read frame date and time
        frame.Year = reader.ReadUInt16LE();
        frame.Month = reader.ReadUInt16LE();
        frame.Day = reader.ReadUInt16LE();
        frame.Hour = reader.ReadUInt16LE();
        frame.Minute = reader.ReadUInt16LE();
        frame.Second = reader.ReadUInt16LE();

        // Read variable size
        frame.VarSize = reader.ReadUInt16LE();

        // Read frame data
        frame.Data = reader.ReadBytes((int)frame.Size - HeaderSize);

        // Parse specific frame types
        switch (frame.Type)
        {
            case FrameType.MDA:
                return ParseMDAFrame(frame);
            default:
                return frame;
        }
    }

    private static MDAFrame ParseMDAFrame(MDTFrame frame)
    {
        var mdaFrame = new MDAFrame
        {
            Size = frame.Size,
            Type = frame.Type,
            Version = frame.Version,
            Year = frame.Year,
            Month = frame.Month,
            Day = frame.Day,
            Hour = frame.Hour,
            Minute = frame.Minute,
            Second = frame.Second,
            VarSize = frame.VarSize,
            Data = frame.Data
        };

        using (var ms = new MemoryStream(frame.Data))
        using (var reader = new BinaryReader(ms))
        {
            // Read MDA header
            uint headSize = reader.ReadUInt32LE();
            uint totLen = reader.ReadUInt32LE();
            reader.ReadBytes(16 * 2 + 4); // Skip guids and frame status

            uint nameSize = reader.ReadUInt32LE();
            uint commSize = reader.ReadUInt32LE();
            uint viewInfoSize = reader.ReadUInt32LE();
            uint specSize = reader.ReadUInt32LE();
            uint sourceInfoSize = reader.ReadUInt32LE();
            uint varSize = reader.ReadUInt32LE();
            reader.ReadBytes(4); // Skip data offset
            uint dataSize = reader.ReadUInt32LE();

            // Read title
            if (nameSize > 0)
            {
                mdaFrame.Title = Encoding.UTF8.GetString(reader.ReadBytes((int)nameSize));
            }

            // Read XML stuff
            if (commSize > 0)
            {
                mdaFrame.XMLStuff = Encoding.UTF8.GetString(reader.ReadBytes((int)commSize));
            }

            // Skip FrameSpec, ViewInfo, SourceInfo and vars
            reader.ReadBytes((int)(specSize + viewInfoSize + sourceInfoSize));

            // Read array structure
            uint structLen = reader.ReadUInt32LE();
            mdaFrame.ArraySize = reader.ReadUInt64LE();
            mdaFrame.CellSize = reader.ReadUInt32LE();
            mdaFrame.NDimensions = reader.ReadInt32LE();
            mdaFrame.NMesurands = reader.ReadInt32LE();

            // Read dimensions
            for (int i = 0; i < mdaFrame.NDimensions; i++)
            {
                var calibration = ReadMDACalibration(reader);
                mdaFrame.Dimensions.Add(calibration);
            }

            // Read mesurands
            for (int i = 0; i < mdaFrame.NMesurands; i++)
            {
                var calibration = ReadMDACalibration(reader);
                mdaFrame.Mesurands.Add(calibration);
            }

            // Read image data
            mdaFrame.ImageData = ReadImageData(reader, mdaFrame);
        }

        return mdaFrame;
    }

    private static MDACalibration ReadMDACalibration(BinaryReader reader)
    {
        var calibration = new MDACalibration
        {
            TotLen = reader.ReadUInt32LE(),
            NameLen = reader.ReadUInt32LE(),
            CommentLen = reader.ReadUInt32LE(),
            UnitLen = reader.ReadUInt32LE(),
            SiUnit = reader.ReadUInt64LE(),
            Accuracy = reader.ReadDoubleLE(),
            Scale = reader.ReadDoubleLE(),
            Bias = reader.ReadDoubleLE(),
            MinIndex = reader.ReadUInt64LE(),
            MaxIndex = reader.ReadUInt64LE(),
            DataType = (MDADataType)reader.ReadInt32LE(),
            AuthorLen = reader.ReadUInt32LE()
        };

        /*
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
        */

        return calibration;
    }

    private static byte[] ReadImageData(BinaryReader reader, MDAFrame mdaFrame)
    {
        // Calculate the total size of the image data
        ulong totalSize = mdaFrame.ArraySize * mdaFrame.CellSize;

        // Read the image data
        return reader.ReadBytes((int)totalSize);
    }
}