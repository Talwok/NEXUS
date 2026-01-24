using System.Xml.Serialization;

namespace NEXUS.Parsers.Cseg.Models.Elements;

public class TightBindingParameters
{
    public const string Namespace = "TightBinding";

    [XmlElement("A")]
    public double A { get; set; }
    [XmlElement("E")]
    public double E { get; set; }
    [XmlElement("p")]
    public double P { get; set; }
    [XmlElement("q")]
    public double Q { get; set; }
    [XmlElement("r0")]
    public double R0 { get; set; }
    [XmlElement("r_cut")]
    public double RCut { get; set; }
}