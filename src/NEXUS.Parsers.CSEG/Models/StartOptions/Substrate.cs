using System.Xml.Serialization;

namespace NEXUS.Parsers.CSEG.Models.StartOptions;

public class Substrate
{
    [XmlElement("type")]
    public string Type { get; set; }

    [XmlElement("element")]
    public string Element { get; set; }

    [XmlElement("agile-height")]
    public double AgileHeight { get; set; }

    [XmlElement("agile-temperature")]
    public double AgileTemperature { get; set; }

    [XmlElement("face")]
    public double Face { get; set; }
}
