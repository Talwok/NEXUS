namespace NEXUS.Parsers.Ovito.Models.XYZFile;

public class XYZFrame
{
    public int NumberOfParticles { get; set; }
    public string Comment { get; set; }
    public List<string> PropertyNames { get; set; } = [];
    public List<Particle> Particles { get; set; } = [];
    public int FrameNumber { get; set; }

    public override string ToString()
    {
        return $"Frame {FrameNumber}: {NumberOfParticles} particles, Comment: {Comment}";
    }
}