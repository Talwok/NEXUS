using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Parsers.MDT.Models.Frames.MDA;

public class MdaFrame : MdtFrame
{
    public MdaFrame(MdtFrame frame) : base(frame)
    {
    }
    
    [Reactive]
    public int DimensionsCount { get; set; }
    [Reactive]
    public int MesurandsCount { get; set; }
    [Reactive]
    public uint CellSize { get; set; }
    [Reactive]
    public ulong ArraySize { get; set; }
    [Reactive]
    public ObservableCollection<MdaCalibration> Dimensions { get; set; } = [];
    [Reactive]
    public ObservableCollection<MdaCalibration> Mesurands { get; set; } = [];
    [Reactive]
    public byte[] ImageBuffer { get; set; }
    [Reactive]
    public string Title { get; set; }
    [Reactive]
    public string XmlStuff { get; set; }
}