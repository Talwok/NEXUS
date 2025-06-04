using NEXUS.Parsers.MDT.Models.Enums;
using NEXUS.Parsers.MDT.Models.Frames.MDA;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Parsers.MDT.Models.Frames;

public class MdtFrame : ReactiveObject
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
    
    [Reactive]
    public uint Size { get; set; }
    [Reactive]
    public FrameType Type { get; set; }
    [Reactive]
    public int Version { get; set; }
    [Reactive]
    public ushort Year { get; set; }
    [Reactive]
    public ushort Month { get; set; }
    [Reactive]
    public ushort Day { get; set; }
    [Reactive]
    public ushort Hour { get; set; }
    [Reactive]
    public ushort Minute { get; set; }
    [Reactive]
    public ushort Second { get; set; }
    [Reactive]
    public ushort VarSize { get; set; }
    [Reactive]
    public byte[] Buffer { get; set; }
}