using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.Parsers.Mdt.Models.Enums;

namespace NEXUS.Parsers.Mdt.Models.Frames.MDA;

public class MdaCalibration : ObservableObject
{
    public uint TotLen { get; set; }
    public uint NameLen { get; set; }
    public string Name { get; set; } = string.Empty; 
    public uint CommentLen { get; set; }
    public string Comment { get; set; } = string.Empty; 
    public uint UnitLen { get; set; }
    public string Unit { get; set; } = string.Empty;
    public uint AuthorLen { get; set; }
    public string Author { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public double Scale { get; set; }
    public double Bias { get; set; }
    public ulong MinIndex { get; set; }
    public ulong MaxIndex { get; set; }
    public MdaDataType DataType { get; set; }
    public ulong SiUnit { get; set; }
    public uint StructLen { get; set; }
}