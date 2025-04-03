using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using NEXUS.Parsers.MDT.Parsers;

namespace NEXUS.Parsers.MDT.Models.Frames;

public class MdtFrame
{
    public MdtFrame()
    {
        
    }
    
    public MdtFrame(MdtFrame frame)
    {
        Size = frame.Size;
        Type = frame.Type;
        Version = frame.Version;
        Year = frame.Year;
        Month = frame.Month;
        Day = frame.Day;
        Hour = frame.Hour;
        Minute = frame.Minute;
        Second = frame.Second;
        VarSize = frame.VarSize;
        Buffer = frame.Buffer;
    }
    
    public uint Size { get; set; }
    public FrameType Type { get; set; }
    public int Version { get; set; }
    public ushort Year { get; set; }
    public ushort Month { get; set; }
    public ushort Day { get; set; }
    public ushort Hour { get; set; }
    public ushort Minute { get; set; }
    public ushort Second { get; set; }
    public ushort VarSize { get; set; }
    public byte[] Buffer { get; set; }
    public MdtFrame FrameData { get; set; }
}