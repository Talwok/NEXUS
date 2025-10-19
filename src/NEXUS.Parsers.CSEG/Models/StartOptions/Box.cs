using System.Xml.Serialization;

namespace NEXUS.Parsers.CSEG.Models.StartOptions;

public class Box
{
    [XmlElement("system")]
    public string System { get; set; }

    [XmlElement("motile")]
    public bool Motile { get; set; }

    [XmlElement("geometry")]
    public string Geometry { get; set; }

    [XmlElement("width")]
    public double Width { get; set; }

    [XmlElement("depth")]
    public double Depth { get; set; }

    [XmlElement("height")]
    public double Height { get; set; }
}
