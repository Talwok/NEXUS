namespace NEXUS.Parsers.Ovito.Models.XYZFile;

public class XyzFile
{
    public int AtomCount { get; set; }
    public string Comment { get; set; }
    public List<Particle> Particles { get; set; }

    public XyzFile()
    {
        Particles = [];
    }

    public override string ToString()
    {
        return $"{AtomCount}\n{Comment}\n" +
               string.Join("\n", Particles.Select(atom => atom.ToString()));
    }
}
