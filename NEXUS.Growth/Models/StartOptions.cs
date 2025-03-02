using System.IO;
using System.Xml.Serialization;

namespace NEXUS.Growth.Models;


[XmlRoot("start-options")]
public class StartOptions
{
    [XmlElement("start-option")]
    public StartOption[]? Options { get; set; }
}

public class StartOption
{
    [XmlElement("process")]
    public string? Process { get; set; }
    [XmlElement("potential")]
    public string? Potential { get; set; }
    [XmlElement("element")]
    public string? Element { get; set; }
    [XmlElement("atom-count")]
    public double AtomCount { get; set; }
    [XmlElement("time-steps")]
    public double TimeSteps { get; set; }
    [XmlElement("temperature-initial")]
    public double TemperatureInitial { get; set; }
    [XmlElement("temperature-2-enable")]
    public bool TemperatureIntermediateEnable { get; set; }
    [XmlElement("temperature-2")]
    public double TemperatureIntermediate { get; set; }
    [XmlElement("temperature-2-percent")]
    public double TemperatureIntermediatePercent { get; set; }
    [XmlElement("temperature-end")]
    public double TemperatureEnd { get; set; }
    [XmlElement("cycles")]
    public double Cycles { get; set; }
    [XmlElement("epitaxy")]
    public Epitaxy? Epitaxy { get; set; }
    [XmlElement("evolution")]
    public Evolution? Evolution { get; set; }
    [XmlElement("substrate")]
    public Substrate? Substrate { get; set; }
    [XmlElement("box")]
    public Box? Box { get; set; }
    [XmlElement("system")]
    public SystemSettings? System { get; set; }
}

public class Epitaxy
{
    [XmlElement("diameter")]
    public double Diameter { get; set; }
    [XmlElement("energy")]
    public double Energy { get; set; }
    [XmlElement("delay")]
    public double Delay { get; set; }
}

public class Evolution
{
    [XmlElement("initial-density")]
    public double InitialDensity { get; set; }
    [XmlElement("initial-configuration")]
    public string? InitialConfiguration { get; set; }
    [XmlElement("initial-position")]
    public string? InitialPosition { get; set; }
}

public class Substrate
{
    [XmlElement("type")]
    public string? Type { get; set; }
    [XmlElement("element")]
    public string? Element { get; set; }
    [XmlElement("agile-height")]
    public double AgileHeight { get; set; }
    [XmlElement("agile-temperature")]
    public double AgileTemperature { get; set; }
    [XmlElement("face")]
    public double Face { get; set; }
}

public class Box
{
    [XmlElement("system")]
    public string? System { get; set; }
    [XmlElement("motile")]
    public bool Motile { get; set; }
    [XmlElement("geometry")]    
    public string? Geometry { get; set; }
    [XmlElement("width")]
    public double Width { get; set; }
    [XmlElement("depth")]
    public double Depth { get; set; }
    [XmlElement("height")]
    public double Height { get; set; }
    [XmlElement("radius")]
    public double Radius { get; set; }
}

public class SystemSettings
{
    [XmlElement("cnMax")]
    public double CnMax { get; set; }
    [XmlElement("time-verlet")]
    public double TimeVerlet { get; set; }
    [XmlElement("save-velocities")]
    public bool SaveVelocities { get; set; }
    [XmlElement("angular-momentum-control")]
    public bool AngularMomentumControl { get; set; }
    [XmlElement("dump-frequency")]
    public double DumpFrequency { get; set; }
    [XmlElement("dump-summary-frequency")]
    public double DumpSummaryFrequency { get; set; }
    [XmlElement("output-method")]
    public string? OutputMethod { get; set; }
    [XmlElement("thermostate-maxwell")]
    public bool ThermostateMaxwell { get; set; }
    [XmlElement("thermostate-maxwell-threshold")]
    public double ThermostateMaxwellThreshold { get; set; }
    [XmlElement("thermostate-berendsen")]
    public bool ThermostateBerendsen { get; set; }
    [XmlElement("thermostate-berendsen-parameter")]
    public double ThermostateBerendsenParameter { get; set; }
    [XmlElement("thermostate-3d-mode")]
    public bool Thermostate3DMode { get; set; }
}

public static class StartOptionsExtensions
{
    private static string StartOptionsFolderName = "Assets\\Startup";
    private static string StartOptionsFileName = "start.xml";
    
    public static void SerializeToXml(this StartOptions options, string filePath)
    {
        var serializer = new XmlSerializer(typeof(StartOptions));
        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, options);
    }
    
    public static void SerializeToXml(this StartOptions options, Stream stream)
    {
        var serializer = new XmlSerializer(typeof(StartOptions));
        using var writer = new StreamWriter(stream);
        serializer.Serialize(writer, options);
    }

    public static StartOptions? DeserializeFromXml(string filePath)
    {
        var serializer = new XmlSerializer(typeof(StartOptions));
        using var reader = new StreamReader(filePath);
        return (StartOptions?)serializer.Deserialize(reader);
    }
    
    public static StartOptions? DeserializeFromXml(Stream stream)
    {
        var serializer = new XmlSerializer(typeof(StartOptions));
        using var reader = new StreamReader(stream);
        return (StartOptions?)serializer.Deserialize(reader);
    }

    public static StartOptions? DeserializeDefaultsFromXml()
    {
        var filePath = Path.Combine(StartOptionsFolderName, StartOptionsFileName);
        using var stream = File.Open(filePath, FileMode.Open);
        return DeserializeFromXml(stream);
    }
    
    public static void SerializeDefaultsToXml(this StartOptions options)
    {
        var filePath = Path.Combine(StartOptionsFolderName, StartOptionsFileName);
        using var stream = File.Open(filePath, FileMode.Open);
        options.SerializeToXml(stream);
    }
}