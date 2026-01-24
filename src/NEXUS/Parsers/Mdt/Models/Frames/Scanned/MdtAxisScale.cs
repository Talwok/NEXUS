namespace NEXUS.Parsers.Mdt.Models.Frames.Scanned;

public record struct MdtAxisScale
{
    public float Offset { get; set; }
    public float Step { get; set; }
    public short Unit { get; set; }
}