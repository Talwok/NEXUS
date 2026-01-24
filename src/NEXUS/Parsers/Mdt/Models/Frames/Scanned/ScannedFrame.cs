namespace NEXUS.Parsers.Mdt.Models.Frames.Scanned;

public class ScannedFrame(MdtFrame frame) : MdtFrame(frame)
{
    public MdtAxisScale XScale { get; set; }
    public MdtAxisScale YScale { get; set; }
    public MdtAxisScale ZScale { get; set; }
    public byte ChannelIndex { get; set; }
    public byte Mode { get; set; }
    public ushort XResolution { get; set; }
    public ushort YResolution { get; set; }
    public ushort Ndacq { get; set; }
    public float StepLength { get; set; }
    public ushort Adt { get; set; }
    public byte AdcGainAmpLog10 { get; set; }
    public byte AdcIndex { get; set; }
    public byte S16Version { get; set; }
    public byte S17PassNum { get; set; }
    public byte ScanDir { get; set; }
    public bool PowerOf2 { get; set; }
    public float Velocity { get; set; }
    public float Setpoint { get; set; }
    public float BiasVoltage { get; set; }
    public bool Draw { get; set; }
    public int XOffset { get; set; }
    public int YOffset { get; set; }
    public bool NlCorr { get; set; }
    public ushort FrameMode { get; set; }
    public ushort FrameXRes { get; set; }
    public ushort FrameYRes { get; set; }
    public ushort FrameNDots { get; set; }
    public byte[] Dots { get; set; } = [];
    public short[] ImageBuffer { get; set; } = [];
    public uint TitleLength { get; set; }
    public string Title { get; set; } = string.Empty;
    public string XmlStuff { get; set; } = string.Empty;
}