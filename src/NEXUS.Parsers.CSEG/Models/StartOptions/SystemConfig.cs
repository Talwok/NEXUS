using System.Xml.Serialization;

namespace NEXUS.Parsers.CSEG.Models.StartOptions;

public class SystemConfig
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
    public string OutputMethod { get; set; }

    [XmlElement("thermostate-maxwell")]
    public bool ThermostateMaxwell { get; set; }

    [XmlElement("thermostate-maxwell-threshold")]
    public double ThermostateMaxwellThreshold { get; set; }

    [XmlElement("thermostate-berendsen")]
    public bool ThermostateBerendsen { get; set; }

    [XmlElement("thermostate-berendsen-parameter")]
    public double ThermostateBerendsenParameter { get; set; }

    [XmlElement("thermostate-3d-mode")]
    public bool Thermostate3dMode { get; set; }
}