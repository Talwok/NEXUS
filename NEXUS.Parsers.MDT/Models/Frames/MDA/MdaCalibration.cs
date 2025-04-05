using NEXUS.Parsers.MDT.Models.Enums;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Parsers.MDT.Models.Frames.MDA;

public class MdaCalibration : ReactiveObject
{
    [Reactive]
    public uint TotLen { get; set; }
    [Reactive]
    public uint NameLen { get; set; }
    [Reactive]
    public string Name { get; set; }
    [Reactive]
    public uint CommentLen { get; set; }
    [Reactive]
    public string Comment { get; set; }
    [Reactive]
    public uint UnitLen { get; set; }
    [Reactive]
    public string Unit { get; set; }
    [Reactive]
    public uint AuthorLen { get; set; }
    [Reactive]
    public string Author { get; set; }
    [Reactive]
    public double Accuracy { get; set; }
    [Reactive]
    public double Scale { get; set; }
    [Reactive]
    public double Bias { get; set; }
    [Reactive]
    public ulong MinIndex { get; set; }
    [Reactive]
    public ulong MaxIndex { get; set; }
    [Reactive]
    public MdaDataType DataType { get; set; }
    [Reactive]
    public ulong SiUnit { get; set; }
    [Reactive]
    public uint StructLen { get; set; }
}