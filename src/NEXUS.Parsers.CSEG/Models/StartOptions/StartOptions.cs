namespace NEXUS.Parsers.CSEG.Models.StartOptions;

using System.Xml.Serialization;

[XmlRoot("start-options")]
public class StartOptions
{
    [XmlElement("start-option")]
    public StartOption StartOption { get; set; } = new();
}






