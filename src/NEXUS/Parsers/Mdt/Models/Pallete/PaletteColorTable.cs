namespace NEXUS.Parsers.Mdt.Models.Pallete;

public class PaletteColorTable
{
    public ushort Index { get; set; }
    public PalleteFile Parent { get; set; }
    public string Title { get; set; }
    public List<PaletteColor> Colors { get; } = [];
}