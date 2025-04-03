using NEXUS.Parsers.MDT.Models.Enums;

namespace NEXUS.Parsers.MDT.Models.Frames.MDA;

public class MdaCalibration
{
    public uint TotLen { get; set; }
    public uint NameLen { get; set; }
    public string Name { get; set; }
    public uint CommentLen { get; set; }
    public string Comment { get; set; }
    public uint UnitLen { get; set; }
    public string Unit { get; set; }
    public uint AuthorLen { get; set; }
    public string Author { get; set; }
    public double Accuracy { get; set; }
    public double Scale { get; set; }
    public double Bias { get; set; }
    public ulong MinIndex { get; set; }
    public ulong MaxIndex { get; set; }
    public MdaDataType DataType { get; set; }
    public ulong SiUnit { get; set; }
    public uint StructLen { get; set; }
}