using System.Collections.ObjectModel;
using NEXUS.Parsers.MDT.Models.Frames;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Parsers.MDT.Models;

public class MdtFile : ReactiveObject
{
    [Reactive]
    public uint Size { get; set; }
    [Reactive]
    public ushort LastFrame { get; set; }
    [Reactive]
    public ObservableCollection<MdtFrame> Frames { get; set; } = [];
}