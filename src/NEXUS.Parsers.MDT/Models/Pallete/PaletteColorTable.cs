namespace NEXUS.Parsers.MDT.Models.Pallete;

public class PaletteColorTable
{
    public ushort Index { get; set; }
    public PalleteFile Parent { get; set; }
    public string Title { get; set; }
    public List<PaletteColor> Colors { get; } = [];
}