namespace NEXUS.Parsers.MDT.Models.Pallete;

public class CollorTable
{
    public ushort Index { get; set; }
    public PalleteFile Parent { get; set; }
    public string Title { get; set; }
    public List<Color> Colors { get; } = [];
}