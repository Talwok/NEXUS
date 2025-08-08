using NEXUS.Parsers.MDT.Models.Frames.Scanned;

namespace NEXUS.Parsers.MDT.Models.Frames.Spectroscopy;

public class SpectroscopyFrame(MdtFrame frame) : MdtFrame(frame)
{
    public MdtAxisScale XScale { get; set; }
    public MdtAxisScale YScale { get; set; }
    public MdtAxisScale ZScale { get; set; }
    public ushort SpMode { get; set; }
    public ushort SpFilter { get; set; }
    public float UBegin { get; set; }
    public float UEnd { get; set; }
    public short ZUp { get; set; }
    public short ZDown { get; set; }
    public ushort SpAveraging { get; set; }
    public bool SpRepeat { get; set; }
    public bool SpBack { get; set; }
    public short Sp4Nx { get; set; }
    public bool SpOsc { get; set; }
    public byte SpN4 { get; set; }
    public float Sp4X0 { get; set; }
    public float Sp4Xr { get; set; }
    public short Sp4U { get; set; }
    public short Sp4I { get; set; }
    public short SpNx { get; set; }
    public byte[] SpReserved { get; set; }
    public byte SpVer { get; set; }
    public uint ScnGuid { get; set; }
    public ushort FrameMode { get; set; }
    public ushort FrameXRes { get; set; }
    public ushort FrameYRes { get; set; }
    public ushort FrameNDots { get; set; }
    public byte[] Dots { get; set; }
    public short[] Data { get; set; }
    public uint TitleLength { get; set; }
    public string Title { get; set; }
    public string XmlStuff { get; set; }
}