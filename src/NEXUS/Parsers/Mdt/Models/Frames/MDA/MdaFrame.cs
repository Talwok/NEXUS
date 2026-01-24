using System.Collections.ObjectModel;

namespace NEXUS.Parsers.Mdt.Models.Frames.MDA;

public class MdaFrame(MdtFrame frame) : MdtFrame(frame)
{
    public int DimensionsCount { get; set; }
    public int MesurandsCount { get; set; }
    public uint CellSize { get; set; }
    public ulong ArraySize { get; set; }
    public List<MdaCalibration> Dimensions { get; set; } = [];
    public List<MdaCalibration> Mesurands { get; set; } = [];
    public byte[] ImageBuffer { get; set; }
    public string Title { get; set; }
    public string XmlStuff { get; set; }
}