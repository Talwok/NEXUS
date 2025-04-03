namespace NEXUS.Parsers.MDT.Models.Frames.MDA;

public class MdaFrame : MdtFrame
{
    public MdaFrame(MdtFrame frame) : base(frame)
    {
        
    }
    public int NDimensions { get; set; }
    public int NMesurands { get; set; }
    public uint CellSize { get; set; }
    public ulong ArraySize { get; set; }
    public List<MdaCalibration> Dimensions { get; set; } = new();
    public List<MdaCalibration> Mesurands { get; set; } = new();
    public byte[] ImageBuffer { get; set; }
    public string Title { get; set; }
    public string XMLStuff { get; set; }
    public string Technique { get; set; }
    public uint NameSize { get; set; }
    public uint CommSize { get; set; }
}