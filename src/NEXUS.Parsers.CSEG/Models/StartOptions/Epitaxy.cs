using System.Xml.Serialization;

namespace NEXUS.Parsers.CSEG.Models.StartOptions;

public class Epitaxy
{
    [XmlElement("diameter")]
    public double Diameter { get; set; }

    [XmlElement("energy")]
    public double Energy { get; set; }

    [XmlElement("delay")]
    public double Delay { get; set; }
}