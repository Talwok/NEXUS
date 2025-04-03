namespace NEXUS.Parsers.MDT.Models.Pallete;

public class PalleteCollorTable
{
    public ushort Index { get; set; }
    public PalleteFile Parent { get; set; }
    public string Title { get; set; }
    public List<PalleteColor> Colors { get; } = [];
}