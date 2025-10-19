using ReactiveUI.Fody.Helpers;

namespace NEXUS.Parsers.MDT.Models.Frames.Scanned;

public class ScannedFrame(MdtFrame frame) : MdtFrame(frame)
{
    [Reactive]
    public MdtAxisScale XScale { get; set; }
    [Reactive]
    public MdtAxisScale YScale { get; set; }
    [Reactive]
    public MdtAxisScale ZScale { get; set; }
    [Reactive]
    public byte ChannelIndex { get; set; }
    [Reactive]
    public byte Mode { get; set; }
    [Reactive]
    public ushort XResolution { get; set; }
    [Reactive]
    public ushort YResolution { get; set; }
    [Reactive]
    public ushort Ndacq { get; set; }
    [Reactive]
    public float StepLength { get; set; }
    [Reactive]
    public ushort Adt { get; set; }
    [Reactive]
    public byte AdcGainAmpLog10 { get; set; }
    [Reactive]
    public byte AdcIndex { get; set; }
    [Reactive]
    public byte S16Version { get; set; }
    [Reactive]
    public byte S17PassNum { get; set; }
    [Reactive]
    public byte ScanDir { get; set; }
    [Reactive]
    public bool PowerOf2 { get; set; }
    [Reactive]
    public float Velocity { get; set; }
    [Reactive]
    public float Setpoint { get; set; }
    [Reactive]
    public float BiasVoltage { get; set; }
    [Reactive]
    public bool Draw { get; set; }
    [Reactive]
    public int XOffset { get; set; }
    [Reactive]
    public int YOffset { get; set; }
    [Reactive]
    public bool NlCorr { get; set; }
    [Reactive]
    public ushort FrameMode { get; set; }
    [Reactive]
    public ushort FrameXRes { get; set; }
    [Reactive]
    public ushort FrameYRes { get; set; }
    [Reactive]
    public ushort FrameNDots { get; set; }
    [Reactive]
    public byte[] Dots { get; set; } = [];
    [Reactive]
    public short[] ImageBuffer { get; set; } = [];
    [Reactive]
    public uint TitleLength { get; set; }
    [Reactive]
    public string Title { get; set; }
    [Reactive]
    public string XmlStuff { get; set; }
}