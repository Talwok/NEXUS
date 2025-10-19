using System.Xml.Serialization;

namespace NEXUS.Parsers.CSEG.Models.StartOptions;

public class StartOption
{
    [XmlElement("process")]
    public string Process { get; set; }

    [XmlElement("potential")]
    public string Potential { get; set; }

    [XmlElement("element")]
    public string Element { get; set; }

    [XmlElement("atom-count")]
    public double AtomCount { get; set; }

    [XmlElement("time-steps")]
    public double TimeSteps { get; set; }

    [XmlElement("temperature-initial")]
    public double TemperatureInitial { get; set; }

    [XmlElement("temperature-2-enable")]
    public bool Temperature2Enable { get; set; }

    [XmlElement("temperature-end")]
    public double TemperatureEnd { get; set; }

    [XmlElement("cycles")]
    public double Cycles { get; set; }

    [XmlElement("epitaxy")]
    public Epitaxy Epitaxy { get; set; } = new();

    [XmlElement("substrate")]
    public Substrate Substrate { get; set; } = new();

    [XmlElement("box")]
    public Box Box { get; set; } = new();

    [XmlElement("system")]
    public SystemConfig System { get; set; } = new();
}
