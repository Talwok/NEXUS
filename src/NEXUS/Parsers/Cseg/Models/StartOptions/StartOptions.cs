using System.Xml.Serialization;

namespace NEXUS.Parsers.Cseg.Models.StartOptions;

[XmlRoot("start-options")]
public class StartOptions
{
    [XmlElement("start-option")]
    public StartOption StartOption { get; set; } = new();
}






