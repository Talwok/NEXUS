using System.Xml.Serialization;

namespace NEXUS.Parsers.CSEG.Models.Elements;

public class GlobalParameters
{
    public const string Namespace = "Global";

    [XmlElement("Title")]
    public string Title { get; set; }
    [XmlElement("Name")]
    public string Name { get; set; }
    [XmlElement("Mass")]
    public double Mass { get; set; }
    [XmlElement("Diameter")]
    public double Diameter { get; set; }
    [XmlElement("LatticeType")]
    public string LatticeType { get; set; }
    [XmlElement("LatticeParameter")]
    public double LatticeParameter { get; set; }
    [XmlElement("Z1_r")]
    public double Z1R { get; set; }
    [XmlElement("Z1_delta")]
    public double Z1Delta { get; set; }
}