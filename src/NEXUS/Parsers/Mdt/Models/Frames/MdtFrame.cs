using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.Parsers.Mdt.Models.Enums;

namespace NEXUS.Parsers.Mdt.Models.Frames;

public class MdtFrame : ObservableObject
{
    public MdtFrame()
    {

    }

    protected MdtFrame(MdtFrame frame)
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
    public byte[] Buffer { get; set; } = [];
}