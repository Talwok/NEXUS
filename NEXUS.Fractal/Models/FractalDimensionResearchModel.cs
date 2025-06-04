using System.Collections.Generic;
using MessagePack;
using NEXUS.Fractal.Enums;

namespace NEXUS.Fractal.Models;

[MessagePackObject]
public class FractalDimensionResearchModel : ResearchModel
{
    [Key(nameof(DimensionType))] public FractalDimensionType DimensionType { get; set; }
    [Key(nameof(X))] public List<float> X { get; set; }
    [Key(nameof(Y))] public List<float> Y { get; set; }
    [Key(nameof(LowerLimitX))] public float LowerLimitX { get; set; } = float.NaN;
    [Key(nameof(UpperLimitX))] public float UpperLimitX { get; set; } = float.NaN;
    [Key(nameof(LowerLimitY))] public float LowerLimitY { get; set; } = float.NaN;
    [Key(nameof(UpperLimitY))] public float UpperLimitY { get; set; } = float.NaN;
    [Key(nameof(MinX))] public float MinX { get; set; }
    [Key(nameof(MinY))] public float MinY { get; set; }
    [Key(nameof(MaxX))] public float MaxX { get; set; }
    [Key(nameof(MaxY))] public float MaxY { get; set; }
}