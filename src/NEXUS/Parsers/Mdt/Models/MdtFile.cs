using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.Parsers.Mdt.Models.Frames;

namespace NEXUS.Parsers.Mdt.Models;

public class MdtFile : ObservableObject
{ 
    public uint Size { get; set; }
    public ushort LastFrame { get; set; }
    public List<MdtFrame> Frames { get; set; } = [];
}