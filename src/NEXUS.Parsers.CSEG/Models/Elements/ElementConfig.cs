namespace NEXUS.Parsers.CSEG.Models.Elements;

public class ElementConfig
{
    public GlobalParameters Global { get; set; } = new();
    public TightBindingParameters TightBinding { get; set; } = new();
}