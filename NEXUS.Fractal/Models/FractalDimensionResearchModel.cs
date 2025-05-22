using System.Collections.Generic;
using MessagePack;

namespace NEXUS.Fractal.Models;

[MessagePackObject]
public class FractalDimensionResearchModel : ResearchModel
{
    [Key(nameof(X))] public List<float> X { get; set; }
    [Key(nameof(Y))] public List<float> Y { get; set; }
    [Key(nameof(Dimension))] public float Dimension { get; set; }
}